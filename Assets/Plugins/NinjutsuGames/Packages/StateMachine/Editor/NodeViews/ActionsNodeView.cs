using UnityEngine;
using NinjutsuGames.StateMachine.Runtime;

namespace NinjutsuGames.StateMachine.Editor
{
    [NodeCustomEditor(typeof(ActionsNode))]
    public class ActionsNodeView : BaseGameCreatorNodeView
    {
        public override Texture2D DefaultIcon => ICON_INSTRUCTION.Texture;
        public override string DefaultIconName 
        {
            get
            {
                var actionsNode = nodeTarget as ActionsNode;
                if (actionsNode?.instructions == null) return null;
                if (actionsNode.instructions.Length <= 0) return null;

                var instruction = actionsNode.instructions.Get(0);
                return instruction?.GetType().Name;
            }
        }

        public override void Enable()
        {
            base.Enable();
            
            var node = (ActionsNode) nodeTarget;
            AddCounter(node.instructions.Length);
        }
        
        public override void Update()
        {
            if (nodeTarget is ActionsNode n) UpdateCounter(n.instructions.Length);
        }
    }
}