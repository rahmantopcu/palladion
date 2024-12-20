using System.Collections.Generic;
using UnityEngine;

namespace Palladion.Runtime
{
    public class PalladionAsset : ScriptableObject
    {
        public void CopyValue(PalladionAsset objectToCopy)
        {
            this.nodes = objectToCopy.nodes;
        }
        
        // SerializeReference is used because a normal SerializeField can't expose custom data from classed that inherit from AssetGraphNode.
        [SerializeReference] public List<PalladionNode> nodes = new List<PalladionNode>();
    }
}
