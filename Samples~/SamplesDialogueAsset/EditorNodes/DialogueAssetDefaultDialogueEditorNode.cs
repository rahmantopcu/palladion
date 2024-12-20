using System;
using Palladion.Editor;
using Palladion.Editor.Attributes;
using Palladion.Runtime;
using SamplesDialogueAsset.Nodes;
using UnityEngine.UIElements;

namespace SamplesDialogueAsset.EditorNodes
{
    // Use the PalladionEditorNode attribute to instantiate graphical representation of node types in the Palladion Graph Editor.
    [PalladionEditorNode(type: typeof(DialogueAssetDefaultDialogueNode))]
    public class DialogueAssetDefaultDialogueEditorNode : PalladionEditorNode
    {
        // Create a reference node so you can read and update data from the node linked with the editor node.
        private DialogueAssetDefaultDialogueNode _referenceNode;

        private TextField _nameField;
        private TextField _lineField;
        
        // You must create a constructor in order for the editor node to be instantiated.
        public DialogueAssetDefaultDialogueEditorNode(PalladionNode target, PalladionEditorGraphView graphView) : base(target, graphView)
        {
            _referenceNode = (DialogueAssetDefaultDialogueNode)target;
        }

        // Override the DrawNode method to customize the contents of the node.
        protected override void DrawNode()
        {
            base.DrawNode();

            var nameFoldout = new Foldout()
            {
                text = "Dialogue Name"
            };
                
            _nameField = new TextField
            {
                value = !String.IsNullOrEmpty(_referenceNode.dialogueName) ? _referenceNode.dialogueName : "Default Name",
                isDelayed = true,
                multiline = true
            };

            _nameField.RegisterValueChangedCallback(RaiseSaveCallback);
            
            nameFoldout.Add(_nameField);
            this.extensionContainer.Add(nameFoldout);

            var lineFoldout = new Foldout()
            {
                text = "Dialogue Line"
            };
            
            _lineField = new TextField
            {
                value = !String.IsNullOrEmpty(_referenceNode.dialogueLine) ? _referenceNode.dialogueLine : "Default Line",
                isDelayed = true,
                multiline = true
            };

            _lineField.RegisterValueChangedCallback(RaiseSaveCallback);
            
            lineFoldout.Add(_lineField);
            this.extensionContainer.Add(lineFoldout);
            
            this.RefreshExpandedState();
        }

        // Override the Save method to set the instructions that will be followed when the node gets saved. 
        public override void SaveNode()
        {
            _referenceNode.dialogueName = _nameField.value;
            _referenceNode.dialogueLine = _lineField.value;
        }
    }
}