using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("State Machine Node")]
    [Description("A node that executes a sub state machine")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)] 
    [System.Serializable, NodeMenuItem("StateMachine Node")]
    public class StateMachineNode : BaseGameCreatorNode, ICreateNodeFrom<StateMachineAsset>, Nodes
    {
        [Input("In", true)] public StateMachinePortIn input;
        [Output("Out")] public StateMachinePortOut output;

        public StateMachineAsset stateMachine;
        public override string name => "State Machine";
        public override string layoutStyle => "GraphProcessorStyles/StateMachineNode";
        public override bool useNetwork => false;

        protected override void Process(Args customArgs)
        {
            Context = customArgs.Self;

            if(!Application.isPlaying) return;
            if (!stateMachine) return;
            if(!CanExecute(customArgs.Self)) return;

            OnStartRunning(customArgs.Self);
            var processor = new StateMachineGraphProcessor(stateMachine, customArgs.Self);
            processor.Run(customArgs.Clone, RunChildNodes);
            OnStopRunning(customArgs.Self);
        }

        public bool InitializeNodeFromObject(StateMachineAsset value)
        {
            var result = value && value != StateMachineAsset.Active;
            if (!result) return false;
            nodeCustomName = value.name;
            stateMachine = value;
            return true;
        }
    }
}