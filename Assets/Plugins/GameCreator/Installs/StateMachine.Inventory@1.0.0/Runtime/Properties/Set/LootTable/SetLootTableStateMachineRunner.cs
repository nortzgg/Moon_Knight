using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;

namespace NinjutsuGames.StateMachine.Runtime.Inventory
{
    [Title("State Machine Runner Variable")]
    [Category("State Machine Runner Variable")]
    
    [Description("Sets the Loot Table value on a State Machine Runner Variable")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]

    [Serializable] [HideLabelsInEditor]
    public class SetLootTableStateMachineRunner : PropertyTypeSetLootTable
    {
        [SerializeField]
        protected FieldSetStateMachineRunner m_Variable = new FieldSetStateMachineRunner(ValueLootTable.TYPE_ID);

        public override void Set(LootTable value, Args args) => m_Variable.Set(value, args);
        public override LootTable Get(Args args) => m_Variable.Get(args) as LootTable;

        public static PropertySetLootTable Create => new PropertySetLootTable(
            new SetLootTableStateMachineRunner()
        );
        
        public override string String => m_Variable.ToString();
    }
}