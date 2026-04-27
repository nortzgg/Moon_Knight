using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Melee
{
    [Title("State Machine Variable")]
    [Category("State Machine Variable")]
    
    [Description("Sets the Shield value on a State Machine Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]

    [Serializable] [HideLabelsInEditor]
    public class SetShieldStateMachine : PropertyTypeSetShield
    {
        [SerializeField]
        protected FieldSetStateMachine m_Variable = new FieldSetStateMachine(ValueShield.TYPE_ID);

        public override void Set(IShield value, Args args) => m_Variable.Set(value, args);
        public override IShield Get(Args args) => m_Variable.Get(args) as IShield;

        public static PropertySetShield Create => new PropertySetShield(
            new SetShieldStateMachine()
        );
        
        public override string String => m_Variable.ToString();
    }
}