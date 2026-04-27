using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Melee
{
	[Version(0, 0, 2)]
	[Title("Set Melee Weapon")]
	[Description("Sets a Equip Melee Weapon ")]

	[Category("Melee/Equip/Set Melee Weapon")]

    [Parameter("Set", "Where the value is set")]
    [Parameter("From", "The value that is set")]

	[Keywords("Melee", "Equip", "Variable", "Weapon")]
	[Image(typeof(IconMeleeSword), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class InstructionMeleeSetMeleeWeapon : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] 
	    private PropertySetWeapon m_Set = SetWeaponNone.Create;
        
        [SerializeField]
	    private PropertyGetWeapon m_From = new PropertyGetWeapon();

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override string Title => $"Set {this.m_Set} = {this.m_From}";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override System.Threading.Tasks.Task Run(Args args)
        {
	        MeleeWeapon value = this.m_From.Get(args) as MeleeWeapon;
            this.m_Set.Set(value, args);

            return DefaultResult;
        }
    }
}