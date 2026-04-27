using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Inventory
{
    [Title("State Machine Variable")]
    [Category("Variables/State Machine Variable")]
    
    [Description("Sets the Melee Weapon value on a State Machine Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]

    [Serializable] [HideLabelsInEditor]
    public class SetMeleeWeaponStateMachine : PropertyTypeSetWeapon
    {
        [SerializeField]
        protected FieldSetStateMachine m_Variable = new(ValueMeleeWeapon.TYPE_ID);

        public override void Set(IWeapon value, Args args) => m_Variable.Set(value, args);
        public override IWeapon Get(Args args) => m_Variable.Get(args) as IWeapon;

        public static PropertySetWeapon Create => new(
            new SetMeleeWeaponGlobalList()
        );

        public override string String => m_Variable.ToString();
    }
}