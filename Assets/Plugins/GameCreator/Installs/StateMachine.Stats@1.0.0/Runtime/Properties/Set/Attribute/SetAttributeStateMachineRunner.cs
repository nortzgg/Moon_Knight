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
    
    [Description("Sets the Attribute value on a State Machine Runner Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]

    [Serializable] [HideLabelsInEditor]
    public class SetAttributeStateMachineRunner : PropertyTypeSetAttribute
    {
        [SerializeField]
        protected FieldSetStateMachineRunner m_Variable = new FieldSetStateMachineRunner(ValueAttribute.TYPE_ID);

        public override void Set(GameCreator.Runtime.Stats.Attribute value, Args args) => m_Variable.Set(value, args);
        public override GameCreator.Runtime.Stats.Attribute Get(Args args) => m_Variable.Get(args) as GameCreator.Runtime.Stats.Attribute;

        public static PropertySetAttribute Create => new PropertySetAttribute(
            new SetAttributeStateMachineRunner()
        );
        
        public override string String => m_Variable.ToString();
    }
}