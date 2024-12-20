using Palladion.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Palladion.Editor
{
    public abstract class PalladionAssetCustomInspector : UnityEditor.Editor
    {
        private string ButtonText => $"Edit {this.target.GetType().Name} In Palladion Editor";

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int index)
        {
            object asset = EditorUtility.InstanceIDToObject(instanceID);
            
            // This method isn't meant to be working with the original PalladionAsset type as it should only be inherited.
            if (asset.GetType().BaseType == typeof(PalladionAsset))
            {
                PalladionEditorWindow.Open((PalladionAsset)asset);

                return true;
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(ButtonText))
            {
                var asset = (PalladionAsset)this.target;
                
                PalladionEditorWindow.Open(asset);
            }
        }
        
    }
}