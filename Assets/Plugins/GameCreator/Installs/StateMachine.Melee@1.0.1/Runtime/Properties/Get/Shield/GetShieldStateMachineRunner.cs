using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Melee
{
    [Title("State Machine Runner Variable")]
    [Category("Variables/State Machine Runner Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]
    [Description("Returns the Shield value of a State Machine Runner Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetShieldStateMachineRunner : PropertyTypeGetShield
    {
        [SerializeField]
        protected FieldGetStateMachineRunner m_Variable = new FieldGetStateMachineRunner(ValueShield.TYPE_ID);

        public override IShield Get(Args args) => m_Variable.Get<Shield>(args);

        public override string String => m_Variable.ToString();
    }
}