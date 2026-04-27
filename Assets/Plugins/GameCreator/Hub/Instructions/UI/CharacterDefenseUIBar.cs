using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 0, 2)]

    [Title("Character Defense UI Bar")]
    [Description("Controls an Image bar's fill amount to display a Character's current Defense. When the Character is not equipped with a Shield, sets the fill amount to 0. Otherwise, sets the fill amount to the current Defense.")]

    [Category("UI/Character Defense UI Bar")]

    [Parameter("Character", "The character target")]
    [Parameter("Defense Bar Image", "The UI bar to control the fill amount of. The image must be of type Filled.")]
    
    [Keywords("UI", "Defense", "Melee", "Block", "Combat")]
    [Image(typeof(IconUIImage), ColorTheme.Type.Blue,typeof(OverlayDot))]

    [Serializable]
    public class CharacterDefenseUIBar : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] 
        private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        
        [Space]
        [SerializeField] private Image defenseBarImage;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override string Title => $"{this.m_Character} Defense Bar UI fill";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            if (character.Combat.Block.Shield!= null){

                float currentDefense = character.Combat.CurrentDefense;
                float maxDefense = character.Combat.MaximumDefense;
                if(maxDefense !=0){
                defenseBarImage.fillAmount = currentDefense/maxDefense;
                }

            }

            else{
                defenseBarImage.fillAmount =0;
            }

            return DefaultResult;
        }
    }
}