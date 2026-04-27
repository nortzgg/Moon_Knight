using System;
using Fusion;
using UnityEngine;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;

namespace NinjutsuGames.StateMachine.Runtime.Fusion
{
    [Title("State Machine Runner Variable")]
    [Category("Variables/State Machine Runner Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]
    [Description("Returns the Network Prefab Ref value of a State Machine Runner Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetNetworkPrefabRefRunner : PropertyTypeGetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldGetStateMachineRunner m_Variable = new (ValueNetworkPrefabRef.TYPE_ID);

        public override NetworkPrefabRef Get(Args args) => this.m_Variable.Get<NetworkPrefabRef>(args);

        public override string String => this.m_Variable.ToString();
    }
}