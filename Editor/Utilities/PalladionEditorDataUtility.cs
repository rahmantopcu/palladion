using System.Linq;
using Palladion.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Palladion.Editor.Utilities
{
    public class PalladionEditorDataUtility
    {
        public readonly int InitialUndoOperation;

        private readonly SerializedObject _target;
        private PalladionAsset _asset;

        private SerializedObject _copiedTarget;
        private PalladionAsset _copiedAsset;
        
        
        public PalladionEditorDataUtility(PalladionAsset asset)
        {
            _target = new SerializedObject(asset);
            _asset = (PalladionAsset)_target.targetObject;

            // Copy References
            _copiedTarget = new SerializedObject(Object.Instantiate(asset));
            _copiedAsset = (PalladionAsset)_copiedTarget.targetObject;
            
            
            Undo.RecordObject(_target.targetObject, "Start Editing Asset");
            
            InitialUndoOperation = Undo.GetCurrentGroup();
            
            _target.Update();
        }
        
        public void AddNode(PalladionNode node)
        {
            Undo.RecordObject(_target.targetObject, "Register Node");
            EditorUtility.SetDirty(_target.targetObject);
            
            _asset.nodes.Add(node);
            
            _target.Update();
        }

        public void AddConnection(Port connectionOutputPort, Port connectionInputPort)
        {
            var outputEditorNode = (PalladionEditorNode)connectionOutputPort.node;
            var outputNode = outputEditorNode.Node;

            var inputEditorNode = (PalladionEditorNode)connectionInputPort.node;
            var inputNode = inputEditorNode.Node;

            // If the specified editor nodes don't point to nodes, skip adding the connection.
            if (outputNode == null || inputNode == null) return;

            Undo.RecordObject(_target.targetObject, "Add Connection");
            EditorUtility.SetDirty(_target.targetObject);
            
            // Add port reference for output ports that didn't have a connection before this one.
            var outputPortReference = outputNode.outputPortsConnections.FirstOrDefault(x => x.name == connectionOutputPort.name);
            if (outputPortReference == null)
            {
                var portReference = new PortConnections(connectionOutputPort.name);
                outputNode.outputPortsConnections.Add(portReference);

                outputPortReference = portReference;
            }
            
            // Add port reference for input ports that didn't have a connection before this one.
            var inputPortReference = inputNode.inputPortsConnections.FirstOrDefault(x => x.name == connectionInputPort.name);
            if (inputPortReference == null)
            {
                var portReference = new PortConnections(connectionInputPort.name);
                inputNode.inputPortsConnections.Add(portReference);

                inputPortReference = portReference;
            }

            // TODO: Check for errors after refactoring is over.
            var outputPortConnection = new PortConnection(connectionInputPort.name, inputNode.Guid);
            outputPortReference.connectionList.Add(outputPortConnection);

            var inputPortConnection = new PortConnection(connectionOutputPort.name, outputNode.Guid);
            inputPortReference.connectionList.Add(inputPortConnection);

            _target.Update();
        }
        
        public void RemoveNode(PalladionNode node)
        {
            Undo.RecordObject(_target.targetObject, "Remove Node");
            EditorUtility.SetDirty(_target.targetObject);

            _asset.nodes.Remove(node);
            
            _target.Update();
        }

        public void RemoveConnection(Port connectionOutputPort, Port connectionInputPort)
        {
            var outputEditorNode = (PalladionEditorNode)connectionOutputPort.node;
            var outputNode = outputEditorNode?.Node;

            var inputEditorNode = (PalladionEditorNode)connectionInputPort.node;
            var inputNode = inputEditorNode?.Node;
            
            Undo.RecordObject(_target.targetObject, "Remove Connection");            
            EditorUtility.SetDirty(_target.targetObject);

            
            var outputPortReference = outputNode?.outputPortsConnections.FirstOrDefault(x => x.name == connectionOutputPort.name);
            var inputPortReference = inputNode?.inputPortsConnections.FirstOrDefault(x => x.name == connectionInputPort.name);

            // TODO: Check for errors after refactoring is over.
            var outputPortConnection = new PortConnection(connectionInputPort.name, inputNode?.Guid);
            outputPortReference?.connectionList.Remove(outputPortConnection);

            var inputPortConnection = new PortConnection(connectionOutputPort.name, outputNode?.Guid);
            inputPortReference?.connectionList.Remove(inputPortConnection);

            // Remove reference of output nodes that don't have any connections left after removal.
            if (outputPortReference?.connectionList.Count == 0)
            {
                outputNode.outputPortsConnections.Remove(outputPortReference);
            }

            // Remove reference of input nodes that don't have any connections left after removal.
            if (inputPortReference?.connectionList.Count == 0)
            {
                inputNode.inputPortsConnections.Remove(inputPortReference);
            }
            
            _target.Update();
        }

        public void ReorderConnectionsAfterPortRemoval(Port removedPort)
        {
            var affectedEditorNode = (PalladionEditorNode)removedPort.node;
            
            var affectedEditorNodeDirectionConnections = removedPort.direction == Direction.Input ? affectedEditorNode.Node.inputPortsConnections : affectedEditorNode.Node.outputPortsConnections;
            var affectedEditorNodePorts = removedPort.direction == Direction.Input ? affectedEditorNode.InputPorts : affectedEditorNode.OutputPorts;

            var removedPortIndex = affectedEditorNodePorts.IndexOf(removedPort);

            Undo.RecordObject(_target.targetObject, "Reorder Connections After Port Removal");
            EditorUtility.SetDirty(_target.targetObject);
            
            // Change references in the connected nodes.
            for (int i = 0; i <= affectedEditorNodePorts.Count - 1; i++)
            {
                // If the current port is below the removed port in the list, it's references shouldn't be modified.
                if (i <= removedPortIndex) { continue; }
                
                var currentPort = affectedEditorNodePorts[i];
                var currentPortConnections = affectedEditorNodeDirectionConnections.FirstOrDefault(x => x.name == currentPort.name);
                
                // If the current port has no connections that means there is nothing to reorder about it, so skip it.
                if (currentPortConnections == null) { continue; }

                // TODO: Check for errors after refactoring is over.
                foreach (var portConnection in currentPortConnections.connectionList)
                {
                    var portInstance = affectedEditorNode.GraphView.Query<Port>().Where(x => x.name == portConnection.portName).First();
                    var portInstanceEditorNode = (PalladionEditorNode)portInstance.node;
                    
                    var portInstanceDirectionPorts = portInstance.direction == Direction.Input ? portInstanceEditorNode.Node.inputPortsConnections : portInstanceEditorNode.Node.outputPortsConnections;
                    var portInstanceReference = portInstanceDirectionPorts.FirstOrDefault(x => x.name == portInstance.name);
                    
                    // Defensive Programming Check: Warn the user if the specified port has no connection to the current port.
                    // TODO: Check for errors after refactoring is over.
                    var targetConnection = new PortConnection(currentPort.name, affectedEditorNode.Node.Guid);
                    if (portInstanceReference == null || !portInstanceReference.connectionList.Contains(targetConnection))
                    {
                        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                        Debug.LogError("The port specified in the current node either doesn't exist or doesn't contain a reference to the specified node." +
                                       "This may indicate that something has gone wrong or the value might have been manipulated by the user outside the editor. " +
                                       "Skipping updating the reference of the specified node.");
                        
                        continue;
                    }

                    // TODO: Check for errors after refactoring is over.
                    var portInstanceTargetIndex = portInstanceReference.connectionList.IndexOf(targetConnection);
                    portInstanceReference.connectionList[portInstanceTargetIndex] = new PortConnection(affectedEditorNodePorts[i - 1].name, affectedEditorNode.Node.Guid);
                }
            }

            // Change the names of ports in references to the names the node will be given in the next GeneratePortNames.
            for (int i = 0; i <= affectedEditorNodePorts.Count - 1; i++)
            {
                // If the current port is below the removed port in the list, it's references shouldn't be modified.
                if (i <= removedPortIndex) { continue; }
                
                var currentPort = affectedEditorNodePorts[i];
                var currentPortConnections = affectedEditorNodeDirectionConnections.FirstOrDefault(x => x.name == currentPort.name);
                
                // If the current port has no connections that means there is nothing to reorder about it.
                if (currentPortConnections == null) { continue; }
                
                // It's crucial to change the name last as it gets used when changing references in connected nodes.
                currentPortConnections.name = affectedEditorNodePorts[i - 1].name;
            }
            
            _target.Update();
        }
        
        public void SaveEditorNode(PalladionEditorNode node)
        {
            Undo.RecordObject(_target.targetObject,$"Save Editor Node {node.Guid}");
            EditorUtility.SetDirty(_target.targetObject);
            
            node.SaveNode();
            
            _target.Update();
        }

        public void SaveEditorWindow()
        {
            // Copy the value from the copied asset into the asset reference.
            _asset.CopyValue(_copiedAsset);
        }
        
        public void DiscardEditorWindow()
        {
            // Reset the copied target references to the ones in the asset reference.
            _copiedTarget = new SerializedObject(Object.Instantiate(_asset));
            _copiedAsset = (PalladionAsset)_copiedTarget.targetObject;
        }
        
    }
}