using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Stats
{
    [Title("State Machine Runner Variable")]
    [Category("State Machine Runner Variable")]
    
    [Description("Sets the Status Effect value on a State Machine Runner Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]

    [Serializable] [HideLabelsInEditor]
    public class SetStatusEffectStateMachineRunner : PropertyTypeSetStatusEffect
    {
        [SerializeField]
        protected FieldSetStateMachineRunner m_Variable = new FieldSetStateMachineRunner(ValueStatusEffect.TYPE_ID);

        public override void Set(StatusEffect value, Args args) => m_Variable.Set(value, args);
        public override StatusEffect Get(Args args) => m_Variable.Get(args) as StatusEffect;

        public static PropertySetStatusEffect Create => new PropertySetStatusEffect(
            new SetStatusEffectStateMachineRunner()
        );
        
        public override string String => m_Variable.ToString();
    }
}