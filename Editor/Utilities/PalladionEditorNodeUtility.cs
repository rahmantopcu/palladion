using System;
using System.Collections.Generic;
using System.Linq;
using Palladion.Editor.Attributes;
using UnityEditor.Experimental.GraphView;

namespace Palladion.Editor.Utilities
{
    public class PalladionEditorNodeUtility
    {
        private readonly PalladionEditorGraphView _parent;

        
        public PalladionEditorNodeUtility(PalladionEditorGraphView parent)
        {
            _parent = parent;
        }

        
        // Creation
        public void CreateNode(PalladionEditorNode editorNode)
        {
            _parent.DataUtility.AddNode(editorNode.Node);
            
            DrawEditorNode(editorNode);
        }
        
        public void DrawEditorNodes()
        {
            _parent.EditorNodes = new List<PalladionEditorNode>();

            foreach (var node in _parent.GraphAsset.nodes)
            {
                var editorNodeType = PalladionEditorNodeAttribute.GetEditorNodeType(node.NodeType);
                
                // A node type that doesn't have a drawer type can't be drawn.
                if (editorNodeType == null) continue;

                var editorNodeInstance = Activator.CreateInstance(editorNodeType, node, this._parent);
                var editorNodeBase = (PalladionEditorNode)editorNodeInstance;
                
                DrawEditorNode(editorNodeBase);
            }
        }
        
        private void DrawEditorNode(PalladionEditorNode editorNode)
        {
            _parent.EditorNodes.Add(editorNode);
            _parent.AddElement(editorNode);
            
            editorNode.Initialize();
        }
        
        
        // Destruction
        public void RemoveAllEditorNodes()
        {
            var allEditorNodes = _parent.graphElements.OfType<PalladionEditorNode>();
            
            _parent.DeleteElements(allEditorNodes);
        }

        public void RemoveEditorNodes(List<PalladionEditorNode> removedNodes)
        {
            foreach (var editorNode in removedNodes)
            {
                _parent.DataUtility.RemoveNode(editorNode.Node);
                _parent.EditorNodes.Remove(editorNode);
            }
        }

        public void RemovePort(Port removedPort)
        {
            // Deleting the connections manually prompts the change callback receiver to remove connections on its next execution.
            _parent.DeleteElements(removedPort.connections);
            _parent.DataUtility.ReorderConnectionsAfterPortRemoval(removedPort);
            
            var removedPortEditorNode = (PalladionEditorNode)removedPort.node;
            _parent.DeleteElements(new List<GraphElement> {removedPort} );
            
            removedPortEditorNode.GeneratePortNames();
        }
        
        
        // Mobilization
        public void MoveEditorNodes(List<PalladionEditorNode> movedNodes)
        {
            movedNodes.ForEach(x => x.SavePosition());
        }
        
    }
}