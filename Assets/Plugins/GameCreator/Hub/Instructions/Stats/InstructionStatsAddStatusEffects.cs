using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(2, 0, 0)]
[Dependency("stats", 2, 0, 1)]
    
[Title("Add Status Effects")]
[Category("Stats/Add Status Effects")]
    
[Image(typeof(IconStatusEffect), ColorTheme.Type.Green, typeof(OverlayPlus))]
[Description("Adds a variable number of Status Effects to the selected game object's Traits component")]

[Parameter("Target", "The targeted game object with a Traits component")]
[Parameter("Amount", "The amount of Status Effects applied")]
[Parameter("Status Effect", "The type of Status Effects that are added")]
    
[Keywords("Buff", "Debuff", "Enhance", "Ailment")]
[Keywords(
    "Blind", "Dark", "Burn", "Confuse", "Dizzy", "Stagger", "Fear", "Freeze", "Paralyze", 
    "Shock", "Silence", "Sleep", "Silence", "Slow", "Toad", "Weak", "Strong", "Poison"
)]
[Keywords(
    "Haste", "Protect", "Reflect", "Regenerate", "Shell", "Armor", "Shield", "Berserk",
    "Focus", "Raise"
)]

[Serializable]
public class InstructionStatsAddStatusEffects : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Target = new PropertyGetGameObject();

    [SerializeField] private StatusEffect m_StatusEffect;
    
    [SerializeField] private PropertyGetInteger m_Amount = GetDecimalInteger.Create(1);

    public override string Title => string.Format(
        "Add {0} ({1}) to {2}",
        this.m_StatusEffect != null 
            ? this.m_StatusEffect.ID.String 
            : "(none)",
        this.m_Amount,
        this.m_Target
    );
        
    protected override Task Run(Args args)
    {
        GameObject target = this.m_Target.Get(args);
        if (target == null) return DefaultResult;

        Traits traits = target.Get<Traits>();
        if (traits == null) return DefaultResult;
            
        if (this.m_StatusEffect == null) return DefaultResult;

        int amount = (int) this.m_Amount.Get(args);
        for (int i = 0; i < amount; ++i)
        {
            traits.RuntimeStatusEffects.Add(this.m_StatusEffect);
        }
        
        return DefaultResult;
    }
}
