using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Melee
{
    [Title("State Machine Runner Variable")]
    [Category("State Machine Runner Variable")]
    
    [Description("Sets the Shield value on a State Machine Runner Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]

    [Serializable] [HideLabelsInEditor]
    public class SetShieldStateMachineRunner : PropertyTypeSetShield
    {
        [SerializeField]
        protected FieldSetStateMachineRunner m_Variable = new FieldSetStateMachineRunner(ValueShield.TYPE_ID);

        public override void Set(IShield value, Args args) => m_Variable.Set(value, args);
        public override IShield Get(Args args) => m_Variable.Get(args) as IShield;

        public static PropertySetShield Create => new PropertySetShield(
            new SetShieldStateMachineRunner()
        );
        
        public override string String => m_Variable.ToString();
    }
}