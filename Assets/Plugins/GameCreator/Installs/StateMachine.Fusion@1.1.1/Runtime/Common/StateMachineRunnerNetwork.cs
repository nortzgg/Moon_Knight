using System;
using System.Collections.Generic;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using NinjutsuGames.FusionNetwork.Runtime;
using NinjutsuGames.StateMachine.Runtime;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Fusion.Runtime
{
    [AddComponentMenu("Game Creator/Fusion/State Machine Runner Network")]
    [RequireComponent(typeof(NetworkObject))]
    public class StateMachineRunnerNetwork : NetworkBehaviour, IStateAuthorityChanged
    {
        [SerializeField] private StateMachineRunner targetRunner;
        private readonly List<BaseGameCreatorNode> _networkNodes = new();
        
        [Networked, Capacity(20)]
        private NetworkDictionary<string, VariableData> NetworkVars => default;

        private bool debug = false;

        private StateMachineRunner _stateMachineRunner;
        private ChangeDetector _changeDetector;
        
        #region Variable Sync
        
        public void OnEnable()
        {
            if(!_stateMachineRunner) _stateMachineRunner = GetComponent<StateMachineRunner>();
            _stateMachineRunner.Register(OnVariableChange);
        }
        
        public void OnDisable()
        {
            _stateMachineRunner.Unregister(OnVariableChange);
            if(targetRunner) targetRunner.Unregister(OnVariableChange);
            CleanNetworkNodes();
        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            SetupRunnerIds();
            SetupNetworkNodes();

            if (HasStateAuthority) return;
            UpdateLocalVariables();
        }

        private void UpdateLocalVariables()
        {
            foreach (var networkVar in NetworkVars)
            {
                _stateMachineRunner.Set(networkVar.Key, networkVar.Value.GetValue());
            }
        }

        public override void Render()
        {
            if(HasStateAuthority) return;

            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(NetworkVars):
                        UpdateLocalVariables();
                        break;
                }
            }
        }

        private void OnVariableChange(string varId)
        {
            if(!Runner) return;
            if(!Runner.IsRunning) return;
            if(!HasStateAuthority) return;
            
            var data = _stateMachineRunner.Get(varId);
            if (!data.IsAllowedType()) return;
            
            if(debug) Debug.Log($"OnVariableChange: {varId}={data} type: {data.GetType().Name} NetworkVars: {NetworkVars.Count} allowed: {data.IsAllowedType()} hasStateAuthority: {HasStateAuthority}");

            var varData = VariableData.ConvertFromObject(data);
            NetworkVars.Set(varId, varData);
        }

        public void StateAuthorityChanged()
        {
            if(debug) Debug.LogWarning($"StateAuthorityChanged: {HasStateAuthority} state authority: {Object.StateAuthority}");
            if(!HasStateAuthority) UpdateLocalVariables();
        }
        
        #endregion

        private void SetupNetworkNodes()
        {
            targetRunner ??= GetComponent<StateMachineRunner>();
            var no = Object;
            
            // Search for network nodes in sub state machines
            foreach (var subStateMachine in targetRunner.stateMachineAsset.nodes)
            {
                if (subStateMachine is not StateMachineNode stateMachineNode) continue;
                if (stateMachineNode.stateMachine == null) continue;
                
                for (var i = 0; i < stateMachineNode.stateMachine.networkNodes.Count; i++)
                {
                    var index = i;
                    var key = stateMachineNode.stateMachine.networkNodes[i];
                    var value = stateMachineNode.stateMachine.GetNodeById(key);
                    if (value is not BaseGameCreatorNode node) continue;
                    if (node.networkingSettings.config is not FusionNodeConfig config) continue;
                    _networkNodes.Add(node);
                    config.Index = _networkNodes.IndexOf(node);
                    SetupNetworkNode(node, no, config);
                }
            }

            for (var i = 0; i < targetRunner.stateMachineAsset.networkNodes.Count; i++)
            {
                var index = i;
                var key = targetRunner.stateMachineAsset.networkNodes[i];
                var value = targetRunner.stateMachineAsset.GetNodeById(key);
                if (value is not BaseGameCreatorNode node) continue;
                if (node.networkingSettings.config is not FusionNodeConfig config) continue;
                _networkNodes.Add(node);
                config.Index = _networkNodes.IndexOf(node);
                SetupNetworkNode(node, no, config);
            }
        }

        private void SetupNetworkNode(BaseGameCreatorNode node, NetworkObject no, FusionNodeConfig config)
        {
            node.EventStartRunning += OnStartRunning(no, config);
            node.EventStopRunning += OnStopRunning(config);
        }

        private static Action<GameObject, bool> OnStopRunning(FusionNodeConfig config)
        {
            return (_, result) => { config.Sender = PlayerRef.None; };
        }

        private Action<GameObject> OnStartRunning(NetworkObject no, FusionNodeConfig config)
        {
            return _ => 
            {
                try
                {
                    if (!no.HasStateAuthority) return;
                    if (config.Sender != PlayerRef.None && !Equals(config.Sender, NetworkManager.Runner.LocalPlayer)) return;
                    RPC(config);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error sending Node RPC: {e.Message}/n{e.StackTrace}");
                }
            };
        }

        private void CleanNetworkNodes()
        {
            foreach (var node in _networkNodes)
            {
                node.EventStartRunning -= OnStartRunning(Object, null);
                node.EventStopRunning -= OnStopRunning(null);
            }
            _networkNodes.Clear();
        }

        private void SetupRunnerIds()
        {
            var runners = GetComponentsInChildren<StateMachineRunner>();
            var i = 0;
            foreach (var variable in runners)
            {
                variable.ChangeId(new IdString($"{Object.Id.Raw}-runner-{i}"));
                i++;
            }
        }
        private void RPC(FusionNodeConfig config)
        {
            switch (config.rpcTargets)
            {
                case RpcTargets.All:
                    RPC_All(config.Index);
                    break;
                case RpcTargets.Proxies:
                    RPC_Proxies(config.Index);
                    break;
                case RpcTargets.InputAuthority:
                    RPC_InputAuth(config.Index);
                    break;
                case RpcTargets.StateAuthority:
                    RPC_StateAuth(config.Index);
                    break;
                case RpcTargets.Proxies | RpcTargets.InputAuthority:
                    RPC_ProxiesInputAuth(config.Index);
                    break;
                case RpcTargets.Proxies | RpcTargets.StateAuthority:
                    RPC_ProxiesStateAuth(config.Index);
                    break;
                case RpcTargets.InputAuthority | RpcTargets.StateAuthority:
                    RPC_InputAuthStateAuth(config.Index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(config.rpcTargets), config.rpcTargets, null);
            }
        }

        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        public void RPC_All(int index, RpcInfo info = default) => RunRpc(index, info);
        
        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.Proxies, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        public void RPC_Proxies(int index, RpcInfo info = default) => RunRpc(index, info);
        
        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        public void RPC_InputAuth(int index, RpcInfo info = default) => RunRpc(index, info);
        
        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        public void RPC_StateAuth(int index, RpcInfo info = default) => RunRpc(index, info);
        
        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.Proxies | RpcTargets.InputAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        public void RPC_ProxiesInputAuth(int index, RpcInfo info = default) => RunRpc(index, info);
        
        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.Proxies | RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        public void RPC_ProxiesStateAuth(int index, RpcInfo info = default) => RunRpc(index, info);
        
        [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, Channel = RpcChannel.Reliable)]
        public void RPC_InputAuthStateAuth(int index, RpcInfo info = default) => RunRpc(index, info);
        

        private void RunRpc(int index, RpcInfo info)
        {
            if(index < 0 || index >= _networkNodes.Count) return;
            var node = _networkNodes[index];
            if (node.networkingSettings.config is FusionNodeConfig config) config.Sender = info.Source;
            var senderTag = PlayerManager.Instance.GetAvatar(info.Source);
            node.OnProcess(new Args(gameObject, senderTag.gameObject));
        }
    }
}