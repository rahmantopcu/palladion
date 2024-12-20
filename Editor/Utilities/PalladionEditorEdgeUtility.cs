using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Palladion.Editor.Utilities
{
    public class PalladionEditorEdgeUtility
    {
        private readonly PalladionEditorGraphView _parent;
        
        
        public PalladionEditorEdgeUtility(PalladionEditorGraphView parent)
        {
            _parent = parent;
        }
        
        
        // Creation
        public void CreateEdges(List<Edge> edges)
        {
            edges.ForEach(x => _parent.DataUtility.AddConnection(x.output, x.input));
        }

        public void DrawEdges()
        {
            // Use existing node data to draw edges.
            foreach (var editorNode in _parent.EditorNodes)
            {
                foreach (var outputPortConnectionData in editorNode.Node.outputPortsConnections)
                {
                    var outputPort = _parent.Query<Port>(outputPortConnectionData.name).First();
                    
                    // Can't draw an edge starting from a port that doesn't exist.
                    if (outputPort == null) {continue;}

                    foreach (var inputPortConnection in outputPortConnectionData.connectionList)
                    {
                        var inputPort = _parent.Query<Port>(inputPortConnection.portName).First();
                        
                        // Can't draw and edge ending at a port that doesn't exist.
                        if (inputPort == null) {continue;}
                        
                        DrawEdge(outputPort, inputPort);
                    }
                }
            }
        }
        
        private void DrawEdge(Port outputPort, Port inputPort)
        {
            var edge = outputPort.ConnectTo(inputPort);
            
            _parent.AddElement(edge);
        }
        
        
        // Destruction
        public void RemoveAllEdges()
        {
            var allEdges = _parent.graphElements.OfType<Edge>();
            
            _parent.DeleteElements(allEdges);
        }

        public void RemoveEdges(List<Edge> removedEdges)
        {
            removedEdges.ForEach(x => _parent.DataUtility.RemoveConnection(x.output, x.input));
        }
        
    }
}