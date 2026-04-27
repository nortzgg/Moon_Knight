using System.Linq;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("Conditions Node")]
    [Description("A node that checks a list of conditions")]
    [Image(typeof(IconConditions), ColorTheme.Type.Green)] 
    [System.Serializable, NodeMenuItem("Conditions Node")]
    public class ConditionsNode : BaseGameCreatorNode, Nodes //, ICreateNodeFrom<Actions>
    {
        [Input("In", true)] public ConditionsPort input;
        [Input("In", true), Vertical] public TriggerPortIn input2;
        [Output("Out Fail")] public ConditionsPortOutFail failure;
        [Output("Out Success")] public ConditionsPortOutSuccess success;

        public CheckMode checkMode = CheckMode.And;
        public ConditionList conditions = new();

        public override string name => "Conditions";

        public override string layoutStyle => "GraphProcessorStyles/ConditionsNode";

        protected override void Process(Args customArgs = null)
        {
            if(!Application.isPlaying) return;

            Context = customArgs.Self;

            if(!CanExecute(customArgs.Self)) return;
            var nodeId = NodeId(customArgs.Self);

            if(IsContextRunning.Contains(nodeId)) return;

            OnStartRunning(customArgs.Self);
            var result = conditions.GetCachedData(nodeId).Check(customArgs.Clone, checkMode);
            var fieldName = result ? nameof(success) : nameof(failure);

            var nodes = outputPorts.FirstOrDefault(n => n.fieldName == fieldName)?.GetEdges().Where(e => e.inputNode.enabledForExecution && e.inputNode is ConditionsNode or ActionsNode or BranchNode or StateMachineNode or ExitNode)
                .Select(e => e.inputNode);
            foreach (var node in nodes)
            {
                node.OnProcess(customArgs);
            }
            OnStopRunning(customArgs.Self, result);
        }
    }
}