using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    public class StartNode : BaseGameCreatorNode
    {
        [Output("Out")] public EntryPort output;
        public override string layoutStyle => "GraphProcessorStyles/EntryNode";

        public override string name => "Start";

        public override Color color => new(0.24f, 0.15f, 0.48f);

        public override bool deletable => false;
        public override bool isRenamable => false;
        
        public InstructionList instructions = new();
        
        protected override void Process(Args customArgs)
        {
            Context = customArgs.Self;
            if(!Application.isPlaying) return;
            if(!CanExecute(customArgs.Self)) return;

            var nodeId = NodeId(customArgs.Self);
            var runner = customArgs.Self.GetCached<ActionsRunner>(nodeId);
            if(!runner) return;
            if(runner.IsRunning) return;
            OnStartRunning(customArgs.Self);

            runner.Run(instructions.Clone(), customArgs, (args1) =>
            {
                if(!Application.isPlaying) return;
                OnStopRunning(customArgs.Self);
                RunChildNodes(args1);
            });
        }
    }
}