using System;
using GameCreator.Runtime.Common;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    public class StateMachineRepository : TRepository<StateMachineRepository>
    {
        public const string REPOSITORY_ID = "statemachine.assets";

        // REPOSITORY PROPERTIES: -----------------------------------------------------------------
        
        public override string RepositoryID => REPOSITORY_ID;

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private StateMachineVersion m_Version = new();
        [SerializeField] private StateMachineGeneralSettings m_Settings = new();
        [SerializeField] private StateMachineAssets m_StateMachineAssets = new();
        [SerializeField] private StateMachineStaticAssets m_AutoInstantiate = new();

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public StateMachineAssets StateMachineAssets => m_StateMachineAssets;
        public StateMachineGeneralSettings StateMachineSettings => m_Settings;
        public StateMachineStaticAssets AutoInstantiate => m_AutoInstantiate;
        
        // EDITOR ENTER PLAYMODE: -----------------------------------------------------------------

#if UNITY_EDITOR
        
        [InitializeOnEnterPlayMode]
        public static void InitializeOnEnterPlayMode() => Instance = null;
        
#endif
    }

}