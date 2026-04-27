using System;
using System.Reflection;
using System.Collections.Generic;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Custom
{
    /// <summary>
    /// Custom Game Creator 2 On Melee Hit Trigger that checks what melee weapon hit the target.
    /// Only executes if the weapon that hit matches the specified weapon and the character matches (if specified).
    /// </summary>
    [Version(2, 0, 2)]

    [Title("On Melee Hit With Specific Weapon")]
    [Category("Melee/On Melee Hit With Specific Weapon")]
    [Description("Executed when the Trigger receives a hit from a melee Skill, but only if the weapon matches the specified weapon and the character matches (if specified)")]

    [Image(typeof(IconReaction), ColorTheme.Type.Red)]

    [Keywords("Melee", "Hit", "On", "Weapon", "Check", "Filter", "Character")]

    [Serializable]
    public class EventMeleeHitCheckWeapon : VisualScripting.Event
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetWeapon m_WeaponToCheck = GetWeaponMeleeInstance.Create();

        [SerializeField] private PropertyGetGameObject m_CharacterToCheck = GetGameObjectNone.Create();
        [SerializeField] private bool m_NotHitBy = false;

        [SerializeField] private bool m_RequireHitCount = false;
        [SerializeField] private int m_HitCountRequired = 2;
        [SerializeField] private float m_HitTimeWindow = 1f;

        private struct HitCounter { public int count; public float windowStart; }
        private static readonly Dictionary<int, HitCounter> s_HitCounts = new Dictionary<int, HitCounter>();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override bool RequiresCollider => true;

        // METHODS: -------------------------------------------------------------------------------

        protected override void OnReceiveCommand(Trigger trigger, CommandArgs args)
        {
            base.OnReceiveCommand(trigger, args);

            if (args.Command != EventMeleeHit.COMMAND_HIT) return;

            if (args.Target == null) return;

            Character attacker = args.Target.Get<Character>();
            if (attacker == null) return;

            Args eventArgs = new Args(attacker.gameObject, trigger.gameObject);

            GameObject characterToCheck = this.m_CharacterToCheck.Get(eventArgs);

            if (characterToCheck != null)
            {
                Character checkCharacter = characterToCheck.Get<Character>();
                if (checkCharacter == null || attacker != checkCharacter)
                {
                    return;
                }
            }

            // Get the weapon that was used to hit from the MeleeStance
            MeleeWeapon hitWeapon = GetWeaponFromCharacter(attacker);
            if (hitWeapon == null) return;

            IWeapon weaponToCheck = null;

            weaponToCheck = this.m_WeaponToCheck.Get(eventArgs);

            if (weaponToCheck == null)
            {
                Args triggerArgs = new Args(trigger.gameObject, attacker.gameObject);
                weaponToCheck = this.m_WeaponToCheck.Get(triggerArgs);
            }

            if (weaponToCheck == null)
            {
                Args attackerArgs = new Args(attacker.gameObject, attacker.gameObject);
                weaponToCheck = this.m_WeaponToCheck.Get(attackerArgs);
            }

            bool matches = false;

            if (weaponToCheck == null)
            {
                matches = true;
            }
            else
            {
                MeleeWeapon checkWeapon = weaponToCheck as MeleeWeapon;
                if (checkWeapon == null) return;
                matches = hitWeapon == checkWeapon;
            }

            if (this.m_NotHitBy) matches = !matches;

            if (matches)
            {
                if (!this.m_RequireHitCount)
                {
                    _ = trigger.Execute(args.Target);
                }
                else
                {
                    int key = trigger.GetInstanceID();
                    float now = Time.time;

                    HitCounter counter;
                    if (!s_HitCounts.TryGetValue(key, out counter))
                    {
                        counter = new HitCounter { count = 0, windowStart = now };
                    }

                    if (now - counter.windowStart > this.m_HitTimeWindow)
                    {
                        counter.count = 1;
                        counter.windowStart = now;
                    }
                    else
                    {
                        counter.count++;
                    }

                    if (counter.count >= Math.Max(1, this.m_HitCountRequired))
                    {
                        _ = trigger.Execute(args.Target);
                        counter.count = 0;
                        counter.windowStart = 0f;
                    }

                    s_HitCounts[key] = counter;
                }
            }
        }

        private static MeleeWeapon GetWeaponFromCharacter(Character character)
        {
            if (character == null || character.Combat == null) return null;

            MeleeStance meleeStance = character.Combat.RequestStance<MeleeStance>();
            if (meleeStance != null)
            {
                try
                {
                    FieldInfo attacksField = typeof(MeleeStance).GetField(
                        "m_Attacks",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );

                    if (attacksField != null)
                    {
                        Attacks attacks = attacksField.GetValue(meleeStance) as Attacks;
                        if (attacks != null && attacks.Weapon != null)
                        {
                            return attacks.Weapon;
                        }
                    }
                }
                catch
                {
                    // Reflection failed, fall through to fallback
                }
            }

            foreach (Weapon weapon in character.Combat.Weapons)
            {
                if (weapon.Asset is MeleeWeapon meleeWeapon)
                {
                    return meleeWeapon;
                }
            }

            return null;
        }
    }
}