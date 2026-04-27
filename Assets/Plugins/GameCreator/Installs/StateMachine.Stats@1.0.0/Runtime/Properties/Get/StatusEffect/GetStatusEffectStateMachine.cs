using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Stats
{
    [Title("State Machine Variable")]
    [Category("Variables/State Machine Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]
    [Description("Returns the Status Effect value of a State Machine Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetStatusEffectStateMachine : PropertyTypeGetStatusEffect
    {
        [SerializeField]
        protected FieldGetStateMachine m_Variable = new FieldGetStateMachine(ValueStatusEffect.TYPE_ID);

        public override StatusEffect Get(Args args) => m_Variable.Get<StatusEffect>(args);

        public override string String => m_Variable.ToString();
    }
}