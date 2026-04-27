using System;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    public class StateMachineGeneralSettings
    {
        [Tooltip("Enable the inspector for the State Machine.")]
        public bool enableInspector = true;
        [Tooltip("Enable the database for the State Machine. If disabled, the database will be cleared. This is useful for when you don't want to have all state machines included in the build.")]
        public bool enableDatabase = true;
    }
}