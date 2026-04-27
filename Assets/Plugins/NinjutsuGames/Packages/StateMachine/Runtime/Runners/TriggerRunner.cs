using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace NinjutsuGames.StateMachine.Runtime
{
    [AddComponentMenu("")]
    public class TriggerRunner : Trigger
    {
        public void Setup(Event triggerEvent, Action<Args> onTriggerRun, Action<Args> onTriggerStopped)
        {
            hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;
            enabled = false;
            
            m_TriggerEvent = triggerEvent;
            // m_Instructions = new InstructionList();
            Awake();

            EventBeforeExecute -= OnRun;
            EventBeforeExecute += OnRun;
            // EventAfterExecute -= OnStopped;
            // EventAfterExecute += OnStopped;
            
            // gameObject.SetActive(true);
            enabled = true;
            return;

            void OnRun()
            {
#if STATE_MACHINE
                onTriggerRun(TriggerArgs);
                onTriggerStopped(TriggerArgs);
#endif
            }
            
            /*void OnStopped(Args newArgs)
            {                
                onTriggerStopped(newArgs);
            }*/
        }
    }
}