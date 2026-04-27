using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Common;

namespace NinjutsuGames.StateMachine.Runtime.Variables
{
    [Title("Runner Instance")]
    [Category("Game Objects/State Machine Runner Instance")]
    [Image(typeof(IconStateMachine), ColorTheme.Type.Yellow)]
    [Description("Returns an auto-instantiated State Machine Runner instance.")]
    [Serializable]
    [HideLabelsInEditor]
    public class GetGameObjectStateMachineInstance : PropertyTypeGetGameObject
    {
        [SerializeField] private StateMachineAsset m_StateMachine;
        public override GameObject Get(Args args)
        {
            return !m_StateMachine ? null : StateMachineRunnerInstances.Instance.Get(m_StateMachine)?.gameObject;
        }

        public override string String => "Runner Instance";
    }
}