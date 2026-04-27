using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
#if UNITY_EDITOR
using GameCreator.Editor.Installs;
#endif

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    public class BaseGameCreatorNode : BaseNode, IDisposable
    {
        public override bool isRenamable => true;
        public override bool needsInspector => true;
        public override bool hideControls => false;
        public override bool showControlsOnHover => false;
        public virtual bool useNetwork => 
#if UNITY_EDITOR
            GameCreator.Editor.Installs.InstallManager.IsInstalled("StateMachine.Fusion") || 
            GameCreator.Editor.Installs.InstallManager.IsInstalled("StateMachine.Photon");
#else
            false;
#endif
        
        [SerializeReference] public NetworkingSettings networkingSettings;

        public event Action<GameObject> EventStartRunning;
        public event Action<GameObject, bool> EventStopRunning;
        public event ProcessDelegate OnExecutionDisabled;
        public event ProcessDelegate OnExecutionEnabled;
        
        protected Args GetArgs(GameObject fallbackTarget)
        {
            return new Args(fallbackTarget);
        }

        protected void RunChildNodes(Args args)
        {
            var nodes = GetOutputNodes();
            foreach (var baseNode in nodes)
            {
                var node = (BaseGameCreatorNode) baseNode;
                if (!node.CanExecute(args.Self)) continue;
                node.OnProcess(args);
            }
        }

        protected void OnStartRunning(GameObject currentContext)
        {
            var id = NodeId(currentContext);
            if (IsContextRunning.Contains(id)) return;
            IsContextRunning.Add(id);
            EventStartRunning?.Invoke(currentContext);
        }

        protected void OnStopRunning(GameObject currentContext, bool runResult = true)
        {
            var id = NodeId(currentContext);
            if (!IsContextRunning.Contains(id)) return;
            IsContextRunning.Remove(id);
            EventStopRunning?.Invoke(currentContext, runResult);
        }

        public void Reset()
        {
            IsContextRunning.Clear();
        }

        protected override void Enable()
        {
            base.Enable();
            IsContextRunning.Clear();
        }

        protected override void Disable()
        {
            base.Disable();
            IsContextRunning.Clear();
        }

        public void Stop(GameObject context)
        {
            StopRunning(context);
        }
        
        public void Disable(GameObject context)
        {
            if(context)
            {
                var id = NodeId(context);
                if(!IsContextDisabled.Contains(id))
                {
                    IsContextDisabled.Add(id);
                }
            }
            else enabledForExecution = false;
            
            OnExecutionDisabled?.Invoke();
        }
        
        public void Enable(GameObject context)
        {
            if(context)
            {
                var id = NodeId(context);
                if(IsContextDisabled.Contains(id)) 
                {
                    IsContextDisabled.Remove(id);
                }
            }
            else enabledForExecution = true;

            OnExecutionEnabled?.Invoke();
        }

        protected virtual void StopRunning(GameObject context) {}

        public bool IsRunning(GameObject context)
        {
            var canExecute = !context ? enabledForExecution : IsContextDisabled.Contains(NodeId(context));
            return canExecute && IsContextRunning.Contains(NodeId(!context ? Context : context));
        }

        // PLAY FUNCTIONALITY: -------------------------------------------------------------------
        
        /// <summary>
        /// Invokes the node execution similar to Actions.Invoke()
        /// </summary>
        /// <param name="self">The GameObject context for execution</param>
        public virtual void Invoke(GameObject self = null)
        {
            GameObject context = self != null ? self : this.Context;
            if (context == null)
            {
                Debug.LogWarning($"No context available for node {GetCustomName()}. Node execution skipped.");
                return;
            }
            
            Args args = new Args(context, context);
            // Execute directly without async to avoid graph dependency issues
            OnProcess(args);
        }

        /// <summary>
        /// Runs the node asynchronously
        /// </summary>
        public async Task Run()
        {
            try
            {
                Args args = new Args(this.Context, this.Context);
                await this.RunAsync(args);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.ToString(), this.Context);
            }
        }

        /// <summary>
        /// Runs the node asynchronously with custom args
        /// </summary>
        /// <param name="args">Arguments for execution</param>
        public async Task Run(Args args)
        {
            try
            {
                await this.RunAsync(args);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.ToString(), args.Self);
            }
        }
        
        /// <summary>
        /// Cancels the node execution
        /// </summary>
        public virtual void Cancel()
        {
            if (Context != null)
            {
                StopRunning(Context);
            }
        }

        /// <summary>
        /// Override this method to implement async execution logic
        /// </summary>
        /// <param name="args">Arguments for execution</param>
        protected virtual async Task RunAsync(Args args)
        {
            // Default implementation: process this node and run child nodes
            OnProcess(args);
            await Task.Yield(); // Allow other operations to continue
        }

        /// <summary>
        /// Implements IDisposable to provide better resource management.
        /// Cleans up event handlers and context tracking collections.
        /// </summary>
        public virtual void Dispose()
        {
            // Clean up event handlers
            EventStartRunning = null;
            EventStopRunning = null;
            OnExecutionDisabled = null;
            OnExecutionEnabled = null;

            // Clear collections
            IsContextRunning.Clear();
            IsContextDisabled.Clear();

            // Special case for networked nodes
            if (networkingSettings != null)
            {
                // Clean up any network-specific resources
            }
        }
    }
}