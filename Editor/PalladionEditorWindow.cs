using System;
using System.Linq;
using Palladion.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Palladion.Editor
{
    public class PalladionEditorWindow : EditorWindow
    {
        
        private SerializedObject _target;
        private PalladionAsset _asset;
        
        private PalladionEditorGraphView _graphEditor;

        
        private void OnEnable()
        {
            if (_asset == null)
            {
                return;
            }
            
            // Refreshes serialized object and redraws graph view after each recompilation.
            DrawWindow();
        }

        
        public static void Open(PalladionAsset target)
        {
            // If there is another window editing the same asset, focus on that window instead of creating a new one.
            var windowList = Resources.FindObjectsOfTypeAll<PalladionEditorWindow>().ToList();
            if (windowList.Find(x => x._asset == target))
            {
                PalladionEditorWindow match = windowList.Find(x => x._asset == target);
                
                match.Focus();
            }
            else
            {
                PalladionEditorWindow window = CreateWindow<PalladionEditorWindow>(typeof(PalladionEditorWindow), typeof(SceneView));
                
                window.titleContent = new GUIContent($"{target.name}", EditorGUIUtility.ObjectContent(null, typeof(PalladionAsset)).image);
                window.Initialize(target);
            }
        }
        
        private void Initialize(PalladionAsset target)
        {
            _asset = target;
            
            DrawWindow();
        }

        private void DrawWindow()
        {
            _target = new SerializedObject(_asset);
            
            AddGraphView();
            
            AddStyles();
        }

        private void AddGraphView()
        {
            _graphEditor = new PalladionEditorGraphView(_target, this);
            
            _graphEditor.StretchToParentSize();
            
            this.rootVisualElement.Add(_graphEditor);
        }
        
        private void AddStyles()
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>("AssetGraphEditorVariables");
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void OnGUI()
        {
            this.hasUnsavedChanges = EditorUtility.IsDirty(_asset);
        }
        
        public override void SaveChanges()
        {
            _graphEditor.DataUtility.SaveEditorWindow();
        }

        public override void DiscardChanges()
        {
            _graphEditor.DataUtility.DiscardEditorWindow();
        }

    }
}