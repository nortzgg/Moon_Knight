using System;
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
    [Description("Returns the Skill value of a State Machine Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetSkillStateMachine : PropertyTypeGetSkill
    {
        [SerializeField]
        protected FieldGetStateMachine m_Variable = new FieldGetStateMachine(ValueSkill.TYPE_ID);

        public override Skill Get(Args args) => m_Variable.Get<Skill>(args);

        public override string String => m_Variable.ToString();
    }
}