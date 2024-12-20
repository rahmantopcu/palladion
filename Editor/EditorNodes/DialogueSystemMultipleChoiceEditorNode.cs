using System;
using System.Collections.Generic;
using System.Linq;
using Palladion.Editor.Attributes;
using Palladion.Runtime;
using Palladion.Runtime.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Palladion.Editor.EditorNodes
{
    [PalladionEditorNode(typeof(DialogueSystemMultipleChoiceNode))]
    public class DialogueSystemMultipleChoiceEditorNode : PalladionEditorNode
    {
        private readonly DialogueSystemMultipleChoiceNode _referenceNode;

        private readonly List<TextField> _choiceFields = new();
        private readonly List<string> _choices;
        
        // private Dictionary<TextField, string> m_choicesDictionary = new();

        private readonly List<VisualElement> _compartments = new();
        private Button _choiceAddButton;
        
        
        public DialogueSystemMultipleChoiceEditorNode(PalladionNode target, PalladionEditorGraphView graphView) : base(target, graphView)
        {
            _referenceNode = (DialogueSystemMultipleChoiceNode)target;

            _choices = _referenceNode.Choices;
        }

        protected override void DrawNode()
        {
            base.DrawNode();

            _choices.ForEach(DrawChoiceCompartment);
            
            _choiceAddButton = new Button
            {
                text = "Add New Choice"
            };
            
            this.mainContainer.Insert(1, _choiceAddButton);

            _choiceAddButton.clicked += AddNewChoiceCompartment;
            
            RefreshExpandedState();
        }

        private void AddNewChoiceCompartment() => DrawChoiceCompartment();
        private void DrawChoiceCompartment(string value = "Default Text")
        {
            // Create and add compartment.
            var compartment = new VisualElement();

            var foldout = new Foldout()
            {
                text = "Dialogue Choice"
            };
            var choiceField = new TextField { value = value, isDelayed = true };
            foldout.Add(choiceField);
            
            compartment.Add(foldout);
            var choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PalladionNode));
            compartment.Add(choicePort);
            
            compartment.AddToClassList("palladion-editor-node__foldout");
            
            _compartments.Add(compartment);
            this.outputContainer.Add(compartment);
            

            choiceField.RegisterValueChangedCallback(OnChoiceFieldValueChange);
            
            _choiceFields.Add(choiceField);
            
            this.GeneratePortNames();
            
            this.RaiseSaveCallBack();
            
            this.CreateStylingAfterDrawing();
            
            // Ports must be refreshed after styling to appear.
            RefreshPorts();

        }

        
        private void OnChoiceFieldValueChange(ChangeEvent<string> evt)
        {
            var targetField = (TextField)evt.target;

            var targetCompartment = _compartments.FirstOrDefault( x => x.Q<TextField>() == targetField);

            if (targetField.value == String.Empty)
            {
                _compartments.Remove(targetCompartment);
                // m_choicesDictionary.Remove(targetField);
                _choiceFields.Remove(targetField);
                
                var outputNode = targetCompartment.Q<Port>();
                this.GraphView.NodeUtility.RemovePort(outputNode);
                
                this.outputContainer.Remove(targetCompartment);
                
                this.RaiseSaveCallBack();
                
                return;
            }

            // m_choicesDictionary[targetField] = targetField.value;
            
            this.RaiseSaveCallBack();
        }

        public override void SaveNode()
        {
            var choicesList = new List<string>();
            _choiceFields.ForEach(x => choicesList.Add(x.value));

            _referenceNode.Choices = choicesList;
        }


        
    }
    
}