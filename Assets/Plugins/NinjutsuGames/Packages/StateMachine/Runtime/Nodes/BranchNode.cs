using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("Branch Node")]
    [Description("A node that has conditions to check and execute a list of actions")]
    [Image(typeof(IconBranch), ColorTheme.Type.Green)] 
    [System.Serializable, NodeMenuItem("Branch Node")]
    public class BranchNode : BaseGameCreatorNode, Nodes//, ICreateNodeFrom<Actions>
    {
        [Input("In", true), Vertical] public BranchPortIn input;
        [Output("Out"), Vertical] public BranchPortOut output;

        public Branch branch = new();

        public override string name => "Branch";
        
        public override string layoutStyle => "GraphProcessorStyles/BranchNode";

        protected override void Process(Args customArgs)
        {
            Context = customArgs.Self;
            if(!Application.isPlaying) return;
            if(!CanExecute(customArgs.Self)) return;
            
            var nodeId = NodeId(customArgs.Self);
            var runner = customArgs.Self.GetCached<BranchRunner>(nodeId);
            if(!runner) return;
            if(runner.IsRunning) return;

            OnStartRunning(customArgs.Self);

            runner.Run(branch.GetCachedData(nodeId), customArgs.Clone, (result) =>
            {
                if(!Application.isPlaying) return;

                OnStopRunning(customArgs.Self, result);
                if(!result) RunChildNodes(customArgs);
            });
        }
        
        protected override void StopRunning(GameObject context)
        {
            if(!Application.isPlaying) return;
            
            var runner = context.GetCached<BranchRunner>(NodeId(context));
            if(!runner) return;

            runner.Cancel();
            OnStopRunning(context);
        }
    }
}