using Palladion.Editor.Attributes;
using Palladion.Runtime.Nodes;
using UnityEngine.UIElements;

namespace Palladion.Editor.EditorNodes
{
    [PalladionEditorNode(typeof(DebugConsolePalladionNode))]
    public class DebugConsolePalladionEditorNode : PalladionEditorNode
    {
        private DebugConsolePalladionNode _referenceNode;

        private TextField _field;
        
        public DebugConsolePalladionEditorNode(DebugConsolePalladionNode target, PalladionEditorGraphView graphView) : base(target, graphView)
        {
            _referenceNode = (DebugConsolePalladionNode)this.Node;
        }
        
        public override void SaveNode()
        {
            _referenceNode.DebugConsoleInformation = _field.value;
        }

        protected override void DrawNode()
        {
            base.DrawNode();

            /* EXTENSION CONTAINER */

            _field = new TextField
            {
                label = "Debug Console Output",
                value = _referenceNode.DebugConsoleInformation,
                isDelayed = true
            };

            this.extensionContainer.Add(_field);

            /* PORTS */
            this.InputPorts[0].portName = "Input Node";
            this.OutputPorts[0].portName = "Output Node";

            _field.RegisterValueChangedCallback(RaiseSaveCallback);
            
            RefreshExpandedState();
        }
        
    }
}