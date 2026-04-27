using System;
using System.Collections;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    public class RunnerVariableList : TPolymorphicList<RunnerVariableItem>, IEnumerable
    {
        [SerializeField] protected FieldGetNodeStateMachine m_Node = new();
        [SerializeReference] private RunnerVariableItem[] m_List;
    
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Length => m_List.Length;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public RunnerVariableItem Get(int index) => m_List[index];
        public StateMachineAsset StateMachine => m_Node.GetStateMachine();
        public string NodeGUID => m_Node.GUID;
        public override string ToString() => m_Node.ToString();
        public IEnumerator GetEnumerator()
        {
            foreach (var item in m_List)
            {
                yield return item;
            }
        }
    }
}