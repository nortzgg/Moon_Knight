using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    [InitializeOnLoad]
    public static class StateMachineSearchProvider
    {
        private const string PROVIDER_ID = "sm-nodes";
        private const int TRANSITION_DURATION = 350;
        private static readonly IIcon ICON_SEARCH = new IconSearch(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_INSTRUCTION = new IconInstructions(ColorTheme.Type.Blue);
        private static readonly IIcon ICON_CONDITION = new IconConditions(ColorTheme.Type.Green);
        private static readonly IIcon ICON_BRANCH = new IconBranch(ColorTheme.Type.Green);
        private static readonly IIcon ICON_EVENT = new IconTriggers(ColorTheme.Type.Yellow);
        private static readonly IIcon ICON_NONE = new IconNull(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_STATEMACHINE = new IconStateMachine(ColorTheme.Type.Purple);
        
        static StateMachineSearchProvider()
        {
            // Register the provider when the class is loaded
        }
        
        [SearchItemProvider]
        private static SearchProvider CreateProvider()
        {
            var provider = new SearchProvider(PROVIDER_ID, "State Machine Nodes")
            {
                filterId = "sm:",
                fetchItems = (context, items, provider) => FetchItems(context, provider),
                fetchLabel = (item, context) => item.label,
                fetchDescription = (item, context) => GetItemDescription(item),
                fetchThumbnail = (item, context) => GetItemThumbnail(item),
                // trackSelection = (item, context) => TrackSelection(item),
                showDetails = true,
                priority = 90, // Higher priority to appear near the top
                isExplicitProvider = false, // Make it always available
                showDetailsOptions = ShowDetailsOptions.Inspector | ShowDetailsOptions.Preview | ShowDetailsOptions.Description,
            };
            return provider;
        }

        private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
        {
            var items = new List<SearchItem>();
            
            // Find all StateMachineAsset files in the project
            var guids = AssetDatabase.FindAssets("t:StateMachineAsset");
            
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var stateMachine = AssetDatabase.LoadAssetAtPath<Runtime.StateMachineAsset>(assetPath);
                
                if (stateMachine == null)
                {
                    continue;
                }
                
                if (stateMachine.nodes == null)
                {
                    continue;
                }
                
                foreach (var node in stateMachine.nodes)
                {
                    switch (node)
                    {
                        case null:
                        // Skip certain node types if needed
                        case Runtime.StartNode or Runtime.ExitNode:
                            continue;
                    }

                    var nodeName = node.GetCustomName();
                    var nodeType = node.GetType().Name;
                    
                    // Create search content with relevant information
                    var content = $"{nodeName} {nodeType} {assetPath}";
                    
                    // Check if the node matches the search query
                    if (!string.IsNullOrEmpty(context.searchText) && 
                        !content.ToLowerInvariant().Contains(context.searchText.ToLowerInvariant()))
                    {
                        continue;
                    }
                    
                    // Create a search item for this node
                    var item = provider.CreateItem(context, 
                        $"{guid}_{node.GUID}", // Unique ID
                        1,  // Score (fixed value)
                        $"<b>{nodeName}</b>",              // Display name
                        $"{nodeName} in {stateMachine.name}", // Description
                        null,     // Icon (set later)
                        stateMachine);         // Associated object
                    
                    // Store additional data for selection
                    item.data = new NodeSearchData
                    {
                        StateMachine = stateMachine,
                        Node = node,
                        AssetPath = assetPath
                    };
                    
                    items.Add(item);
                }
            }
            return items;
        }
        
        private static string GetItemDescription(SearchItem item)
        {
            if (item.data is not NodeSearchData data) return string.Empty;
            var nodeType = data.Node.GetType().Name;
            return $"{nodeType} in {data.StateMachine.name}";

        }
        
        private static Texture2D GetItemThumbnail(SearchItem item)
        {
            if (item.data is not NodeSearchData data) return ICON_SEARCH.Texture;
            // Return different icons based on node type
            if (data.Node is Runtime.ActionsNode) return ICON_INSTRUCTION.Texture;
            if (data.Node is Runtime.ConditionsNode) return ICON_CONDITION.Texture;
            if (data.Node is Runtime.TriggerNode) return ICON_EVENT.Texture;
            if (data.Node is Runtime.BranchNode) return ICON_BRANCH.Texture;
            if (data.Node is Runtime.StateMachineNode) return ICON_STATEMACHINE.Texture;
                
            // Default icon
            return ICON_NONE.Texture;

        }
        
        private static bool TrackSelection(SearchItem item)
        {
            if (item?.data is not NodeSearchData data) return false;
            // Select the asset in the project window
            Selection.activeObject = data.StateMachine;
                
            // Open the state machine editor window
            var window = EditorWindow.GetWindow<StateMachineGraphWindow>();
            window.InitializeGraph(data.StateMachine);
                
            // Find and focus on the specific node
            if (!window) return false;
            var graphView = window.rootVisualElement.Query<StateMachineGraphView>().First();
            // Find the node view for this node
            var nodeView = graphView?.nodeViews.FirstOrDefault(nv => 
                nv.nodeTarget.GUID == data.Node.GUID);

            if (nodeView == null) return false;
                            
            // Center the view on this node
            var nodeRect = nodeView.GetPosition();
            var nodeCenter = nodeRect.center;
            var position = new Vector3(-nodeCenter.x, -nodeCenter.y, 0) * graphView.viewTransform.scale.x;
            position.x += graphView.contentRect.size.x / 2;
            position.y += graphView.contentRect.size.y / 2;
            graphView.experimental.animation.Start(
                graphView.viewTransform.position, position, TRANSITION_DURATION,
                (e, pos) => graphView.UpdateViewTransform(pos, graphView.viewTransform.scale)
            );
                            
            // Highlight the node
            nodeView.Highlight(null);
                            
            return true;

        }
        
        // Helper class to store node data
        private class NodeSearchData
        {
            public Runtime.StateMachineAsset StateMachine;
            public Runtime.BaseNode Node;
            public string AssetPath;
        }
        
        // Method to open the search window from the toolbar
        public static void OpenSearchWindow()
        {
            try
            {
                // Create a search context with our provider ID
                var context = SearchService.CreateContext(PROVIDER_ID, string.Empty, SearchFlags.NoIndexing);
                
                // Show the search window with our context
                SearchService.ShowPicker(context, OnSelect);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StateMachineSearchProvider] Error opening search window: {ex.Message}\n{ex.StackTrace}");
                
                // Fallback to a simpler approach
                try
                {
                    // Try using the object picker as a fallback
                    SearchService.ShowObjectPicker(
                        (obj, canceled) => Debug.Log($"Selected: {obj?.name ?? "none"}, canceled: {canceled}"),
                        null,
                        "sm:",
                        null,
                        typeof(Runtime.StateMachineAsset)
                    );
                    Debug.Log("[StateMachineSearchProvider] Opened object picker as fallback");
                }
                catch (Exception fallbackEx)
                {
                    Debug.LogError($"[StateMachineSearchProvider] Fallback error: {fallbackEx.Message}\n{fallbackEx.StackTrace}");
                }
            }
        }

        private static void OnSelect(SearchItem searchItem, bool success)
        {
            TrackSelection(searchItem);
        }
    }
}