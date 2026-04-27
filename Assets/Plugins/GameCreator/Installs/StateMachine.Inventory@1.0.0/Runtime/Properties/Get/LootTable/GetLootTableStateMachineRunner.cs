using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;

namespace NinjutsuGames.StateMachine.Runtime.Inventory
{
    [Title("State Machine Runner Variable")]
    [Category("Variables/State Machine Runner Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]
    [Description("Returns the Loot Table value of a State Machine Runner Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetLootTableStateMachineRunner : PropertyTypeGetLootTable
    {
        [SerializeField]
        protected FieldGetStateMachineRunner m_Variable = new FieldGetStateMachineRunner(ValueLootTable.TYPE_ID);

        public override LootTable Get(Args args) => m_Variable.Get<LootTable>(args);

        public override string String => m_Variable.ToString();
    }
}