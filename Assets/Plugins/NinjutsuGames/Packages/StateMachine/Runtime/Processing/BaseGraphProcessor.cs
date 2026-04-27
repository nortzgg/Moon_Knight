using System;
using GameCreator.Runtime.Common;
using UnityEngine;

// using Unity.Entities;

namespace NinjutsuGames.StateMachine.Runtime
{
    /// <summary>
    /// Graph processor
    /// </summary>
    public abstract class BaseGraphProcessor : IDisposable
    {
        public Action<Args> OnExit;
        protected StateMachineAsset graph;
        public GameObject context;

        /// <summary>
        /// Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public BaseGraphProcessor(StateMachineAsset graph, GameObject context)
        {
            this.graph = graph;
            this.context = context;

            UpdateComputeOrder();
            
            // Subscribe to graph changes if needed
            if (graph != null)
            {
                graph.onGraphChanges += OnGraphChanges;
            }
        }
        
        protected virtual void OnGraphChanges(GraphChanges changes)
        {
            // React to graph changes if needed
        }

        public abstract void UpdateComputeOrder();

        /// <summary>
        /// Schedule the graph into the job system
        /// </summary>
        public abstract void Run(Args args, Action<Args> onFinish);

        public void RunNode(string nodeId, Args args)
        {
            graph.RunNode(nodeId, args);
        }

        public void StopNode(string nodeId, GameObject newContext)
        {
            graph.StopNode(nodeId, newContext);
        }
        
        public void DisableNode(string nodeId, GameObject newContext)
        {
            graph.DisableNode(nodeId, newContext);
        }
        
        public void EnableNode(string nodeId, GameObject newContext)
        {
            graph.EnableNode(nodeId, newContext);
        }

        public bool IsNodeEnabled(string nodeId, GameObject newContext)
        {
            return graph.IsNodeEnabled(nodeId, newContext);
        }
        
        public bool IsNodeRunning(string nodeId, GameObject newContext)
        {
            return graph.IsNodeRunning(nodeId, newContext);
        }
        
        /// <summary>
        /// Implements IDisposable to clean up resources and event subscriptions
        /// </summary>
        public virtual void Dispose()
        {
            // Unsubscribe from graph events
            if (graph != null)
            {
                graph.onGraphChanges -= OnGraphChanges;
            }

            // Release references
            graph = null;
            context = null;

            GC.SuppressFinalize(this);
        }
    }
}