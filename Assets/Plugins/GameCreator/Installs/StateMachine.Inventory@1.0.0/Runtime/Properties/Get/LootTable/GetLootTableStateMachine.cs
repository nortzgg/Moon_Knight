using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;

namespace NinjutsuGames.StateMachine.Runtime.Inventory
{
    [Title("State Machine Variable")]
    [Category("Variables/State Machine Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]
    [Description("Returns the Loot Table value of a State Machine Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetLootTableStateMachine : PropertyTypeGetLootTable
    {
        [SerializeField]
        protected FieldGetStateMachine m_Variable = new FieldGetStateMachine(ValueLootTable.TYPE_ID);

        public override LootTable Get(Args args) => m_Variable.Get<LootTable>(args);

        public override string String => m_Variable.ToString();
    }
}