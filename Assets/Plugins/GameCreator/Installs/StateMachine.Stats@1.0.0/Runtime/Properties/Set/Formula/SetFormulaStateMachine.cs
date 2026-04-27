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
    
    [Description("Sets the Formula value on a State Machine Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]

    [Serializable] [HideLabelsInEditor]
    public class SetFormulaStateMachine : PropertyTypeSetFormula
    {
        [SerializeField]
        protected FieldSetStateMachine m_Variable = new FieldSetStateMachine(ValueFormula.TYPE_ID);

        public override void Set(Formula value, Args args) => m_Variable.Set(value, args);
        public override Formula Get(Args args) => m_Variable.Get(args) as Formula;

        public static PropertySetFormula Create => new PropertySetFormula(
            new SetFormulaStateMachine()
        );
        
        public override string String => m_Variable.ToString();
    }
}