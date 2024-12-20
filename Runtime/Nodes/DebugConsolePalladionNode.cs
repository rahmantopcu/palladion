using Palladion.Runtime.Attributes;

namespace Palladion.Runtime.Nodes
{
    [PalladionNode("Debug Console Node", "Debug/Debug Console Node")]
    public class DebugConsolePalladionNode : PalladionNode
    {
        public string DebugConsoleInformation;

        public DebugConsolePalladionNode()
        {
            DebugConsoleInformation = "Default Text";

        }
        
    }
}