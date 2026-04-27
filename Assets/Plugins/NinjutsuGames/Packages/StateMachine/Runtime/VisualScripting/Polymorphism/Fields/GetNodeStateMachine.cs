using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Common;

namespace NinjutsuGames.StateMachine.Runtime.Variables
{
    [Title("State Machine Node")]
    [Category("State Machine/State Machine Node")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]
    [Description("Returns a node of a State Machine Variable")]
    [Serializable]
    [HideLabelsInEditor]
    public class GetNodeStateMachine : PropertyTypeGetString
    {
        [SerializeField] protected FieldGetNodeStateMachine m_Variable = new();

        public override string Get(Args args) 
        {
            return m_Variable.GUID;
        }

        public override string Get(GameObject gameObject)
        {
            return m_Variable.GUID;
        }

        public override string String => m_Variable.ToString();
        
        public static PropertyGetString Create => new(new GetNodeStateMachine());

        public override string EditorValue => m_Variable.GetStateMachine().ToString();
    }
}