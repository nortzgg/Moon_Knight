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
    
    [Description("Sets the Attribute value on a State Machine Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]

    [Serializable] [HideLabelsInEditor]
    public class SetAttributeStateMachine : PropertyTypeSetAttribute
    {
        [SerializeField]
        protected FieldSetStateMachine m_Variable = new FieldSetStateMachine(ValueAttribute.TYPE_ID);

        public override void Set(GameCreator.Runtime.Stats.Attribute value, Args args) => m_Variable.Set(value, args);
        public override GameCreator.Runtime.Stats.Attribute Get(Args args) => m_Variable.Get(args) as GameCreator.Runtime.Stats.Attribute;

        public static PropertySetAttribute Create => new PropertySetAttribute(
            new SetAttributeStateMachine()
        );
        
        public override string String => m_Variable.ToString();
    }
}