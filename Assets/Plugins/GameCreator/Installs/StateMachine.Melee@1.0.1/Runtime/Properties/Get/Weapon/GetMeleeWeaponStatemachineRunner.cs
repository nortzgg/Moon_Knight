using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Inventory
{
    [Title("Melee State Machine Runner Variable")]
    [Category("Variables/Melee State Machine Runner Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]
    [Description("Returns the Melee Weapon value of a State Machine Runner Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetMeleeWeaponStateMachineRunner : PropertyTypeGetWeapon
    {
        [SerializeField]
        protected FieldGetStateMachineRunner m_Variable = new (ValueMeleeWeapon.TYPE_ID);

        public override IWeapon Get(Args args) => m_Variable.Get<MeleeWeapon>(args);

        public override string String => m_Variable.ToString();
    }
}