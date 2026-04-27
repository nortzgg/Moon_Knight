using System;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Variables
{
    [Serializable]
    public class FieldGetNodeStateMachine
    {
        [SerializeField] protected StateMachineAsset m_StateMachine;
        [SerializeField] protected string m_Name;
        [SerializeField] protected string m_GUID;
        
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public FieldGetNodeStateMachine()
        {
            m_StateMachine = null;
            m_Name = string.Empty;
            m_GUID = string.Empty;
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public StateMachineAsset GetStateMachine() => m_StateMachine;
        
        public string NodeName => m_Name;
        public string GUID => m_GUID;
        
        public T Get<T>()
        {
            var value = Get();
            return Convert.ChangeType(value, typeof(T)) is T typedValue ? typedValue : default;
        }

        public object Get()
        {
            return m_StateMachine != null ? m_StateMachine.GetNode(m_Name) : null;
        }

        public override string ToString()
        {
            var stateMachine = m_StateMachine ? m_StateMachine.name : "(none)";
            var nodeName = string.IsNullOrEmpty(m_Name) ? "(none)" : m_Name;
            return $"<b>{stateMachine}</b> > <b>{nodeName}</b>";
        }
    }
}