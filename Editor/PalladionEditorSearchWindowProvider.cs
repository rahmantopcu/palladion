using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Palladion.Editor.Attributes;
using Palladion.Runtime;
using Palladion.Runtime.Attributes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Palladion.Editor
{
    public struct SearchContextElement
    {
        // The data of the asset graph node class that is being targeted.
        public PalladionNode Target { get; private set; }

        public string Title { get; private set; }
        
        // The class which will draw the targeted asset graph node.
        public Type Drawer { get; private set; }

        public SearchContextElement(PalladionNode target, string title, Type drawer)
        {
            Target = target;

            Title = title;

            Drawer = drawer;
        }
    }

    public class PalladionEditorSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        public PalladionEditorGraphView CurrentGraphView;

        public VisualElement Target;

        private static List<SearchContextElement> _elements;
        

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry> 
            {
                // Add the SearchTreeGroupEntry which will function as the mother tree in the search window.
                new SearchTreeGroupEntry(new GUIContent("Nodes")) 
            };

            _elements = PopulateElements();

            CreateElementGroups(_elements, searchTree);

            return searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition = CurrentGraphView.ChangeCoordinatesTo(CurrentGraphView, context.screenMousePosition - CurrentGraphView.Parent.position.position);
            var viewMousePosition = CurrentGraphView.contentViewContainer.WorldToLocal(windowMousePosition);

            SearchContextElement element = (SearchContextElement)searchTreeEntry.userData;
            
            // Instantiate the node and turn it into an asset graph node to use the properties of its base class.
            var nodeInstance = Activator.CreateInstance(element.Target.NodeType);
            var baseNode = (PalladionNode)nodeInstance;

            baseNode.SetPosition(new Rect(viewMousePosition, Vector2.zero));
            
            // Instantiate the editor node and turn it into an asset graph editor node to use the properties of its base class.
            var editorNodeInstance = Activator.CreateInstance(element.Drawer, nodeInstance, CurrentGraphView);
            var baseEditorNode = (PalladionEditorNode)editorNodeInstance;
            
            // CurrentGraphView.AddNode(baseEditorNode);
            CurrentGraphView.NodeUtility.CreateNode(baseEditorNode);

            return true;
        }
        
        
        private List<SearchContextElement> PopulateElements()
        {
            var searchContextElements = new List<SearchContextElement>();
            
            // Get every assembly to add extensions stored anywhere in the project.
            List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            List<Type> types = new List<Type>();
            assemblies.ForEach(x => types.AddRange(x.GetTypes()));

            // Get every class which has a node drawer attribute which points to a type that has a node attribute.  
            var nodeDrawers = types.FindAll(x => x.GetCustomAttribute<PalladionEditorNodeAttribute>() != null && x.GetCustomAttribute<PalladionEditorNodeAttribute>().TargetType.GetCustomAttribute<PalladionNodeAttribute>() != null);
            
            foreach (var drawer in nodeDrawers)
            {
                var nodeDrawerAttribute = drawer.GetCustomAttribute<PalladionEditorNodeAttribute>();

                var nodeAttribute = nodeDrawerAttribute.TargetType.GetCustomAttribute<PalladionNodeAttribute>();
                
                // If the targeted node type doesn't contain a path for the node, ignore the entry.
                if (String.IsNullOrEmpty(nodeAttribute.NodeMenuItem)) continue;

                var nodeInstance = Activator.CreateInstance(nodeDrawerAttribute.TargetType);
                var nodeBase = (PalladionNode)nodeInstance;

                var element = new SearchContextElement(nodeBase, nodeAttribute.NodeMenuItem, drawer);
                searchContextElements.Add(element);
            }

            if (searchContextElements.Count >= 1)
            {
                SortElements(searchContextElements);
            }
            
            return searchContextElements;
        }
        
        private void SortElements(List<SearchContextElement> elements)
        {
            elements.Sort((element1, element2) =>
            {
                string[] splits1 = element1.Title.Split('/');
                string[] splits2 = element2.Title.Split('/');


                for (int i = 0; i < splits1.Length; i++)
                {
                    if (i >= splits2.Length) return 1;

                    var value = String.Compare(splits1[i], splits2[i], StringComparison.Ordinal);

                    // If the compared strings are equal to each other.
                    if (value == 0) continue;
                    
                    // Make sure that splits go before nodes.
                    if (splits1.Length != splits2.Length && (i == splits1.Length || i == splits2.Length - 1)) return splits1.Length < splits2.Length ? 1 : -1;

                    return value;
                }

                return 0;
            });
        }
        
        private void CreateElementGroups(List<SearchContextElement> elements, List<SearchTreeEntry> entries)
        {
            List<string> elementGroups = new List<string>();

            foreach (var element in elements)
            {
                string[] elementTitle = element.Title.Split('/');
                
                string groupName = String.Empty;
                
                // Cycle through every part of the title to map group entry trees.
                for (int i = 0; i < elementTitle.Length - 1; i++)
                {
                    groupName += elementTitle[i];
                    
                    // If there isn't another group with the same name already, create a new group for the current element.
                    if (!elementGroups.Contains(groupName))
                    {
                        entries.Add(new SearchTreeGroupEntry(new GUIContent(elementTitle[i]), i + 1));
                        elementGroups.Add(groupName);
                    }

                    groupName += '/';
                }
                
                // Update the tree with the newly created groups.
                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(elementTitle.Last()))
                {
                    level = elementTitle.Length,
                    userData = new SearchContextElement(element.Target, element.Title, element.Drawer)
                };

                entries.Add(entry);
            }
        }

    }

    
}