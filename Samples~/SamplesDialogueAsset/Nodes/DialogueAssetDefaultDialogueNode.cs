using Palladion.Runtime;
using Palladion.Runtime.Attributes;

namespace SamplesDialogueAsset.Nodes
{
    // Use the PalladionNode attribute to make your node type exposed in the Palladion Graph Editor.
    [PalladionNode(title:"Default Dialogue Node", menuItem:"Samples/Dialogue Asset/Default Dialogue Node")]
    public class DialogueAssetDefaultDialogueNode : PalladionNode
    {
        // Declare variables inside the class to keep custom data.
        public string dialogueName;
        public string dialogueLine;
    }
}