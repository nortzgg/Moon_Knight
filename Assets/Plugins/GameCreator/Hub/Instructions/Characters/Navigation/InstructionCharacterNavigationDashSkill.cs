using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Melee;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 2, 0)]

    [Title("Dash with Melee Skill")]
    [Description("Moves the Character in the chosen direction for a brief period of time, this dash is based on melee skill, so it won't trigger on dash trigger, beware of this when using this instruction, but you can use callbacks on skill assets")]

    [Category("Characters/Navigation/Dash with MeleeSkill")]

    [Parameter("Direction", "Vector oriented towards the desired direction")]
    [Parameter("Phase", "The Phase which waits to")]
    [Parameter("Weapon", "Weapon to use for the dash skill")]
    [Parameter("Mode", "Whether to use Cardinal Animations (4 clips for each direction) or a single one")]
    
    [Keywords("Leap", "Blink", "Roll", "Flash")]
    [Image(typeof(IconCharacterDash), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionCharacterNavigationDashSkill : TInstructionCharacterNavigation
    {
        private const int DIRECTION_KEY = 5;
        
        [Serializable]
        public struct DashSkill
        {
            public enum AnimationMode
            {
                CardinalAnimation,
                SingleAnimation
            }
         
            // EXPOSED MEMBERS: -------------------------------------------------------------------
            
            [SerializeField] private AnimationMode m_Mode;
            
            [SerializeField] private PropertyGetSkill m_SkillForward;
            [SerializeField] private PropertyGetSkill m_SkillBackward;
            [SerializeField] private PropertyGetSkill m_SkillRight;
            [SerializeField] private PropertyGetSkill m_SkillLeft;
            
            [SerializeField] private PropertyGetSkill m_Skill;
            
            // PROPERTIES: ------------------------------------------------------------------------

            public AnimationMode Mode => this.m_Mode;

            public Skill GetSkill(float angle, Args args)
            {
                return this.m_Mode switch
                {
                    AnimationMode.CardinalAnimation => angle switch
                    {
                        <= 45f and >= -45f => this.m_SkillForward.Get(args),
                        < 135f and > 45f => this.m_SkillLeft.Get(args),
                        > -135f and < -45f => this.m_SkillRight.Get(args),
                        _ => this.m_SkillBackward.Get(args)
                    },
                    AnimationMode.SingleAnimation => this.m_Skill.Get(args),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        private enum UntilPhase
        {
            FinishStrike,
            FinishRecovery,
            None
        }

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetDirection m_Direction = GetDirectionCharactersMoving.Create;
        [SerializeField] private UntilPhase m_Until = UntilPhase.FinishStrike;

        [NonSerialized] private MeleeStance m_MeleeStance;
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectNone.Create();

        [SerializeField] private PropertyGetWeapon m_Weapon = GetWeaponMeleeInstance.Create();
        [SerializeField] private DashSkill m_DashSkill;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Dash {this.m_Character} towards {this.m_Direction}";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override async Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return;
            if (character.Busy.AreLegsBusy) return;
            this.m_MeleeStance = character.Combat.RequestStance<MeleeStance>();
            if (this.m_MeleeStance == null) return;

            Vector3 direction = this.m_Direction.Get(args);
            if (direction == Vector3.zero) direction = character.transform.forward;

            character.Busy.MakeLegsBusy();
            float angle = Vector3.SignedAngle(
                direction, 
                character.transform.forward, 
                Vector3.up
            );

            Skill skill = this.m_DashSkill.GetSkill(angle, args);
            MeleeWeapon weapon = this.m_Weapon.Get(args) as MeleeWeapon;
            GameObject target = this.m_Target.Get(args);

            if (skill != null)
            {
            character.Combat
                .RequestStance<MeleeStance>()
                .PlaySkill(weapon, skill, target);
            }

            switch (this.m_Until)
            {
                case UntilPhase.FinishStrike: await this.Until(this.UntilFinishStrike); break;
                case UntilPhase.FinishRecovery: await this.Until(this.UntilFinishRecovery); break;
                case UntilPhase.None: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private bool UntilFinishStrike()
        {
            if (this.m_MeleeStance == null) return true;

            return this.m_MeleeStance.CurrentPhase switch
            {
                MeleePhase.None => true,
                MeleePhase.Reaction => true,
                MeleePhase.Charge => true,
                MeleePhase.Anticipation => false,
                MeleePhase.Strike => false,
                MeleePhase.Recovery => true,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private bool UntilFinishRecovery()
        {
            if (this.m_MeleeStance == null) return true;

            return this.m_MeleeStance.CurrentPhase switch
            {
                MeleePhase.None => true,
                MeleePhase.Reaction => true,
                MeleePhase.Charge => true,
                MeleePhase.Anticipation => false,
                MeleePhase.Strike => false,
                MeleePhase.Recovery => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}