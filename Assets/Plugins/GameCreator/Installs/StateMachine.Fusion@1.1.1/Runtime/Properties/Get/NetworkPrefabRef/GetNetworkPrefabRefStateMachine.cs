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
    [Category("Variables/State Machine Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]
    [Description("Returns the Network Prefab Ref value of a State Machine Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetNetworkPrefabRefStateMachine : PropertyTypeGetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldGetStateMachine m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override NetworkPrefabRef Get(Args args) => m_Variable.Get<NetworkPrefabRef>(args);

        public override string String => m_Variable.ToString();
    }
}