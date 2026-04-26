using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Crouch")]
    [Category("Characters/Navigation/Crouch")]
    [Description("Changes the character's crouching state")]
    [Keywords("Toggle", "Stand" , "Crouch")]
    [Image(typeof(IconCharacterCrouch), ColorTheme.Type.Blue)]

    [Example(@"
    Use this instruction to toggle the character's crouching state using the current locomotion animator blend tree configuration.
    Since it already have all the animations transitions set up, you can just call this instruction and the character will automatically transition between Land and Stand blend trees.

    It's also possible to combine this instruction with a custom press/release input event to leave crouch when the input is release.
    ")]
    
    [Serializable]
    [Parameter("Mode", "The crouching behavior to use. Toggle will toggle between Crouch and Stand. Crouch will force the character to crouch and Stand will force the character to stand")]
    [Parameter("SmoothTime", "The smooth time until the character reaches the target stand level")]
    public class InstructionCharacterNavigationToggleCrouch : TInstructionCharacterNavigation
    {
        private enum CrouchMode
        {
            Toggle,
            Crouch,
            Stand
        }
        
        public override string Title => $"Crouch {this.m_Character}";
        
        // these values are the thresholds used by the locomotion blend tree in the animator
        private const float CROUCH_LOCOMOTION_THRESHOLD = 0.5f;
        private const float STAND_LOCOMOTION_THRESHOLD = 1f;
        
        [SerializeField] private CrouchMode m_Mode = CrouchMode.Toggle;
        [SerializeField] private float m_SmoothTime = 0.1f;
        
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            
            if (character == null) 
                return DefaultResult;

            float currentStandTarget = character.Motion.StandLevel.Target;
            
            character.Motion.StandLevel.Smooth = this.m_SmoothTime;
            
            switch (this.m_Mode)
            {
                case CrouchMode.Toggle:
                    bool isStanding = currentStandTarget >= STAND_LOCOMOTION_THRESHOLD;
                    character.Motion.StandLevel.Target = isStanding 
                        ? CROUCH_LOCOMOTION_THRESHOLD 
                        : STAND_LOCOMOTION_THRESHOLD;
                    break;
                case CrouchMode.Crouch:
                    character.Motion.StandLevel.Target = CROUCH_LOCOMOTION_THRESHOLD;
                    break;
                case CrouchMode.Stand:
                    character.Motion.StandLevel.Target = STAND_LOCOMOTION_THRESHOLD;
                    break;
            }
            
            return DefaultResult;
        }
    }
}