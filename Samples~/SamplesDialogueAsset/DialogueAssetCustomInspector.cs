using Palladion.Editor;
using UnityEditor;

namespace SamplesDialogueAsset
{
    // You need to use the CustomEditor attribute to set which asset type this inspector is meant to be active on.
    [CustomEditor(typeof(DialogueAsset))]
    public class DialogueAssetCustomInspector : PalladionAssetCustomInspector
    {
        // You don't need to do anything within the class as everything is handled by the PalladionAssetCustomInspector class.
    }
}