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
    
    [Description("Sets the Stat value on a State Machine Runner Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]

    [Serializable] [HideLabelsInEditor]
    public class SetStatStateMachineRunner : PropertyTypeSetStat
    {
        [SerializeField]
        protected FieldSetStateMachineRunner m_Variable = new FieldSetStateMachineRunner(ValueStat.TYPE_ID);

        public override void Set(Stat value, Args args) => m_Variable.Set(value, args);
        public override Stat Get(Args args) => m_Variable.Get(args) as Stat;

        public static PropertySetStat Create => new PropertySetStat(
            new SetStatStateMachineRunner()
        );
        
        public override string String => m_Variable.ToString();
    }
}