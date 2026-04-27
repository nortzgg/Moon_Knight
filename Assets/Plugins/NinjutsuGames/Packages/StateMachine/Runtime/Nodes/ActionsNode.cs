using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("Actions Node")]
    [Description("A node that executes a list of actions")]
    [Image(typeof(IconInstructions), ColorTheme.Type.Blue)] 
    [Serializable, NodeMenuItem("Actions Node")]
    public class ActionsNode : BaseGameCreatorNode, ICreateNodeFrom<Actions>, Nodes
    {
        [Input("In", true)] public ActionPortIn input;
        [Input("In", true), Vertical] public TriggerPortIn input2;
        [Output("Out")] public ActionPortOut output;
        
        public InstructionList instructions = new();

        public override string name => "Actions";
        public override string layoutStyle => "GraphProcessorStyles/ActionsNode";

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

            runner.Run(instructions.GetCachedData(nodeId), customArgs.Clone, (args1) =>
            {
                if(!Application.isPlaying) return;
                OnStopRunning(args1.Self ? args1.Self : args1.Target);
                RunChildNodes(args1);
            });
        }
        
        public bool InitializeNodeFromObject(Actions value)
        {
#if UNITY_EDITOR
            instructions = value.GetInstructionsList().Clone();
            return instructions != null;
#else
            return true;
#endif
        }
        
        protected override void StopRunning(GameObject context)
        {
            if(!Application.isPlaying) return;
            
            var runner = context.GetCached<ActionsRunner>(NodeId(context));
            if(!runner)
            {
                Debug.LogWarning($"Runner not found on {context.name} with id {NodeId(context)}");
                return;
            }
            runner.Cancel();
            OnStopRunning(context);
        }
    }
}