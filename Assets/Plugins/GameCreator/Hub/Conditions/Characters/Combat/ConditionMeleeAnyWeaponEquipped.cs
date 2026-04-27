using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 2)]
[Title("Has Any Weapon Equipped")]
[Description("Returns true if the Character has any type of Weapon equipped")]

[Category("Characters/Combat/Has Any Weapon Equipped")]
[Parameter("Character", "The targeted Character")]

[Keywords("Combat", "Melee")]
[Image(typeof(IconTennis), ColorTheme.Type.Blue)]

[Serializable]
public class ConditionMeleeAnyWeaponEquipped : Condition
{
    // MEMBERS: -----------------------------------------------------------------------------------
        
    [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

    // PROPERTIES: --------------------------------------------------------------------------------
        
    protected override string Summary => $"has {this.m_Character} Weapon equipped";
        
    // RUN METHOD: --------------------------------------------------------------------------------

    protected override bool Run(Args args)
    {
        Character character = this.m_Character.Get<Character>(args);
        return character != null && character.Combat.Weapons.Length > 0;
    }
}
