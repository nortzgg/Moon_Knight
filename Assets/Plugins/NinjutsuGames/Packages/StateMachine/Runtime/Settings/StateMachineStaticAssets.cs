using System;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    public class StateMachineStaticAssets
    {
        [SerializeField] private StateMachineList assetList;
        
        public StateMachineList AssetList => assetList;
    }
}