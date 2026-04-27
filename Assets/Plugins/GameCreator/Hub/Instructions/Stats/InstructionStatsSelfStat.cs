using System;
using System.Reflection;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GVL.GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 0)]

    [Title("Set Global Stat Base")]
    [Description("Overwrites a Stat base value globally so new spawned Traits use it")]
    [Category("Stats/Set Global Stat Base")]
    [Image(typeof(IconStat), ColorTheme.Type.Red)]

    [Parameter("Stat", "The Stat to overwrite")]
    [Parameter("Value", "The new base value")]
    [Parameter("Apply Current", "If enabled, currently existing Traits get updated too")]
    [Parameter("Tag", "When Apply Current is enabled, filters by Tag. Leave empty for all")]

    [Serializable]
    public class InstructionStatsSelfStat : Instruction
    {
        [SerializeField] private PropertyGetStat m_Stat = new PropertyGetStat();
        [SerializeField] private PropertyGetDecimal m_Value = new PropertyGetDecimal(0f);
        [SerializeField] private bool m_ApplyCurrent = true;
        [SerializeField] private PropertyGetString m_Tag = new PropertyGetString("Enemy");

        public override string Title => $"Global[{this.m_Stat}] = {this.m_Value}";

        protected override Task Run(Args args)
        {
            Stat stat = this.m_Stat.Get(args);
            if (stat == null) return DefaultResult;

            double value = this.m_Value.Get(args);

            FieldInfo fieldData = typeof(Stat).GetField("m_Data", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldData == null) return DefaultResult;

            object statData = fieldData.GetValue(stat);
            if (statData == null) return DefaultResult;

            FieldInfo fieldBase = statData.GetType().GetField("m_Base", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldBase == null) return DefaultResult;

            fieldBase.SetValue(statData, value);

            if (!this.m_ApplyCurrent) return DefaultResult;

            float currentValue = (float)value;
            string tagFilter = this.m_Tag.Get(args);
            bool useTagFilter = !string.IsNullOrEmpty(tagFilter);
            Traits[] allTraits = UnityEngine.Object.FindObjectsOfType<Traits>(true);

            for (int i = 0; i < allTraits.Length; ++i)
            {
                if (useTagFilter && allTraits[i].gameObject.tag != tagFilter) continue;

                RuntimeStatData runtimeStat = allTraits[i].RuntimeStats.Get(stat.ID);
                if (runtimeStat == null) continue;

                runtimeStat.Base = currentValue;
            }

            return DefaultResult;
        }
    }
}
