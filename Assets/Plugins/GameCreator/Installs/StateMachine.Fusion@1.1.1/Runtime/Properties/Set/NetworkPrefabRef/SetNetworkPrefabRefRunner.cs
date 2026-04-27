using System;
using Fusion;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Inventory
{
    [Title("State Machine Runner Variable")]
    [Category("State Machine Runner Variable")]
    
    [Description("Sets the NetworkPrefabRef value on a State Machine Runner Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]

    [Serializable] [HideLabelsInEditor]
    public class SetNetworkPrefabRefRunner : PropertyTypeSetNetworkPrefabRef
    {
        [SerializeField]
        protected FieldSetStateMachineRunner m_Variable = new(ValueNetworkPrefabRef.TYPE_ID);

        public override void Set(NetworkPrefabRef value, Args args) => m_Variable.Set(value, args);
        public override NetworkPrefabRef Get(Args args) => m_Variable.Get(args) is NetworkPrefabRef ? (NetworkPrefabRef)m_Variable.Get(args) : default;

        public static PropertySetNetworkPrefabRef Create => new(
            new SetNetworkPrefabRefRunner()
        );
        
        public override string String => m_Variable.ToString();
    }
}