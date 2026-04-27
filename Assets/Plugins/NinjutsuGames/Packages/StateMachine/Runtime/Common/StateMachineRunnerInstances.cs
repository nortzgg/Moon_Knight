using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [AddComponentMenu("")]
    public class StateMachineRunnerInstances : Singleton<StateMachineRunnerInstances>
    {
        private readonly Dictionary<int, StateMachineRunner> _instantiated = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnSubsystemsInit()
        {
            Instance.WakeUp();
        }

        private void Start()
        {
            Instantiate();
        }

        private void Instantiate()
        {
            var lenght = StateMachineRepository.Get.AutoInstantiate.AssetList.Length;
            for (var i = 0; i < lenght; i++)
            {
                var item = StateMachineRepository.Get.AutoInstantiate.AssetList.Get(i);
                if (item is not { IsEnabled: true }) continue;
                if(item.Type == StateMachineItem.InstanceType.Asset)
                {
                    if (!item.Asset) continue;
                    var runner = StateMachineRunner.Run(item.Asset, true);
                    if (runner) Register(item.Asset, runner.Get<StateMachineRunner>());
                }
                else
                {
                    if (!item.Prefab) continue;
                    var go = Instantiate(item.Prefab);
                    var runner = go.Get<StateMachineRunner>();
                    if (runner) Register(runner.stateMachineAsset, runner);
                    DontDestroyOnLoad(go);
                }
            }
        }
        
        public void Register(StateMachineAsset asset, StateMachineRunner runner)
        {
            if(!asset || !runner) return;
            _instantiated.TryAdd(asset.UniqueID.Hash, runner);
        }
        
        public void Unregister(StateMachineAsset asset)
        {
            _instantiated.Remove(asset.UniqueID.Hash);
        }
        
        public StateMachineRunner Get(StateMachineAsset asset)
        {
            return !asset ? null : _instantiated.GetValueOrDefault(asset.UniqueID.Hash);
        }
    }
}