using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Characters
{
    /// <summary>
    /// GC2 Condition: Character Dash Ready
    /// Version: 1.0.0
    /// 
    /// Checks if the Character can dash using GC2's native CanDash() method.
    /// Validates: Cooldown, Grounded state, Legs busy, Dashes in succession.
    /// </summary>
    [Version(1, 0, 0)]

    [Title("Character Dash Ready")]
    [Description("Returns true if the Character can dash (checks cooldown, grounded state, etc.)")]
    [Category("Characters/Navigation/Dash Ready")]
    [Keywords("Dash", "Cooldown", "Ready", "Movement")]
    [Image(typeof(IconCharacterDash), ColorTheme.Type.Green)]

    [Serializable]
    public class ConditionDashReady : Condition
    {
        [SerializeField] 
        private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        protected override string Summary => $"Can Dash {m_Character}";

        protected override bool Run(Args args)
        {
            GameObject target = m_Character.Get(args);
            if (target == null) return false;

            Character character = target.GetComponent<Character>();
            if (character == null) return false;

            // Use GC2's native CanDash() method - checks everything!
            return character.Dash.CanDash();
        }
    }
}
