using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palladion.Editor.Utilities;
using Palladion.Runtime;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Assembly = System.Reflection.Assembly;
using Port = UnityEditor.Experimental.GraphView.Port;

namespace Palladion.Editor
{
    public class PalladionEditorGraphView : GraphView
    {
        public readonly PalladionEditorWindow Parent;
        public readonly PalladionAsset GraphAsset;

        public readonly PalladionEditorDataUtility DataUtility;
        public readonly PalladionEditorNodeUtility NodeUtility;
       
        public List<PalladionEditorNode> EditorNodes;

        private readonly PalladionEditorEdgeUtility _edgeUtility;
        
        private PalladionEditorSearchWindowProvider _searchWindowProvider;
        
        
        public PalladionEditorGraphView(SerializedObject target, PalladionEditorWindow parent)
        {
            Parent = parent;

            GraphAsset = (PalladionAsset)target.targetObject;

            DataUtility = new PalladionEditorDataUtility(GraphAsset);
            NodeUtility = new PalladionEditorNodeUtility(this);
            _edgeUtility = new PalladionEditorEdgeUtility(this);
            
            Initialize();

            this.graphViewChanged += OnGraphViewChange;

            Undo.undoRedoEvent += UndoRedoPerformed;
        }
        
        
        // Initialization
        private void Initialize()
        {
            AddManipulators();
            
            AddStyles();
            
            AddBackground();

            AddToolbar();
            
            RefreshGraph();
        }
        
        private void AddManipulators()
        {
            // Instantiate a DialogueGraphSearchWindowProvider and link it with node creation requests.
            _searchWindowProvider = ScriptableObject.CreateInstance<PalladionEditorSearchWindowProvider>();
            _searchWindowProvider.CurrentGraphView = this;
            
            nodeCreationRequest = ShowSearchWindow;
            
            // Instead of creating a new ContentZoomer with AddManipulator, use the existing one provided by GraphView.
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // SelectionDragger needs to be added before RectangleSelector for it to function properly.
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }
        
        private void AddStyles()
        {
            // TODO: Make this function path-independent.
            StyleSheet styleSheet = Resources.Load<StyleSheet>("AssetGraphEditorGraphView");              
            styleSheets.Add(styleSheet);
        }
        
        private void AddBackground()
        {
            var background = new GridBackground();
            background.StretchToParentSize();
            
            Insert(0, background);
        }

        private void AddToolbar()
        {
            var graphToolbar = new Toolbar
            {
                name = "asset-graph-toolbar"
            };

            Insert(2, graphToolbar);
            graphToolbar.BringToFront();

            var undoButton = new ToolbarButton(Undo.PerformUndo);
            undoButton.AddToClassList("asset-graph-editor-toolbar-button-undo");

            var redoButton = new ToolbarButton(Undo.PerformRedo);
            redoButton.AddToClassList("asset-graph-editor-toolbar-button-redo");

            var saveButton = new ToolbarButton(Undo.ClearAll);
            saveButton.AddToClassList("asset-graph-editor-toolbar-button-save");

            var discardButton = new ToolbarButton(() => Undo.RevertAllDownToGroup(DataUtility.InitialUndoOperation));
            discardButton.AddToClassList("asset-graph-editor-toolbar-button-discard");
            
            graphToolbar.Add(undoButton);
            graphToolbar.Add(redoButton);
            graphToolbar.Add(saveButton);
            graphToolbar.Add(discardButton);
        }
        
        
        // Control 
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var editorPorts = new List<Port>();
            
            EditorNodes.Where(x => x.InputPorts != null).ToList().ForEach(x=> editorPorts.AddRange(x.InputPorts));
            EditorNodes.Where(x => x.OutputPorts != null).ToList().ForEach(x => editorPorts.AddRange(x.OutputPorts));
            
            List<Port> compatiblePorts = new List<Port>(); 

            foreach (var port in editorPorts)
            {
                if (startPort.node == port.node || startPort.direction == port.direction)
                {
                    continue;
                }

                if (startPort.portType != port.portType)
                {
                    continue;
                }
                
                compatiblePorts.Add(port);
            }
            
            return compatiblePorts;
        }

        private GraphViewChange OnGraphViewChange(GraphViewChange change)
        {
            if (change.movedElements != null)
            {
                NodeUtility.MoveEditorNodes(change.movedElements.OfType<PalladionEditorNode>().ToList());
            }
            
            if (change.elementsToRemove != null)
            {
                NodeUtility.RemoveEditorNodes(change.elementsToRemove.OfType<PalladionEditorNode>().ToList());
                _edgeUtility.RemoveEdges(change.elementsToRemove.OfType<Edge>().ToList());
            }
            
            if (change.edgesToCreate != null)
            {
                _edgeUtility.CreateEdges(change.edgesToCreate);
            }
            
            return change;
        }

        private void RefreshGraph()
        {
            NodeUtility.RemoveAllEditorNodes();
            
            _edgeUtility.RemoveAllEdges();
            
            NodeUtility.DrawEditorNodes();
            
            _edgeUtility.DrawEdges();
        }

        private void ShowSearchWindow(NodeCreationContext context)
        {
            _searchWindowProvider.Target = (VisualElement)this.focusController.focusedElement;
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindowProvider);
        }

        private void UndoRedoPerformed(in UndoRedoInfo undo)
        {
            // OnGraphViewChange will make operations done in RefreshGraph Permanent if it stays subscribed.
            this.graphViewChanged -= OnGraphViewChange;
            
            RefreshGraph();

            this.graphViewChanged += OnGraphViewChange;
        }


    }
}
