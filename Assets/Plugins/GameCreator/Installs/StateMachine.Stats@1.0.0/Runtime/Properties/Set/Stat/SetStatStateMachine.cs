using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Stats
{
    [Title("State Machine Variable")]
    [Category("State Machine Variable")]
    
    [Description("Sets the Stat value on a State Machine Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]

    [Serializable] [HideLabelsInEditor]
    public class SetStatStateMachine : PropertyTypeSetStat
    {
        [SerializeField]
        protected FieldSetStateMachine m_Variable = new FieldSetStateMachine(ValueStat.TYPE_ID);

        public override void Set(Stat value, Args args) => m_Variable.Set(value, args);
        public override Stat Get(Args args) => m_Variable.Get(args) as Stat;

        public static PropertySetStat Create => new PropertySetStat(
            new SetStatStateMachine()
        );
        
        public override string String => m_Variable.ToString();
    }
}