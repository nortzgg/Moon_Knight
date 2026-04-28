using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(0, 1, 2)]

[Title("Dash 8 Directions")]
[Description("Moves the Character in the chosen direction for a brief period of time")]

[Category("Characters/Navigation/Dash 8 Directions")]

[Parameter("Character", "The game object with the Character target")]
[Parameter("Direction", "Vector oriented towards the desired direction")]
[Parameter("Speed", "Velocity the Character moves throughout the whole movement")]
[Parameter("Damping",
    "Defines the duration and gradually changes the rate of the movement over time")]
[Parameter("Wait to Finish", "If true this Instruction waits until the dash is completed")]

[Parameter(
    "Animations",
    "Animations, identified by Forward, Backward, Right and Left, and their halves"
)]

[Keywords("Leap", "Blink", "Roll", "Flash")]
[Keywords("Character", "Player")]

[Image(typeof(IconCharacterDash), ColorTheme.Type.Blue, typeof(OverlayDot))]

[Serializable]
public class InstructionCharacterDash8Directions : Instruction
{
    private const float TRANSITION_IN = 0.1f;

    private const float TRANSITION_OUT = 0.25f;
    // MEMBERS: -------------------------------------------------------------------------------

    [SerializeField] protected PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

    [SerializeField]
    private PropertyGetDirection m_Direction = GetDirectionCharactersMoving.Create;

    [Space] [SerializeField] private PropertyGetDecimal m_Speed = new PropertyGetDecimal(10f);

    [SerializeField] private AnimationCurve m_Damping = new AnimationCurve(
        new Keyframe(0.0f, 0f, 0f, 0f),
        new Keyframe(0.2f, 1f, 0f, 0f),
        new Keyframe(1.0f, 0f, 0f, 0f)
    );

    [Space] [SerializeField] private bool m_WaitToFinish = true;

    [Space] [SerializeField] private AnimationClip m_Forward;
    [SerializeField] private AnimationClip m_ForwardRight;
    [SerializeField] private AnimationClip m_ForwardLeft;
    
    [SerializeField] private AnimationClip m_Right;
    [SerializeField] private AnimationClip m_Left;
    
    [SerializeField] private AnimationClip m_BackwardRight;
    [SerializeField] private AnimationClip m_BackwardLeft;

    [SerializeField] private AnimationClip m_Backward;

    // PROPERTIES: ----------------------------------------------------------------------------

    public override string Title => $"Dash {this.m_Character} towards {this.m_Direction}";

    // RUN METHOD: ----------------------------------------------------------------------------

    protected override async Task Run(Args args)
    {
        Character character = this.m_Character.Get<Character>(args);
        if (character == null) return;
        if (character.Busy.AreLegsBusy) return;

        Vector3 direction = this.m_Direction.Get(args);
        if (direction == Vector3.zero) direction = character.transform.forward;

        float speed = (float) this.m_Speed.Get(args);
        float duration = this.m_Damping.length > 0
            ? this.m_Damping[this.m_Damping.length - 1].time
            : 0f;

        ITweenInput tween = new TweenInput<float>(
            0f, 1f, duration,
            (a, b, t) =>
            {
                character.Motion.MoveToDirection(
                    direction.normalized * (this.m_Damping.Evaluate(t) * speed),
                    Space.World
                );
            },
            Tween.GetHash(typeof(Transform), "position"),
            Easing.Type.Linear
        );

        tween.EventFinish += isComplete =>
        {
            character.Busy.RemoveLegsBusy();
            character.Motion.StopToDirection();
        };

        character.Busy.MakeLegsBusy();
        float angle = Vector3.SignedAngle(
            direction,
            character.transform.forward,
            Vector3.up
        );

        AnimationClip animationClip = this.GetAnimationClip(angle);
        if (animationClip != null)
        {
            ConfigGesture config = new ConfigGesture(
                0f, animationClip.length, 1f, false,
                TRANSITION_IN, TRANSITION_OUT
            );

            _ = character.Gestures.CrossFade(animationClip, null, BlendMode.Blend, config,
                true);
        }

        Tween.To(character.gameObject, tween);
        if (this.m_WaitToFinish) await this.Until(() => tween.IsFinished);
    }

    private AnimationClip GetAnimationClip(float angle)
    {
        return angle switch
        {
            >= -20f and <= 20f => this.m_Forward,
            >= 20f and <= 60f => this.m_ForwardRight,
            >= 60f and <= 120f => this.m_Right,
            >= 120f and <= 160f => this.m_BackwardRight,
            >= 160f or <= -160f => this.m_Backward,
            <= -120f and >= -160f => this.m_BackwardLeft,
            <= -60f and >= -120f => this.m_Left,
            <= -20f and >= -60f => this.m_ForwardLeft,
            _ => throw new Exception($"Unable to determine direction for angle '{angle}'")
        };
    }
}