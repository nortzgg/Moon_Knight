using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Stats
{
    [Title("Clear Stat Modifier")]
    [Category("Stats/Clear Stat Modifier")]
    [Version(1, 0, 0)]

    [Image(typeof(IconStat), ColorTheme.Type.Yellow, typeof(OverlayArrowDown))]
    [Description("Clear Stat Modifier value from the selected Stat on a game object's Traits component.")]

    [Parameter("Target", "The targeted game object with a Traits component")]
    [Parameter("Stat", "The Stat that receives clearing the Modifier")]

    [Keywords("Stat", "Clear", "Remove")]

    [Serializable]
    public class InstructionClearStatModifier : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField] private PropertyGetStat m_Stat = new PropertyGetStat();
        public override string Title =>
           $"Set base for {this.m_Target}[{this.m_Stat}]";

        protected override Task Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null) return DefaultResult;

            Traits traits = target.Get<Traits>();
            if (traits == null) return DefaultResult;

            Stat stat = this.m_Stat.Get(args);
            if (stat == null) return DefaultResult;

            RuntimeStatData runtimeStat = traits.RuntimeStats.Get(stat.ID);
            if (runtimeStat == null) return DefaultResult;

            //runtimeStat.ClearModifiers();
            runtimeStat.RemoveModifier(ModifierType.Constant, runtimeStat.ModifiersValue);

            return DefaultResult;
        }
    }
}
