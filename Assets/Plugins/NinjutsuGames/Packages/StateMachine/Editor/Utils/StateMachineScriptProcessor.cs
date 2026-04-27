using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Editor
{
    [InitializeOnLoad]
    public static class StateMachineScriptProcessor
    {
        private const string SM_SYMBOL = "STATE_MACHINE";

        internal static string SM_PATH = $"{Runtime.RuntimePaths.PACKAGES}StateMachine/";
        internal static string GC_PATH = $"Assets/Plugins/GameCreator/Packages/Core/Runtime/VisualScripting/Components/";
        private static bool HasGC => IsAssemblyLoaded("GameCreator.Runtime.Core");
        
        static StateMachineScriptProcessor()
        {
            if(HasGC)
            {
                var path = $"{GC_PATH}/Trigger.cs";

                var linesToAdd = new Dictionary<string, string>
                {
                    {"[NonSerialized] private Args m_Args;", "        protected Args TriggerArgs => m_Args;"},
                };
                var linesToModify = new Dictionary<string, string>();
                ModifyScript(path, linesToAdd, linesToModify);
            }

            CheckGC();
        }
        
        private static bool IsAssemblyLoaded(string assemblyName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == assemblyName);
        }

        private static void CheckGC()
        {
            var isInstalled = AssetDatabase.IsValidFolder(SM_PATH);
            if(HasGC && isInstalled) AddScriptingDefineSymbol(SM_SYMBOL);
            else CleanUpDefineSymbols();
        }
        
        private static void AddScriptingDefineSymbol(string symbol)
        {
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown) continue;

                try
                {
                    // Convert BuildTargetGroup to NamedBuildTarget
                    var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);

                    var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
                    if (!defines.Contains(symbol))
                    {
                        defines += $";{symbol}";
                        PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
                    }
                }
                catch (ArgumentException)
                {
                    // Debug.LogWarning($"Skipping unsupported BuildTargetGroup '{group}': {e.Message}");
                }
            }
        }

        internal static void CleanUpDefineSymbols()
        {
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown) continue;

                try
                {
                    // Convert BuildTargetGroup to NamedBuildTarget
                    var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);

                    var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget)
                        .Split(';')
                        .Where(symbol => !symbol.Equals(SM_SYMBOL) && !symbol.StartsWith(SM_SYMBOL))
                        .ToArray();
    
                    PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, string.Join(";", defines));
                }
                catch (ArgumentException)
                {
                    // Debug.LogWarning($"Skipping unsupported BuildTargetGroup '{group}': {e.Message}");
                }
            }
        }

        private static void ModifyScript(string path, Dictionary<string, string> linesToAdd, Dictionary<string, string> linesToModify)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"StateMachineScriptProcessor: File not found at path: {path}");
                return;
            }

            var modified = false;
            var lines = new List<string>(File.ReadAllLines(path));
            var newLines = new List<string>(); // This list will keep track of the new lines that have been added in the current iteration

            for (var i = 0; i < lines.Count; i++)
            {
                foreach (var pair in linesToAdd)
                {
                    if (lines[i].Contains(pair.Key) && !lines.Any(line => line.Contains(pair.Value)) && !newLines.Contains(pair.Value))
                    {
                        Debug.Log($"StateMachineScriptProcessor: Adding line: {pair.Value} after {pair.Key}");

                        lines.Insert(i + 1, pair.Value);
                        newLines.Add(pair.Value); // Add the new line to the list of new lines
                        modified = true;
                    }
                }

                // Clear the list of new lines for the next iteration
                newLines.Clear();

                // Remove the matched keys from the dictionary
                foreach (var line in newLines)
                {
                    var key = linesToAdd.First(pair => pair.Value == line).Key;
                    linesToAdd.Remove(key);
                }
            }

            for (var i = 0; i < lines.Count; i++)
            {
                foreach (var pair in linesToModify)
                {
                    if (!lines[i].Contains(pair.Key)) continue;
                    lines[i] = pair.Value;
                    modified = true;
                }
            }

            if (!modified) return;
            Debug.Log($"StateMachineScriptProcessor: Script {path} modified");
            File.WriteAllLines(path, lines.ToArray());
        }
    }
    
    public class CleanUpDefinesOnSMDelete : AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions rao)
        {
            var path = StateMachineScriptProcessor.SM_PATH;

            if (StateMachineScriptProcessor.GC_PATH.Equals(assetPath) || path.Equals(assetPath))
            {
                StateMachineScriptProcessor.CleanUpDefineSymbols();
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}