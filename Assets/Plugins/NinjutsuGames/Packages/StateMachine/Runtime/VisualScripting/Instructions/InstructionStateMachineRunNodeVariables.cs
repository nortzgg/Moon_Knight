using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("Run Runner Node with Variables")]
    [Description("Executes a State Machine node from an specific target runner with variables")]

    [Category("State Machine/Runner/Run Runner Node with Variables")]

    [Parameter(
        "Target",
        "The target GameObject that contains the State Machine Runner"
    )]

    [Parameter(
        "Node",
        "The node to execute from the specified State Machine"
    )]
    
    [Keywords("Execute", "Call", "Instruction", "Action", "State Machine", "Run")]
    [Image(typeof(IconStateMachineOverlayGreen), ColorTheme.Type.Yellow, typeof(OverlayListVariable))]
    
    [Serializable]
    public class InstructionStateMachineRunNodeVariables : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();
        [SerializeField] private RunnerVariableList m_Variables = new();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Run {m_Variables} with [{m_Variables.Length}] variables";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var runner = m_Target.Get<StateMachineRunner>(args);
            if (runner == null)
            {
                var instanceManager = m_Target.Get<StateMachineRunnerInstances>(args);
                if(instanceManager) runner = instanceManager.Get(m_Variables.StateMachine);
            }
            if (runner == null)
            {
                Debug.LogWarning($"[InstructionStateMachineRunNodeVariables] Runner not found on {m_Target}");
                return DefaultResult;
            }
            foreach (RunnerVariableItem variable in m_Variables)
            {
                runner.Set(variable.Name, variable.GetValue(args));
            }
            args.ChangeSelf(runner.gameObject);
            runner.RunNode(m_Variables.NodeGUID, args);
            return DefaultResult;
        }
    }
}