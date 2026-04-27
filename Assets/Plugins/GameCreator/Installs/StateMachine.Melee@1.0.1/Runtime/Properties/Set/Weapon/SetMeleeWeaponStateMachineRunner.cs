using System;
using GameCreator.Runtime.Characters;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;

namespace NinjutsuGames.StateMachine.Runtime.Inventory
{
    [Title("State Machine Variable")]
    [Category("Variables/State Machine Variable")]
    
    [Description("Sets the Item value on a State Machine Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]

    [Serializable] [HideLabelsInEditor]
    public class SetItemStateMachineRunner : PropertyTypeSetWeapon
    {
        [SerializeField]
        protected FieldSetStateMachineRunner m_Variable = new(ValueMeleeWeapon.TYPE_ID);

        public override void Set(IWeapon value, Args args) => m_Variable.Set(value, args);
        public override IWeapon Get(Args args) => m_Variable.Get(args) as IWeapon;

        public static PropertySetWeapon Create => new(
            new SetMeleeWeaponGlobalList()
        );

        public override string String => m_Variable.ToString();
    }
}