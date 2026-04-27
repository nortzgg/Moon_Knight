using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    public class StateMachineAssets
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [NonSerialized] private Dictionary<IdString, StateMachineAsset> m_MapAssets;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private StateMachineAsset[] m_StateMachineAssets = Array.Empty<StateMachineAsset>();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public StateMachineAsset[] Assets => m_StateMachineAssets;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public StateMachineAsset GetNameVariablesAsset(IdString itemID)
        {
            RequireInitialize();
            return m_MapAssets.GetValueOrDefault(itemID);
        }

        public void RequireInitialize()
        {
            if (!StateMachineRepository.Get.StateMachineSettings.enableDatabase)
            {
                m_MapAssets?.Clear();
                m_MapAssets = null;
                m_StateMachineAssets = Array.Empty<StateMachineAsset>();
                return;
            }

            if (m_MapAssets != null) return;
            
            m_MapAssets = new Dictionary<IdString, StateMachineAsset>();

            foreach (var stateMachineAsset in m_StateMachineAssets)
            {
                m_MapAssets[stateMachineAsset.UniqueID] = stateMachineAsset;
            }
        }
        
        // INTERNAL METHODS: ----------------------------------------------------------------------
        
        internal void Set(StateMachineAsset[] nameVariables)
        {
            m_StateMachineAssets = nameVariables;
        }
    }
}