using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Melee
{
    [Title("State Machine Variable")]
    [Category("Variables/State Machine Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]
    [Description("Returns the Shield value of a State Machine Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetShieldStateMachine : PropertyTypeGetShield
    {
        [SerializeField]
        protected FieldGetStateMachine m_Variable = new FieldGetStateMachine(ValueShield.TYPE_ID);

        public override IShield Get(Args args) => m_Variable.Get<Shield>(args);

        public override string String => m_Variable.ToString();
    }
}