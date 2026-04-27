using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Stats;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Stats
{
    [Title("State Machine Runner Variable")]
    [Category("Variables/State Machine Runner Variable")]
    
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow, typeof(OverlayBolt))]
    [Description("Returns the Attribute value of a State Machine Runner Variable")]

    [Serializable] [HideLabelsInEditor]
    public class GetAttributeStateMachineRunner : PropertyTypeGetAttribute
    {
        [SerializeField]
        protected FieldGetStateMachineRunner m_Variable = new FieldGetStateMachineRunner(ValueAttribute.TYPE_ID);

        public override GameCreator.Runtime.Stats.Attribute Get(Args args) => m_Variable.Get<GameCreator.Runtime.Stats.Attribute>(args);

        public override string String => m_Variable.ToString();
    }
}