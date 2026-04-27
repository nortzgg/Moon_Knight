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
    [Description("Returns the Stat value of a State Machine Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetStatStateMachine : PropertyTypeGetStat
    {
        [SerializeField]
        protected FieldGetStateMachine m_Variable = new FieldGetStateMachine(ValueStat.TYPE_ID);

        public override Stat Get(Args args) => m_Variable.Get<Stat>(args);

        public override string String => m_Variable.ToString();
    }
}