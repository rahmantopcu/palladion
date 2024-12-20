using System;
using System.Collections.Generic;
using System.Reflection;
using Palladion.Runtime;
using Palladion.Runtime.Attributes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Palladion.Editor
{
    public abstract class PalladionEditorNode : Node
    {
        public readonly string Guid;

        public readonly PalladionNode Node;
        public readonly PalladionEditorGraphView GraphView;

        [HideInInspector] public List<Port> InputPorts;
        [HideInInspector] public List<Port> OutputPorts;

        // This constructor is meant to be called only by the Activator class, the constructor doesn't have to be public to be accessed by Activator.
        protected PalladionEditorNode(PalladionNode target, PalladionEditorGraphView graphView)
        {
            this.Node = target;
            this.Guid = target.Guid;

            GraphView = graphView;
            
            var nodeAttribute = target.NodeType.GetCustomAttribute<PalladionNodeAttribute>();
            CreateStylingReferences(nodeAttribute);
        }
        
        private void CreateStylingReferences(PalladionNodeAttribute attribute)
        {
            // TODO: Add a variable for the style sheet.
            AddToClassList("palladion-editor-node");
            
            this.mainContainer.AddToClassList("palladion-editor-node__main-container");
            this.extensionContainer.AddToClassList("palladion-editor-node__extension-container");
            
            if ((StyleSheet)EditorGUIUtility.Load("palladion-editor-node.uss"))
            {
                this.styleSheets.Add((StyleSheet)EditorGUIUtility.Load("palladion-editor-node.uss"));
            }
            
            
            string[] splits = attribute.NodeMenuItem.Split('/');

            foreach (var split in splits)
            {
                AddToClassList(split.ToLower().Replace(' ', '-'));
                if ((StyleSheet)EditorGUIUtility.Load($"{split.ToLower().Replace(' ', '-')}.uss"))
                {
                    // TODO: Consider doing this in another way.
                    this.styleSheets.Add((StyleSheet)EditorGUIUtility.Load($"{split.ToLower().Replace(' ', '-')}.uss"));
                }
                
            }
            
            
        }

        protected void CreateStylingAfterDrawing()
        {
            var textFields = this.Query<TextField>().ToList();
            textFields.ForEach(x => x.AddToClassList("palladion-editor-node__textfield"));

            var foldouts = this.Query<Foldout>().ToList();
            foldouts.ForEach(x => x.AddToClassList("palladion-editor-node__foldout"));

            var buttons = this.Query<Button>().ToList();
            buttons.ForEach(x => x.AddToClassList("palladion-editor-node__button"));
        }
        
        public void Initialize()
        {
            CreatePorts();

            // The port list needs to be created once before the DrawNode call as default ports will be named there.
            GeneratePortList();
            
            DrawNode();
            
            GeneratePortNames();
            
            // Styles are created after everything required to make the editor node is completed.
            CreateStylingAfterDrawing();
        }
        
        
        // Creation
        private void CreatePorts()
        {
            if (Node.UseDefaultInputPortConstructor)
            {
                CreateDefaultInputPort();
            }

            if (Node.UseDefaultOutputPortConstructor)
            {
                CreateDefaultOutputPort();
            }
        }
        
        protected virtual void CreateDefaultInputPort()
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(PalladionNode));
            this.inputContainer.Add(inputPort);
        }

        protected virtual void CreateDefaultOutputPort()
        {
            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PalladionNode));
            this.outputContainer.Add(outputPort);
        }
        
        public void GeneratePortNames()
        {
            // Refreshing the port list before generating names intervenes errors.
            GeneratePortList();
            
            // Generate port names based on their index inside the lists.
            for (int i = 0; i < InputPorts.Count; i++)
            {
                InputPorts[i].name = Guid + $"-input-{i}";
            }

            for (int i = 0; i < OutputPorts.Count; i++)
            {
                OutputPorts[i].name = Guid + $"-output-{i}";
            }
        }
        
        private void GeneratePortList()
        {
            var allPorts = this.Query<Port>().ToList();
            
            InputPorts = allPorts.FindAll(x => x.direction == Direction.Input);
            OutputPorts = allPorts.FindAll(x => x.direction == Direction.Output);
        }
        
        protected virtual void DrawNode()
        {
            this.SetPosition(Node.Position);
            
            // Draw the node title as the default set by node type.
            var attribute = Attribute.GetCustomAttribute(this.Node.NodeType, typeof(PalladionNodeAttribute));
            var nodeInformation = (PalladionNodeAttribute)attribute;
            this.title = nodeInformation.NodeTitle;
            
            RefreshPorts();
        }
        

        // Saving
        public void SavePosition()
        { 
            Node.SetPosition(this.GetPosition());
        } 
        
        protected void RaiseSaveCallBack()
        {
            GraphView.DataUtility.SaveEditorNode(this);
        }
        
        protected void RaiseSaveCallback(ChangeEvent<string> evt)
        {
            GraphView.DataUtility.SaveEditorNode(this);
        }   
        
        public virtual void SaveNode()
        {
            // Keep the saving operations of the node here.
        }
        
    }
}
