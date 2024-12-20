using System;

namespace Palladion.Runtime.Attributes
{
    [Serializable]
    public class PalladionNodeAttribute : Attribute
    {
        public string NodeTitle { get; private set; }

        public string NodeMenuItem { get; private set; }


        public PalladionNodeAttribute(string title, string menuItem)
        {
            NodeTitle = title;

            NodeMenuItem = menuItem;
        }
        
    }
}