using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Palladion.Editor.Attributes
{
    [Serializable]
    public class PalladionEditorNodeAttribute : Attribute
    {
        public Type TargetType { get; private set; }
        
        
        public static Type GetEditorNodeType(Type type)
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies().ToList();

            var typeList = new List<Type>();
            assemblyList.ForEach(x => typeList.AddRange(x.GetTypes()));

            var filteredTypeList = typeList.Where(x => x.GetCustomAttribute<PalladionEditorNodeAttribute>() != null).ToList();

            // Return null if there isn't a drawer for the specified type.
            return filteredTypeList.FirstOrDefault(x => x.GetCustomAttribute<PalladionEditorNodeAttribute>().TargetType == type);
        }

        public PalladionEditorNodeAttribute(Type type)
        {
            TargetType = type;
        }
        
    }
}