using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Runtime.Common;
using UnityEngine;

// using Unity.Entities;

namespace NinjutsuGames.StateMachine.Runtime
{
    /// <summary>
    /// Graph processor
    /// </summary>
    public class StateMachineGraphProcessor : BaseGraphProcessor
    {
        /// <summary>
        /// Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        /// <param name="context"></param>
        public StateMachineGraphProcessor(StateMachineAsset graph, GameObject context) : base(graph, context) {}

        public override void UpdateComputeOrder() {}
        
        // Track active nodes to enable proper cleanup
        private readonly HashSet<string> activeNodes = new HashSet<string>();

        /// <summary>
        /// Process all the nodes following the compute order.
        /// </summary>
        public override void Run(Args args, Action<Args> onFinish)
        {
            var initialNodes = graph.nodes.Where(n => n is TriggerNode or StartNode);
            
            var exitNode = graph.nodes.FirstOrDefault(n => n is ExitNode) as ExitNode;
            exitNode!.OnFinish -= onFinish;
            exitNode.OnFinish += onFinish;
            
            var baseNodes = initialNodes as BaseNode[] ?? initialNodes.ToArray();
            foreach (var node in baseNodes)
            {
                node.OnProcess(args);
                activeNodes.Add(node.GUID);
            }
        }
        
        protected override void OnGraphChanges(GraphChanges changes)
        {
            // Handle node removals for cleanup
            if (changes.removedNode != null)
            {
                activeNodes.Remove(changes.removedNode.GUID);
            }
        }

        public override void Dispose()
        {
            // Stop all active nodes
            foreach (var nodeId in activeNodes)
            {
                try
                {
                    StopNode(nodeId, context);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error stopping node {nodeId} during disposal: {ex.Message}");
                }
            }

            activeNodes.Clear();

            // Call base implementation
            base.Dispose();
        }
    }
}