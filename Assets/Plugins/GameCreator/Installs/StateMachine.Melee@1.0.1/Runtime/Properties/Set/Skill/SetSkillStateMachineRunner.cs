using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Melee
{
    [Title("State Machine Runner Variable")]
    [Category("State Machine Runner Variable")]
    
    [Description("Sets the Skill value on a State Machine Runner Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]

    [Serializable] [HideLabelsInEditor]
    public class SetSkillStateMachineRunner : PropertyTypeSetSkill
    {
        [SerializeField]
        protected FieldSetStateMachineRunner m_Variable = new FieldSetStateMachineRunner(ValueSkill.TYPE_ID);

        public override void Set(Skill value, Args args) => m_Variable.Set(value, args);
        public override Skill Get(Args args) => m_Variable.Get(args) as Skill;

        public static PropertySetSkill Create => new PropertySetSkill(
            new SetSkillStateMachineRunner()
        );
        
        public override string String => m_Variable.ToString();
    }
}