using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Editor
{
    [InitializeOnLoad]
    public class ObjectChangeEventCallback
    {
        static ObjectChangeEventCallback()
        {
            ObjectChangeEvents.changesPublished += OnChangesPublished; 
        }
        ~ObjectChangeEventCallback()
        {
            ObjectChangeEvents.changesPublished -= OnChangesPublished; 
        }
        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (var i = 0; i < stream.length; i++)
            {
                var evt = stream.GetEventType(i);
                // Debug.Log($"ObjectChangeEventCallback: {evt}");
                if (evt != ObjectChangeKind.CreateGameObjectHierarchy) continue;
                stream.GetCreateGameObjectHierarchyEvent(i, out var data);
                
                // Get object by instance id
                var obj = EditorUtility.InstanceIDToObject(data.instanceId) as GameObject;
                if (obj == null) continue;
                var runner = obj.GetComponent<StateMachineRunner>();
                if (!runner) continue;
                if (!runner.cloneAsset) continue;
                // Debug.Log($"Creating a copy of the asset <b>{runner.cloneAsset.name}</b> for <b>{runner.gameObject.name}</b> runner.");
                var newInstance = Object.Instantiate(runner.cloneAsset);
                newInstance.name = $"{runner.gameObject.name} State Machine";
                runner.cloneAsset = newInstance;
                if(runner.isEmbedded) runner.stateMachineAsset = newInstance;
            }
        }
    }
}