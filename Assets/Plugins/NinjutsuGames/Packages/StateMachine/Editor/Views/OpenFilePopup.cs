using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Editor
{
    public static class OpenFilePopup
    {
        public static void Open()
        {
            SearchService.ShowObjectPicker(OnSelect, OnObjectPickerSelect, null, null, typeof(StateMachineAsset));
        }

        private static void OnSelect(Object stateMachineAsset, bool arg2)
        {
            if(!stateMachineAsset) return;
            if(stateMachineAsset is not StateMachineAsset asset) return;
            
            var window = EditorWindow.GetWindow<StateMachineGraphWindow>();
            window.InitializeGraph(asset);
        }

        private static void OnObjectPickerSelect(Object stateMachineAsset)
        {
            if(!stateMachineAsset) return;
            if(stateMachineAsset is not StateMachineAsset asset) return;
            
            // var window = EditorWindow.GetWindow<StateMachineGraphWindow>();
            // window.InitializeGraph(asset);
        }
    }
}