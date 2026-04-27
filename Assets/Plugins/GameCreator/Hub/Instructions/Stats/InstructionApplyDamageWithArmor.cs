using System;
using System.Threading.Tasks;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Stats;

[Version(1, 0, 0)]

[Title("Apply Damage (Armor Priority)")]
[Description(
    "Applies damage to a target. If the target has Armor, damage is absorbed by Armor first. " +
    "Remaining damage overflows to Health. If the target has no Armor attribute, all damage goes directly to Health."
)]

[Category("Stats/Apply Damage (Armor Priority)")]

[Parameter("Target", "The GameObject receiving damage (must have a Traits component)")]
[Parameter("Damage", "The amount of damage to apply")]
[Parameter("Armor Attribute", "The Armor attribute to reduce first (skipped if target doesn't have it)")]
[Parameter("Health Attribute", "The Health attribute to reduce with remaining damage")]

[Keywords("Damage", "Armor", "Health", "Shield", "Absorb", "Reduce", "Stats", "Combat")]
[Image(typeof(IconAttr), ColorTheme.Type.Red)]

[Serializable]
public class InstructionApplyDamageWithArmor : Instruction
{
    // EXPOSED MEMBERS: -----------------------------------------------------------------------

    [Header("Target")]
    [Tooltip("The GameObject receiving damage (must have a Traits component)")]
    [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();

    [Header("Damage")]
    [Tooltip("The amount of damage to apply")]
    [SerializeField] private PropertyGetDecimal m_Damage = new PropertyGetDecimal(10);

    [Header("Attributes")]
    [Tooltip("The Armor attribute (damage is absorbed here first)")]
    [SerializeField] private PropertyGetAttribute m_ArmorAttribute = new PropertyGetAttribute();

    [Tooltip("The Health attribute (receives remaining damage)")]
    [SerializeField] private PropertyGetAttribute m_HealthAttribute = new PropertyGetAttribute();

    // PROPERTIES: ----------------------------------------------------------------------------

    public override string Title => $"Damage {m_Target} ({m_Damage}) [Armor → Health]";

    // RUN: -----------------------------------------------------------------------------------

    protected override Task Run(Args args)
    {
        GameObject targetObj = m_Target.Get(args);
        if (targetObj == null) return DefaultResult;

        Traits traits = targetObj.GetComponent<Traits>();
        if (traits == null)
        {
            traits = targetObj.GetComponentInChildren<Traits>();
            if (traits == null)
            {
                traits = targetObj.GetComponentInParent<Traits>();
                if (traits == null) return DefaultResult;
            }
        }

        double damage = m_Damage.Get(args);
        if (damage <= 0) return DefaultResult;

        GameCreator.Runtime.Stats.Attribute armorAttr = m_ArmorAttribute.Get(args);
        GameCreator.Runtime.Stats.Attribute healthAttr = m_HealthAttribute.Get(args);

        if (healthAttr == null) return DefaultResult;

        // Try to absorb damage with Armor first
        double remainingDamage = damage;

        if (armorAttr != null)
        {
            RuntimeAttributeData armorData = GetAttributeSafe(traits, armorAttr.ID);

            if (armorData != null && armorData.Value > 0)
            {
                double armorBefore = armorData.Value;
                double absorbed = Math.Min(armorBefore, remainingDamage);

                armorData.Value = armorBefore - absorbed;
                remainingDamage -= absorbed;
            }
        }

        // Apply remaining damage to Health
        if (remainingDamage > 0)
        {
            RuntimeAttributeData healthData = GetAttributeSafe(traits, healthAttr.ID);

            if (healthData != null)
            {
                healthData.Value -= remainingDamage;
            }
        }

        return DefaultResult;
    }

    // PRIVATE METHODS: -------------------------------------------------------------------

    private static RuntimeAttributeData GetAttributeSafe(Traits traits, IdString attributeID)
    {
        try
        {
            return traits.RuntimeAttributes.Get(attributeID);
        }
        catch
        {
            return null;
        }
    }
}