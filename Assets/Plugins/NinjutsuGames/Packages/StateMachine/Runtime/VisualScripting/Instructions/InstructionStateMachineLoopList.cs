using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("Loop List with Node")]
    [Description("Loops a Game Object List Variables and executes a State Machine Node for each value")]
    
    [Image(typeof(IconInstructions), ColorTheme.Type.Blue, typeof(OverlayListVariable))]
    
    [Category("State Machine/Loop List with Node")]
    
    [Parameter("List Variable", "Local List or Global List which elements are iterated")]
    [Parameter(
        "Actions", 
        "The Actions component executed for each element in the list. The Target argument of " +
        "any Instruction contains the object inspected"
    )]

    [Keywords("Iterate", "Cycle", "Every", "All", "Stack")]
    [Serializable]
    public class InstructionStateMachineLoopList : Instruction
    {
        [SerializeField] 
        private CollectorListVariable m_ListVariable = new();

        [SerializeField] private PropertyGetString m_Node = GetNodeStateMachine.Create;
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Loop {m_ListVariable} with {m_Node} on {m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override Task Run(Args args)
        {
            var actionsArgs = new Args(args.Self, null);
            var source = m_ListVariable.Get(args);

            var runner = m_Target.Get<StateMachineRunner>(args);
            if (!runner) return DefaultResult;

            for (var i = 0; i < source.Count; ++i)
            {
                var gameObject = source[i] as GameObject;
                if (gameObject) actionsArgs.ChangeTarget(gameObject);
                runner.RunNode(m_Node.Get(actionsArgs), actionsArgs);
            }
            return DefaultResult;
        }
    }
}