using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("Trigger Node")]
    [Description("A node for events that trigger other nodes")]
    [Image(typeof(IconTriggers), ColorTheme.Type.Yellow)] 
    [System.Serializable, NodeMenuItem("Trigger Node")]
    public class TriggerNode : BaseGameCreatorNode, ICreateNodeFrom<Trigger>, Nodes
    {
        [Output("Out"), Vertical] public TriggerPortOut output;
        
        [SerializeReference] public Event triggerEvent = new EventOnStart();

        public override string name => "Trigger";
        
        public override string layoutStyle => "GraphProcessorStyles/TriggerNode";

        public override Color color => new Color(0.4f, 0.12f, 0.11f);

        protected override void Process(Args customArgs)
        {
            if(!CanExecute(customArgs.Self)) return;
            Context = customArgs.Self;

            var nodeId = NodeId(customArgs.Self);
            var runner = customArgs.Self.GetCached<TriggerRunner>(nodeId);
            if(!runner) return;
            runner.Setup(triggerEvent.GetCachedData(nodeId), OnTriggerExecuted, OnTriggerStopped);
        }

        private void OnTriggerStopped(Args args)
        {
            OnStopRunning(args.Self ? args.Self : args.Target);
        }

        private void OnTriggerExecuted(Args args)
        {
            var newContext = args.Self ? args.Self : args.Target;
            if(!CanExecute(newContext)) return;

            OnStartRunning(newContext);
            RunChildNodes(args);
        }
        
        public bool InitializeNodeFromObject(Trigger value)
        {
#if UNITY_EDITOR
            triggerEvent = value.GetTriggerEvent().Clone();
            return triggerEvent != null;
#else
            return true;
#endif
        }
    }
}