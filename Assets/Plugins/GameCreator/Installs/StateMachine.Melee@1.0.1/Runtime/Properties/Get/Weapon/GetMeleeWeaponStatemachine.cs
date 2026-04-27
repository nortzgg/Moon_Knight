using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Melee
{
    [Title("Melee State Machine Variable")]
    [Category("Variables/Melee State Machine Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]
    [Description("Returns the Melee Weapon value of a State Machine Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetMeleeWeaponStateMachine : PropertyTypeGetWeapon
    {
        [SerializeField]
        protected FieldGetStateMachine m_Variable = new (ValueMeleeWeapon.TYPE_ID);

        public override IWeapon Get(Args args) => m_Variable.Get<MeleeWeapon>(args);

        public override string String => m_Variable.ToString();
    }
}