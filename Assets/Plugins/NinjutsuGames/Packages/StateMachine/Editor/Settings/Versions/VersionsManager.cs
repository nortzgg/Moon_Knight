using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace NinjutsuGames.StateMachine.Editor
{
    internal static class VersionsManager
    {
        private static readonly TextInfo TXT = CultureInfo.InvariantCulture.TextInfo;
        
        // CONSTANTS: -----------------------------------------------------------------------------
        
        private const string URI = "https://raw.githubusercontent.com/hjupter/documentation/main/game-creator-2/state-machine-2/releases.json";

        private const string KEY_HASH = "state-machine:versions-latest-hash";
        private const string KEY_LATEST = "state-machine:versions-latest-data";

        private const string KEY_ASSET = "state-machine:versions-{0}-data";
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        private static UnityWebRequest RequestLatest;

        // PROPERTIES: ----------------------------------------------------------------------------

        private static bool IsInitialized { get; set; }
        
        public static LatestData Latest { get; private set; }
        public static Dictionary<string, AssetEntry> LatestEntries { get; private set; }

        private static int FetchCount = 0;

        // EVENTS: --------------------------------------------------------------------------------

        public static event Action EventChange;
        public static event Action EventDone;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public static void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            
            Latest = new LatestData();
            LatestEntries = new Dictionary<string, AssetEntry>();

            if (EditorPrefs.HasKey(KEY_LATEST))
            {
                EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(KEY_LATEST), Latest);
                foreach (var entry in Latest.List)
                {
                    if (string.IsNullOrEmpty(entry.Id)) continue;

                    var entryKey = string.Format(KEY_ASSET, entry.Id);
                    if (!EditorPrefs.HasKey(entryKey)) continue;

                    var jsonEntry = new AssetEntry(State.Ready);
                    EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(entryKey), jsonEntry);
                    
                    LatestEntries.Add(entry.Id, jsonEntry);
                }
                
                Latest.State = State.Ready;
                EventDone?.Invoke();
            }
            
            FetchLatest();
        }

        public static AssetVersion GetInstalledVersion(string id)
        {
            var title = TXT.ToTitleCase(id).Replace("-2", "").Replace("-", "");
            var path = RuntimePaths.PACKAGES + title + "/Editor/Version.txt";
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            var version = asset?.text;
            return version != null ? new AssetVersion(version) : AssetVersion.None;
        }
        
        // FETCH METHODS: -------------------------------------------------------------------------
        
        private static void FetchLatest()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            Latest.State = State.Loading;
            
            RequestLatest = UnityWebRequest.Get(URI);
            RequestLatest.SetRequestHeader("ContentType", "application/json");

            var operation = RequestLatest.SendWebRequest();
            operation.completed += OnLatestReceive;
        }
        
        private static void FetchAsset(string id, string uri)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                FetchCount += 1;
                return;
            }

            var request = UnityWebRequest.Get(uri);
            request.SetRequestHeader("ContentType", "application/json");
            var operation = request.SendWebRequest();
            
            operation.completed += _ =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    // Debug.LogError(request.error);
                    LatestEntries[id] = null;

                    EventChange?.Invoke();

                    FetchCount += 1;
                    if (FetchCount >= LatestEntries.Count)
                    {
                        EventDone?.Invoke();
                    }

                    return;
                }

                var json = ExtractLatestReleaseAsJson(request.downloadHandler.text);
                var data = new AssetEntry();

                EditorJsonUtility.FromJsonOverwrite(json, data);
                var entryKey = string.Format(KEY_ASSET, id);
                
                EditorPrefs.SetString(entryKey, EditorJsonUtility.ToJson(data));
                LatestEntries[id] = data;
                
                EventChange?.Invoke();
            };
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static void OnLatestReceive(AsyncOperation asyncOperation)
        {
            if (RequestLatest.result != UnityWebRequest.Result.Success)
            {
                // Debug.LogWarning(RequestLatest.error);

                Latest.State = State.Error;
                LatestEntries.Clear();
                
                EventChange?.Invoke();
                return;
            }

            var json = RequestLatest.downloadHandler.text;
            var data = new LatestData(State.Ready);
            EditorJsonUtility.FromJsonOverwrite(json, data);

            var dataJson = EditorJsonUtility.ToJson(data, false);
            
            EditorPrefs.SetString(KEY_LATEST, dataJson);

            var currentHash = EditorPrefs.GetInt(KEY_HASH, 0);
            if (currentHash == json.GetHashCode()) return;
            
            EditorPrefs.SetInt(KEY_HASH, currentHash);

            Latest.State = State.Ready;
            LatestEntries.Clear();
            
            foreach (var entry in data.List)
            {
                LatestEntries.Add(entry.Id, new AssetEntry(State.Loading));
            }
            
            EventChange?.Invoke();
            FetchCount = 0;

            foreach (var entry in data.List)
            {
                FetchAsset(entry.Id, entry.Path);
            }
        }

        private static string ExtractLatestReleaseAsJson(string markdownContent)
        {
            // Split the content by lines
            var lines = markdownContent.Split('\n');
            
            // Variables to store version and date
            var version = "";
            var date = "";
            var isProcessingChanges = false;
            
            // Sub-categorized changes
            var newChanges = new List<string>();
            var enhancedChanges = new List<string>();
            var changedChanges = new List<string>();
            var removedChanges = new List<string>();
            var fixedChanges = new List<string>();
            
            // Legacy single list for backward compatibility
            var allChanges = new List<string>();
            
            // Current category being processed
            List<string> currentCategoryList = allChanges; // Default to legacy list
            bool hasSubCategories = false;

            // Start parsing from the top to get the latest release
            foreach (var t in lines)
            {
                // Identify version and date line (e.g., "## 1.0.4 (1st October 2024)")
                if (t.StartsWith("## "))
                {
                    if (!string.IsNullOrEmpty(version) && (allChanges.Count > 0 || hasSubCategories))
                    {
                        // We've found the next version after processing the changes for the first one.
                        break;
                    }

                    // Extract version and date for the first (latest) release
                    var match = Regex.Match(t, @"## (\d+\.\d+\.\d+) \((\d+)(?:st|nd|rd|th)? (\w+) (\d{4})\)");
                    if (!match.Success) continue;
                    version = match.Groups[1].Value;
                    date = $"{match.Groups[2].Value} {match.Groups[3].Value} {match.Groups[4].Value}";
                    isProcessingChanges = true; // Start processing changes
                }
                // Check for sub-category headers (e.g., "### New", "#### New", etc.)
                else if (isProcessingChanges && (t.StartsWith("### ") || t.StartsWith("#### ")))
                {
                    var headerStart = t.StartsWith("### ") ? 4 : 5;
                    var categoryName = t[headerStart..].Trim().ToLower();
                    
                    // Remove HTML anchor tags if present (e.g., "new <a href="#new" id="new"></a>")
                    if (categoryName.Contains('<'))
                    {
                        var spaceIndex = categoryName.IndexOf(' ');
                        if (spaceIndex > 0)
                        {
                            categoryName = categoryName[..spaceIndex];
                        }
                    }
                    
                    hasSubCategories = true;
                    
                    currentCategoryList = categoryName switch
                    {
                        "new" => newChanges,
                        "enhanced" => enhancedChanges,
                        "changed" => changedChanges,
                        "removed" => removedChanges,
                        "fixed" => fixedChanges,
                        _ => allChanges // Fallback to legacy list for unknown categories
                    };
                }
                // Extract changes (lines that start with "* ")
                else if (isProcessingChanges && t.StartsWith("* "))
                {
                    var change = t[2..].Trim();
                    currentCategoryList.Add(change);
                    
                    // Also add to legacy list for backward compatibility
                    if (currentCategoryList != allChanges)
                    {
                        allChanges.Add(change);
                    }
                }
                // Handle continuation lines (indented lines that are part of the previous bullet point)
                else if (isProcessingChanges && currentCategoryList.Count > 0 && 
                         (t.StartsWith("  ") || t.StartsWith("\t")) && !string.IsNullOrWhiteSpace(t.Trim()))
                {
                    // This is a continuation of the previous bullet point
                    var lastIndex = currentCategoryList.Count - 1;
                    var continuationText = t.Trim();
                    currentCategoryList[lastIndex] += " " + continuationText;
                    
                    // Also update the legacy list if this is a sub-categorized change
                    if (currentCategoryList != allChanges && allChanges.Count > 0)
                    {
                        var legacyLastIndex = allChanges.Count - 1;
                        allChanges[legacyLastIndex] += " " + continuationText;
                    }
                }
            }
            
            // Create AssetEntry based on whether we found sub-categories
            AssetEntry changelogData;
            if (hasSubCategories)
            {
                var assetChanges = new AssetChanges(
                    newChanges.ToArray(),
                    enhancedChanges.ToArray(),
                    changedChanges.ToArray(),
                    removedChanges.ToArray(),
                    fixedChanges.ToArray()
                );
                
                changelogData = new AssetEntry(version, date, assetChanges);
            }
            else
            {
                // Use legacy constructor for backward compatibility
                changelogData = new AssetEntry(version, date, allChanges);
            }

            // Serialize the dictionary to a JSON string
            return EditorJsonUtility.ToJson(changelogData, false);
        }
    }
}