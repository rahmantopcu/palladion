using System;
using System.Collections.Generic;
using UnityEngine;

namespace Palladion.Runtime
{
    [Serializable]
    public class PortConnections
    {
        public string name;
     
        // TODO: Add a reference to each connection to show which node it leads to rather than just the port name.
        public List<PortConnection> connectionList;
        
        public PortConnections(string name)
        {
            this.name = name;
            
            connectionList = new List<PortConnection>();
        }
    }

    [Serializable]
    public record PortConnection
    {
        // Use record instead of class to implement value equality.
        
        public string portName;
        public string nodeGuid;

        public PortConnection(string portName, string nodeGuid)
        {
            this.portName = portName;
            this.nodeGuid = nodeGuid;
        }
    }
    
    [Serializable]
    public abstract class PalladionNode
    {
        public Type NodeType => GetType();
        
        public string Guid => guid; 
        
        public Rect Position => position;
        
        [SerializeField] 
        private string guid = System.Guid.NewGuid().ToString();
        [SerializeField]
        private Rect position;
        
        public bool UseDefaultInputPortConstructor { get; protected set; } = true;
        public bool UseDefaultOutputPortConstructor { get; protected set; } = true;
        
        [SerializeReference]
        public List<PortConnections> inputPortsConnections = new List<PortConnections>();
        [SerializeReference]
        public List<PortConnections> outputPortsConnections = new List<PortConnections>();
        
        
        public void SetPosition(Rect rectangularPosition)
        {
            position = rectangularPosition;
        }

    }
}