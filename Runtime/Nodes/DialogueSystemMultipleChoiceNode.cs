using System.Collections.Generic;
using Palladion.Runtime.Attributes;

namespace Palladion.Runtime.Nodes
{
    [PalladionNode("Multiple Choice Node", "Dialogue System/ Multiple Choice Node")]
    public class DialogueSystemMultipleChoiceNode : PalladionNode
    {
        public List<string> Choices;

        public DialogueSystemMultipleChoiceNode()
        {
            Choices = new List<string>();

            this.UseDefaultOutputPortConstructor = false;
        }
        
    }
}