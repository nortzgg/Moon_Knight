using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Change Acceleration and Deceleration")]
    [Description("Changes the Character's Acceleration or Deceleration over time")]

    [Category("Characters/Properties/Change Acceleration and Deceleration")]
    
    [Parameter("Acceleration", "The target Acceleration value for the Character")]
    [Parameter("Deceleration", "The target Deceleration value for the Character")]
    [Parameter("Duration", "How long it takes to perform the transition")]
    [Parameter("Easing", "The change rate of the parameter over time")]
    [Parameter("Wait to Complete", "Whether to wait until the transition is finished")]

    [Keywords("Acceleration", "Walk", "Run", "Jog", "Sprint", "Velocity", "Deceleration")]
    [Image(typeof(IconBust), ColorTheme.Type.Yellow)]

    [Serializable]
	public class InstructionCharacterPropertyAccelerationDeceleration : TInstructionCharacterProperty
    {
        // MEMBERS: -------------------------------------------------------------------------------
	    private enum AccDec
	    {
	    	Acceleration,
	    	Deceleration
	    }
	    [SerializeField] private AccDec m_Property;
	    
	     [SerializeField] private ChangeDecimal m_PropertyValue = new ChangeDecimal(10f);
        [SerializeField] private Transition m_Transition = new Transition();
	    
        // PROPERTIES: ----------------------------------------------------------------------------

	    public override string Title => $"Change {this.m_Character} Acceleration or Deceleration";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override async Task Run(Args args)
        {
	        Character character = this.m_Character.Get<Character>(args);
	        
            if (character == null) return;
	        ITweenInput tween;
            
         if(m_Property == AccDec.Acceleration)
         {
         	float valueAcc = character.Motion.Acceleration;
	         float valueTargetAcc = (float) this.m_PropertyValue.Get(valueAcc, args);
         	
            tween = new TweenInput<float>(
	            valueAcc,
	            valueTargetAcc,
                this.m_Transition.Duration,
                (a, b, t) => character.Motion.Acceleration = Mathf.Lerp(a, b, t),
                Tween.GetHash(typeof(Character), "property:linear-speed"),
                this.m_Transition.EasingType,
                this.m_Transition.Time
            );
	         Tween.To(character.gameObject, tween);
	         if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);
         }
         
         else if(m_Property == AccDec.Deceleration)
         {
	         float valueDec = character.Motion.Deceleration;
	         float valueTargetDec = (float) this.m_PropertyValue.Get(valueDec, args);
	         
	         tween = new TweenInput<float>(
		        valueDec ,
		        valueTargetDec,
		        this.m_Transition.Duration,
		        (a, b, t) => character.Motion.Deceleration = Mathf.Lerp(a, b, t),
		        Tween.GetHash(typeof(Character), "property:linear-speed"),
		        this.m_Transition.EasingType,
		        this.m_Transition.Time
	          );
	         Tween.To(character.gameObject, tween);
	         if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);
         }
            
        }
    }
}