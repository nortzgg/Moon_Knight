using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    public class StateMachineList : TPolymorphicList<StateMachineItem>
    {
        [SerializeReference] private StateMachineItem[] m_List;
    
        // PROPERTIES: ----------------------------------------------------------------------------

        public override int Length => m_List?.Length ?? 0;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        public StateMachineItem Get(int index) => m_List[index];
    }
}