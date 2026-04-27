using System;
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
    [Description("Returns the Skill value of a State Machine Runner Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetSkillStateMachineRunner : PropertyTypeGetSkill
    {
        [SerializeField]
        protected FieldGetStateMachineRunner m_Variable = new FieldGetStateMachineRunner(ValueSkill.TYPE_ID);

        public override Skill Get(Args args) => m_Variable.Get<Skill>(args);

        public override string String => m_Variable.ToString();
    }
}