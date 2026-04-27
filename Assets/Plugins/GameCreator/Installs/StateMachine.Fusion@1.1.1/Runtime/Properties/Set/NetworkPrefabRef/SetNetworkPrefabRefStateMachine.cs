using System;
using Fusion;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Fusion
{
    [Title("State Machine Variable")]
    [Category("State Machine Variable")]
    
    [Description("Sets the NetworkPrefabRef value on a State Machine Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]

    [Serializable] [HideLabelsInEditor]
    public class SetNetworkPrefabRefStateMachine : PropertyTypeSetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldSetStateMachine m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override void Set(NetworkPrefabRef value, Args args) => m_Variable.Set(value, args);
        public override NetworkPrefabRef Get(Args args) => m_Variable.Get(args) is NetworkPrefabRef ? (NetworkPrefabRef)m_Variable.Get(args) : default;

        public static PropertySetNetworkPrefabRef Create => new(
            new SetNetworkPrefabRefStateMachine()
        );
        
        public override string String => m_Variable.ToString();
    }
}