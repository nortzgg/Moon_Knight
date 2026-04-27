using System;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    [Description("A state machine item.")]
    [Title("State Machine")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]
    public class StateMachineItem : TPolymorphicItem<StateMachineItem>
    {
        public enum InstanceType
        {
            Asset,
            Prefab
        }
        [SerializeField] private InstanceType m_InstanceType;
        [SerializeField] private PropertyGetGameObject m_Prefab = GetGameObjectInstance.Create();
        [SerializeField] private StateMachineAsset m_Asset;
        
        public StateMachineAsset Asset => m_Asset;
        public InstanceType Type => m_InstanceType;
        public GameObject Prefab => m_Prefab.Get(Args.EMPTY);

        public override string Title
        {
            get
            {
                if (m_InstanceType == InstanceType.Asset)
                {
                    return $"{(m_Asset == null ? "(none)" : m_Asset.name)}";
                }
                return $"{(m_Prefab == null ? "(none)" : m_Prefab.ToString())}";
            }
        }
    }
}