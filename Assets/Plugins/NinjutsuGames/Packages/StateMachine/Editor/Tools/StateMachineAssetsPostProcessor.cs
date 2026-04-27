using System;
using GameCreator.Editor.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Editor
{
    public class StateMachineAssetsPostProcessor : AssetPostprocessor
    {
        public static event Action EventRefresh;
        private static StateMachineAsset asset;
        
        // PROCESSORS: ----------------------------------------------------------------------------

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            asset = null;

            SettingsWindow.InitRunners.Add(new InitRunner(
                SettingsWindow.INIT_PRIORITY_LOW,
                CanRefreshVariables,
                RefreshStateMachines
            ));
        }
        
        private static void OnPostprocessAllAssets(
            string[] importedAssets, 
            string[] deletedAssets, 
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var shouldReloadWindows = false;
            
            // Check if any StateMachine assets were imported/modified
            foreach (var assetPath in importedAssets)
            {
                if (!assetPath.EndsWith(".asset")) continue;
                asset = AssetDatabase.LoadAssetAtPath<StateMachineAsset>(assetPath);
                if (!asset) continue;
                shouldReloadWindows = true;
                break;
            }
            
            // Check if any StateMachine assets were deleted
            foreach (var assetPath in deletedAssets)
            {
                if (!assetPath.EndsWith(".asset")) continue;
                shouldReloadWindows = true;
                break;
            }
            
            // Check if any StateMachine assets were moved
            for (var i = 0; i < movedAssets.Length; i++)
            {
                if (!movedAssets[i].EndsWith(".asset")) continue;
                asset = AssetDatabase.LoadAssetAtPath<StateMachineAsset>(movedAssets[i]);
                if (!asset) continue;
                shouldReloadWindows = true;
                break;
            }
            if(shouldReloadWindows && asset)
            {
                var window = EditorWindow.HasOpenInstances<StateMachineGraphWindow>();
                
                // Delay the reload to ensure all assets are properly imported
                if(window) EditorApplication.delayCall += ReloadWindow;
            }
            if (importedAssets.Length == 0 && deletedAssets.Length == 0) return;
            if(!StateMachineRepository.Get.StateMachineSettings.enableDatabase) return;
            RefreshStateMachines();
        }

        private static void ReloadWindow()
        {
            EditorApplication.delayCall -= ReloadWindow;
            if(!EditorWindow.focusedWindow) return;
            if(EditorWindow.focusedWindow.GetType() != typeof(StateMachineGraphWindow)) return;
            if(!EditorWindow.HasOpenInstances<StateMachineGraphWindow>()) return;
            var window = EditorWindow.GetWindow<StateMachineGraphWindow>();
            if(window.Graph != asset) return;
            if(!window.hasFocus)
            {
                window.ShouldReload = true;
                return;
            }
            window.InitializeGraph(asset);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static bool CanRefreshVariables()
        {
            return true;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public static void RefreshStateMachines()
        {
            var varSettingsGuids = AssetDatabase.FindAssets($"t:{nameof(StateMachineSettings)}");
            if (varSettingsGuids.Length == 0) return;

            var varSettingsPath = AssetDatabase.GUIDToAssetPath(varSettingsGuids[0]);
            
            var varSettings = AssetDatabase.LoadAssetAtPath<StateMachineSettings>(varSettingsPath);
            if (varSettings == null) return;

            // Check if database is enabled before populating
            if (!StateMachineRepository.Get.StateMachineSettings.enableDatabase)
            {
                // Clear the database if it's disabled
                var varSettingsSerializedObject = new SerializedObject(varSettings);
                var globalAssetsProperty = varSettingsSerializedObject
                    .FindProperty(TAssetRepositoryEditor.NAMEOF_MEMBER)
                    .FindPropertyRelative("m_StateMachineAssets");

                var stateMachinesProperty = globalAssetsProperty.FindPropertyRelative("m_StateMachineAssets");
                stateMachinesProperty.arraySize = 0;
                
                varSettingsSerializedObject.ApplyModifiedPropertiesWithoutUndo();
                EventRefresh?.Invoke();
                return;
            }

            var stateMachineGuids = AssetDatabase.FindAssets($"t:{nameof(StateMachineAsset)}");
            var stateMachineAssets = new StateMachineAsset[stateMachineGuids.Length];
            
            for (var i = 0; i < stateMachineGuids.Length; i++)
            {
                var nameVariablesGuid = stateMachineGuids[i];
                var nameVariablesPath = AssetDatabase.GUIDToAssetPath(nameVariablesGuid);
                stateMachineAssets[i] = AssetDatabase.LoadAssetAtPath<StateMachineAsset>(nameVariablesPath);
            }
            
            var varSettingsSerializedObjectFinal = new SerializedObject(varSettings);
            var globalAssetsPropertyFinal = varSettingsSerializedObjectFinal
                .FindProperty(TAssetRepositoryEditor.NAMEOF_MEMBER)
                .FindPropertyRelative("m_StateMachineAssets");

            var stateMachinesPropertyFinal = globalAssetsPropertyFinal.FindPropertyRelative("m_StateMachineAssets");
                
            stateMachinesPropertyFinal.arraySize = stateMachineAssets.Length;
            for (var i = 0; i < stateMachineAssets.Length; ++i)
            {
                stateMachinesPropertyFinal.GetArrayElementAtIndex(i).objectReferenceValue = stateMachineAssets[i];
            }

            varSettingsSerializedObjectFinal.ApplyModifiedPropertiesWithoutUndo();
            EventRefresh?.Invoke();
        }
    }
}