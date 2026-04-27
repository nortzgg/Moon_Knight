using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GVL.GameCreator.Runtime.Melee
{
    [Version(2, 0, 3)]
    
    [Title("Play Melee Reaction By World Direction")]
    [Description("Plays a Melee Reaction on a Character")]

    [Category("Melee/Skills/Play Melee Reaction By World Direction")]
    
    [Parameter("Character", "The Character that plays the Melee Reaction")]
    [Parameter("Attacker", "The Character set as the attacker")]
    [Parameter("Reaction", "The Melee Reaction asset played. If it is null, than use the caracter's default reaction")]

    [Keywords("Melee", "Combat")]
    [Image(typeof(IconReaction), ColorTheme.Type.Yellow)]
    
    [Serializable]
    public class InstructionMeleeReactByWorldDirection : Instruction
    { 
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField] private PropertyGetGameObject m_Attacker = GetGameObjectNone.Create();

        [SerializeField] private Reaction m_Reaction;

        [SerializeField] private PropertyGetDirection m_Direction = GetDirectionConstantBackward.Create;
        [SerializeField] private PropertyGetDecimal m_Force = GetDecimalConstantZero.Create;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => string.Format(
            "React {0} with {1}", 
            this.m_Character, 
            this.m_Reaction != null ? TextUtils.Humanize(this.m_Reaction.name) : "its reaction"
        );

        // RUN METHOD: ----------------------------------------------------------------------------
        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            Reaction reaction = this.m_Reaction;

            if (reaction == null)
                reaction = character.Animim.Reaction;
            
            if(reaction == null) return DefaultResult;

            GameObject attacker = this.m_Attacker.Get(args);
            Vector3 localDirection = character.transform.InverseTransformDirection(this.m_Direction.Get(args));
            
            ReactionInput input = new ReactionInput(
                localDirection,
                (float) this.m_Force.Get(args)
            );

            character.Combat
                .RequestStance<MeleeStance>()
                .PlayReaction(attacker, input, reaction,false);
            
            return DefaultResult;
        }
    }
}

