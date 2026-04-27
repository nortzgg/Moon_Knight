using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime;
using Object = UnityEngine.Object;
using System.Threading.Tasks;

namespace NinjutsuGames.StateMachine.Editor
{
    /// <summary>
    /// Custom editor of the node inspector, you can inherit from this class to customize your node inspector.
    /// </summary>
    [CustomEditor(typeof(NodeInspector))]
    public class NodeInspectorEditor : UnityEditor.Editor
    {
        private NodeInspector inspector;
        private VisualElement root;
        private VisualElement selectedNodeList;
        private VisualElement placeholder;
        private const string INFO_MESSAGE = "Select a node to show it's settings in the inspector";
        
        // Performance optimization: Cache node blocks to avoid recreating them
        private Dictionary<BaseNodeView, VisualElement> nodeBlockCache = new();
        private HashSet<BaseNodeView> lastSelectedNodes = new();
        
        // Cache for transitions lists to enable dynamic updates
        private Dictionary<BaseGameCreatorNode, (ListView listView, List<TransitionData> data)> transitionsCache = new();
        
        protected virtual void OnEnable()
        {
            inspector = target as NodeInspector;
            inspector.nodeSelectionUpdated += UpdateNodeInspectorList;
            inspector.nodeViewRemoved += OnNodeViewRemoved;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            root = new VisualElement();
            selectedNodeList = new VisualElement();
            var styleSheet = Resources.Load<StyleSheet>("GraphProcessorStyles/InspectorView");
            if(styleSheet != null) selectedNodeList.styleSheets.Add(styleSheet);
            root.Add(selectedNodeList);
            placeholder = new InfoMessage(INFO_MESSAGE);
            placeholder.AddToClassList("PlaceHolder");
            
            // Subscribe to graph changes for dynamic updates
            SubscribeToGraphEvents();
            
            UpdateNodeInspectorList();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            if (mode == PlayModeStateChange.EnteredPlayMode)
            {
                // Clear cache on play mode change to ensure fresh state
                nodeBlockCache.Clear();
                transitionsCache.Clear();
                UpdateNodeInspectorList();
            }
            
            // Update play button states when play mode changes
            UpdatePlayButtonStates();
        }
        
        private void UpdatePlayButtonStates()
        {
            if (selectedNodeList == null) return;
            
            var isPlayMode = EditorApplication.isPlayingOrWillChangePlaymode;
            
            // Find all play buttons and update their enabled state
            var playButtons = selectedNodeList.Query<Button>("GC-Instruction-List-Foot-Button").ToList();
            foreach (var button in playButtons)
            {
                button.SetEnabled(isPlayMode);
            }
        }

        protected override void OnHeaderGUI()
        {
            // base.OnHeaderGUI();
        }

        protected virtual void OnDisable()
        {
            if (inspector != null)
            {
                inspector.nodeSelectionUpdated -= UpdateNodeInspectorList;
                inspector.nodeViewRemoved -= OnNodeViewRemoved;
            }

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            // Unsubscribe from graph events
            UnsubscribeFromGraphEvents();

            // Clean up caches
            nodeBlockCache?.Clear();

            // Clear transition cache and properly unsubscribe events
            if (transitionsCache != null)
            {
                foreach (var kvp in transitionsCache)
                {
                    if (kvp.Value.listView != null)
                    {
                        // Clear any delegates on the list view (using safe methods)
                        var listView = kvp.Value.listView;

                        // Set to empty handlers instead of null
                        listView.bindItem = (ve, i) => {};
                        listView.makeItem = () => new VisualElement();
                    }
                }
                transitionsCache.Clear();
            }
        }

        public override VisualElement CreateInspectorGUI() => root;

        private void SubscribeToGraphEvents()
        {
            // Ensure inspector is valid
            if (inspector == null || inspector.selectedNodes == null) return;

            // Find the graph view from the selected nodes
            if (inspector.selectedNodes.Count > 0)
            {
                try
                {
                    var firstNode = inspector.selectedNodes.FirstOrDefault();
                    if (firstNode?.owner != null)
                    {
                        inspector.graphView = firstNode.owner;

                        // Subscribe to graph changes for dynamic updates
                        if (inspector.graphView?.graph != null)
                        {
                            // First unsubscribe to avoid duplicate subscriptions
                            inspector.graphView.graph.onGraphChanges -= OnGraphChanged;
                            inspector.graphView.graph.onGraphChanges += OnGraphChanged;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error subscribing to graph events: {ex.Message}");
                }
            }
        }
        
        private void UnsubscribeFromGraphEvents()
        {
            if (inspector.graphView?.graph != null)
            {
                inspector.graphView.graph.onGraphChanges -= OnGraphChanged;
            }
        }
        
        private void OnGraphChanged(GraphChanges changes)
        {
            if (changes == null) return;

            // Update transitions lists when edges are added/removed
            if (changes.addedEdge != null || changes.removedEdge != null)
            {
                try
                {
                    RefreshAllTransitionsLists();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error refreshing transitions: {ex.Message}");
                }
            }
        }

        private void OnNodeViewRemoved(BaseNodeView view)
        {
            if (view == null) return;
            nodeBlockCache?.Remove(view);
            if (view.nodeTarget is BaseGameCreatorNode gameCreatorNode)
            {
                transitionsCache?.Remove(gameCreatorNode);
            }
        }

        private void CleanUpCaches()
        {
            if (inspector?.graphView == null) return;

            if (nodeBlockCache != null)
            {
                foreach (var key in nodeBlockCache.Keys.ToList())
                {
                    if (key == null || !inspector.graphView.nodeViews.Contains(key))
                    {
                        nodeBlockCache.Remove(key);
                    }
                }
            }

            if (transitionsCache != null)
            {
                foreach (var key in transitionsCache.Keys.ToList())
                {
                    if (key == null || !inspector.graphView.nodeViews.Any(v => v.nodeTarget == key))
                    {
                        transitionsCache.Remove(key);
                    }
                }
            }
        }
        
        private void RefreshAllTransitionsLists()
        {
            if (transitionsCache == null || transitionsCache.Count == 0) return;

            foreach (var kvp in transitionsCache)
            {
                if (kvp.Key == null) continue;

                var node = kvp.Key;
                var (listView, transitionData) = kvp.Value;

                if (listView == null || transitionData == null) continue;

                try
                {
                    // Get output nodes efficiently
                    var outputNodes = node.GetOutputNodes();
                    if (outputNodes == null) continue;

                    var orderedNodes = outputNodes.OrderBy(n => n?.computeOrder ?? 0).ToList();

                    // Only refresh if the data has actually changed
                    var hasChanged = false;

                    // Check if the number of nodes has changed
                    if (orderedNodes.Count != transitionData.Count)
                    {
                        hasChanged = true;
                    }
                    else
                    {
                        // Check if any nodes have changed
                        for (var i = 0; i < orderedNodes.Count; i++)
                        {
                            if (i >= transitionData.Count || transitionData[i].Node != orderedNodes[i])
                            {
                                hasChanged = true;
                                break;
                            }
                        }
                    }

                    if (hasChanged)
                    {
                        // Update the transition data
                        transitionData.Clear();
                        transitionData.AddRange(orderedNodes.Select((outputNode, index) => new TransitionData
                        {
                            Node = outputNode,
                            OriginalIndex = index,
                            DisplayName = GetTransitionDisplayName(outputNode)
                        }));

                        // Refresh the list view
                        if (listView != null)
                        {
                            listView.RefreshItems();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error refreshing transition list: {ex.Message}");
                }
            }
        }

        protected virtual void UpdateNodeInspectorList()
        {
            if (inspector == null || selectedNodeList == null) return;

            try
            {
                // Remove stale cache entries to avoid memory leaks
                CleanUpCaches();

                // Performance optimization: Only update if selection actually changed
                if (inspector.selectedNodes != null && lastSelectedNodes != null && 
                    inspector.selectedNodes.SetEquals(lastSelectedNodes))
                    return;

                selectedNodeList.Clear();

                if (inspector.selectedNodes == null || inspector.selectedNodes.Count == 0)
                {
                    if (placeholder != null)
                    {
                        selectedNodeList.Add(placeholder);
                    }
                    if (lastSelectedNodes != null)
                    {
                        lastSelectedNodes.Clear();
                    }
                    return;
                }

                // Ensure graph events are subscribed when nodes are selected
                if (inspector.graphView == null)
                {
                    SubscribeToGraphEvents();
                }

                // Process nodes in batches for better performance
                const int batchSize = 5;
                var nodeBatch = 0;
                var processedNodes = new HashSet<BaseNodeView>();

                foreach (var nodeView in inspector.selectedNodes)
                {
                    if (nodeView == null) continue;
                    processedNodes.Add(nodeView);

                    // Process in batches to avoid UI freezes with many nodes
                    if (++nodeBatch > batchSize)
                    {
                        nodeBatch = 0;
                        // Allow UI to update
                        EditorApplication.update += ProcessRemainingNodes;
                        break;
                    }

                    try
                    {
                        var block = GetOrCreateNodeBlock(nodeView);
                        if (block != null)
                        {
                            selectedNodeList.Add(block);

                            if (nodeView is not (StartNodeView or ExitNodeView)) continue;
                            if (block.childCount <= 1) continue;

                            var containerElement = block.ElementAt(1);
                            if (containerElement == null) continue;

                            // More efficient check for existing InfoMessage
                            var hasInfoMessage = false;
                            for (var i = 0; i < containerElement.childCount; i++)
                            {
                                if (containerElement.ElementAt(i) is InfoMessage)
                                {
                                    hasInfoMessage = true;
                                    break;
                                }
                            }

                            // Only add InfoMessage if it doesn't already exist
                            if (!hasInfoMessage)
                            {
                                var description = new InfoMessage(nodeView is StartNodeView ? StartNodeView.INFO_MESSAGE : ExitNodeView.INFO_MESSAGE);
                                containerElement.Add(description);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing node view: {ex.Message}");
                    }
                }

                // Update the cache
                if (lastSelectedNodes != null)
                {
                    lastSelectedNodes.Clear();
                    lastSelectedNodes.UnionWith(inspector.selectedNodes);
                }

                // Local function to process remaining nodes in next frame
                void ProcessRemainingNodes()
                {
                    EditorApplication.update -= ProcessRemainingNodes;

                    var remainingBatch = 0;
                    foreach (var nodeView in inspector.selectedNodes)
                    {
                        if (nodeView == null || processedNodes.Contains(nodeView)) continue;

                        try
                        {
                            var block = GetOrCreateNodeBlock(nodeView);
                            if (block != null)
                            {
                                selectedNodeList.Add(block);

                                if (nodeView is not (StartNodeView or ExitNodeView)) continue;
                                if (block.childCount <= 1) continue;

                                var containerElement = block.ElementAt(1);
                                if (containerElement == null) continue;

                                // More efficient check for existing InfoMessage
                                var hasInfoMessage = false;
                                for (var i = 0; i < containerElement.childCount; i++)
                                {
                                    if (containerElement.ElementAt(i) is InfoMessage)
                                    {
                                        hasInfoMessage = true;
                                        break;
                                    }
                                }

                                // Only add InfoMessage if it doesn't already exist
                                if (!hasInfoMessage)
                                {
                                    var description = new InfoMessage(nodeView is StartNodeView ? StartNodeView.INFO_MESSAGE : ExitNodeView.INFO_MESSAGE);
                                    containerElement.Add(description);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error processing node view: {ex.Message}");
                        }

                        processedNodes.Add(nodeView);

                        if (++remainingBatch >= batchSize)
                        {
                            EditorApplication.update += ProcessRemainingNodes;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating node inspector list: {ex.Message}");
            }
        }


        private VisualElement GetOrCreateNodeBlock(BaseNodeView nodeView)
        {
            if (nodeView == null) return new VisualElement();

            // Performance optimization: Reuse cached blocks when possible
            if (nodeBlockCache != null && nodeBlockCache.TryGetValue(nodeView, out var cachedBlock))
            {
                // Check if the block is still valid
                if (cachedBlock != null)
                {
                    // Refresh the block content if needed
                    try
                    {
                        RefreshNodeBlock(nodeView, cachedBlock);
                        return cachedBlock;
                    }
                    catch (Exception)
                    {
                        // If refresh fails, remove from cache and create a new one
                        nodeBlockCache.Remove(nodeView);
                    }
                }
                else
                {
                    // Remove invalid block from cache
                    nodeBlockCache.Remove(nodeView);
                }
            }

            try
            {
                var block = CreateNodeBlock(nodeView);
                if (nodeBlockCache != null && block != null)
                {
                    nodeBlockCache[nodeView] = block;
                }
                return block;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating node block: {ex.Message}");
                return new VisualElement();
            }
        }

        private void RefreshNodeBlock(BaseNodeView nodeView, VisualElement block)
        {
            // Only refresh if the node has changed significantly
            // This is a placeholder for more sophisticated change detection
            if (nodeView is not BaseGameCreatorNodeView gcNodeView) return;
            // Update the header if needed
            var header = block.Q("Header");
            var label = header?.Q<Label>();
            if (label == null) return;
            var newText = string.IsNullOrEmpty(nodeView.nodeTarget.nodeCustomName) 
                ? nodeView.nodeTarget.name 
                : nodeView.nodeTarget.nodeCustomName;
            if (label.text != newText)
            {
                label.text = newText;
            }
        }

        protected VisualElement CreateNodeBlock(BaseNodeView nodeView)
        {
            var view = new VisualElement();

            if(nodeView is BaseGameCreatorNodeView gcNodeView)
            {
                var header = new VisualElement
                {
                    name = "Header"
                };
                header.AddToClassList("Header");
                var icon = new Image
                {
                    image = gcNodeView.DefaultIcon,
                    style =
                    {
                        marginLeft = 0,
                        marginTop = 0,
                        marginBottom = 10,
                        width = 20,
                        height = 20
                    }
                };
                header.Add(icon);
                // var state = gcNodeView.nodeTarget.enabledForExecution ? string.Empty : "(Disabled)";
                var label = new Label(string.IsNullOrEmpty(nodeView.nodeTarget.nodeCustomName) ? nodeView.nodeTarget.name : nodeView.nodeTarget.nodeCustomName);
                header.Add(label);
                
                if (nodeView is TriggerNodeView triggerNode)
                {
                    var helpIcon = new Image
                    {
                        image = BaseNodeView.ICON_HELP.Texture,
                    };
                    var helpButton = new Button(triggerNode.ShowHelp);
                    helpButton.Add(helpIcon);
                    helpButton.AddToClassList("Help");
                    header.Add(helpButton);
                }
                
                if (gcNodeView.nodeTarget.isLocked)
                {
                    var lockIcon = new Image
                    {
                        image = BaseNodeView.ICON_LOCK.Texture,
                    };
                    var lockButton = new Button(nodeView.ChangeLockStatus);
                    lockButton.Add(lockIcon);
                    lockButton.AddToClassList("Locked");
                    header.Add(lockButton);
                }
                
                if (!gcNodeView.nodeTarget.enabledForExecution)
                {
                    var disabledIcon = new Image
                    {
                        image = BaseNodeView.ICON_DISABLE.Texture,
                    };
                    var disabledButton = new Button(nodeView.ToggleExecutionState);
                    disabledButton.Add(disabledIcon);
                    disabledButton.AddToClassList("Disabled");
                    header.Add(disabledButton);
                }
                
                // Add spacer to push play button to the right
                var spacer = new VisualElement();
                spacer.style.flexGrow = 1;
                header.Add(spacer);
                
                // Add play button for BaseGameCreatorNode (positioned at far right)
                if (gcNodeView.nodeTarget is BaseGameCreatorNode gcNode)
                {
                    var playIcon = new Image
                    {
                        image = BaseNodeView.ICON_PLAY.Texture,
                    };
                    var playButton = new Button(() => RunNode(gcNode))
                    {
                        name = "GC-Instruction-List-Foot-Button"
                    };
                    playButton.AddToClassList("PlayButton");
                    playButton.Add(playIcon);
                    playButton.tooltip = "Run this node";
                    
                    playButton.SetEnabled(EditorApplication.isPlayingOrWillChangePlaymode);
                    
                    header.Add(playButton);
                }
                
                view.Add(header);
            }
            else view.Add(new Label(nodeView.nodeTarget.name));

            var tmp = nodeView.controlsContainer;
            nodeView.controlsContainer = view;
            nodeView.Enable(true);
            nodeView.controlsContainer.AddToClassList("NodeControls");
            var block = nodeView.controlsContainer;
            nodeView.controlsContainer = tmp;
            
            // Add transitions list after Network Settings for BaseGameCreatorNode
            if (nodeView is BaseGameCreatorNodeView gcNodeView2 && gcNodeView2.nodeTarget is BaseGameCreatorNode gameCreatorNode)
            {
                try
                {
                    AddTransitionsList(view, gameCreatorNode);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error adding transitions list: {ex.Message}");
                }
            }
            
            return block;
        }

        private void AddTransitionsList(VisualElement container, BaseGameCreatorNode node)
        {
            // Find the Network Settings section and add transitions list after it
            var networkingField = container.Q("networkingSettings");
            if (networkingField != null)
            {
                var transitionsContainer = CreateTransitionsListUI(node);
                var parent = networkingField.parent;
                var index = parent.IndexOf(networkingField);
                parent.Insert(index + 1, transitionsContainer);
            }
            else
            {
                // If no networking settings, add at the end
                container.Add(CreateTransitionsListUI(node));
            }
        }

        private VisualElement CreateTransitionsListUI(BaseGameCreatorNode node)
        {
            var container = new VisualElement();
            container.AddToClassList("transitions-container");
            
            var header = new Label("Transitions:");
            header.AddToClassList("transitions-header");
            container.Add(header);

            var listView = new ListView();
            listView.AddToClassList("transitions-list");
            
            // Get output nodes (transitions) sorted by execution order
            var outputNodes = node.GetOutputNodes().OrderBy(n => n.computeOrder).ToList();
            var transitionData = outputNodes.Select((outputNode, index) => new TransitionData
            {
                Node = outputNode,
                OriginalIndex = index,
                DisplayName = GetTransitionDisplayName(outputNode)
            }).ToList();

            // Cache the transitions list for dynamic updates
            transitionsCache[node] = (listView, transitionData);

            listView.itemsSource = transitionData;
            listView.makeNoneElement = () =>
            {
                var emptyItem = new Label("No transitions available");
                emptyItem.AddToClassList("empty-transition-item");
                return emptyItem;
            };
            listView.makeItem = () =>
            {
                var item = new VisualElement();
                item.AddToClassList("transition-item");
                
                var indexLabel = new Label
                {
                    name = "IndexLabel"
                };
                indexLabel.AddToClassList("transition-index-label");
                item.Add(indexLabel);
                
                var icon = new Image
                {
                    image = BaseNodeView.ICON_ARROW_L.Texture
                };
                icon.AddToClassList("transition-icon");
                item.Add(icon);
                
                var nameLabel = new Label
                {
                    name = "NameLabel"
                };
                nameLabel.AddToClassList("transition-name-label");
                item.Add(nameLabel);
                
                var removeButton = new Button();
                removeButton.Add(new Image
                {
                    image = BaseNodeView.ICON_REMOVE.Texture
                });
                removeButton.AddToClassList("remove-transition-button");
                item.Add(removeButton);
                
                return item;
            };

            listView.bindItem = (element, index) =>
            {
                if (index >= transitionData.Count) return;
                
                var data = transitionData[index];
                
                var indexLabel = element.Q<Label>("IndexLabel");
                if (indexLabel != null)
                {
                    indexLabel.text = $"{index + 1}";
                }
                
                var nameLabel = element.Q<Label>("NameLabel");
                if (nameLabel != null)
                {
                    nameLabel.text = $"{data.DisplayName}";
                }
                
                var removeButton = element.Q<Button>();
                if (removeButton == null) return;

                // Ensure we're not registering multiple handlers
                removeButton.clickable = new Clickable(() => {});

                // Capture the current data in local variables to avoid closure issues
                var currentNode = node;
                var currentTargetNode = data.Node;
                var currentListView = listView;
                var currentTransitionData = transitionData;

                // Create a new callback that captures the current values
                void RemoveCallback()
                {
                    RemoveTransition(currentNode, currentTargetNode, currentListView, currentTransitionData);
                }

                // Add the callback - this will work correctly now
                removeButton.clicked += RemoveCallback;
            };

            // Enable drag and drop reordering
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            
            // Handle reordering - use itemsSourceChanged to detect when items are moved
            listView.itemsSourceChanged += () => 
            {
                // Execute immediately with try/catch to prevent errors
                try
                {
                    ReorderTransitions(node, transitionData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reordering transitions: {ex.Message}");
                }
            };

            container.Add(listView);
            
            return container;
        }

        private string GetTransitionDisplayName(BaseNode outputNode)
        {
            return outputNode.GetCustomName();
        }

        private void RemoveTransition(BaseGameCreatorNode sourceNode, BaseNode targetNode, ListView listView, List<TransitionData> transitionData)
        {
            // Find and remove the transition data
            var dataToRemove = transitionData.FirstOrDefault(t => t.Node == targetNode);
            if (dataToRemove == null) return;
            // Remove the actual edge connection from the graph first
            if (inspector.graphView != null)
            {
                // Find all edges from sourceNode to targetNode
                var edgesToRemove = inspector.graphView.edgeViews
                    .Where(edge => edge.userData is SerializableEdge serializableEdge &&
                                   serializableEdge.outputNode == sourceNode &&
                                   serializableEdge.inputNode == targetNode)
                    .ToList();
                    
                foreach (var edge in edgesToRemove)
                {
                    inspector.graphView.Disconnect(edge);
                }
                    
                // Save the graph
                inspector.graphView.SaveGraphToDisk();
            }
                
            // Remove from the local data
            transitionData.Remove(dataToRemove);
                
            // Update the list view's item source to reflect the change
            listView.itemsSource = transitionData;
            listView.RefreshItems();
                
            // Update the cache
            if (transitionsCache.ContainsKey(sourceNode))
            {
                transitionsCache[sourceNode] = (listView, transitionData);
            }
        }

        private void ReorderTransitions(BaseGameCreatorNode node, List<TransitionData> transitionData)
        {
            if (inspector.graphView == null) return;
            
            // Get the current order from the list view
            var newOrder = transitionData.Select(t => t.Node).ToList();
            
            // Update the execution order in the graph
            // This requires modifying the port order or edge order
            // For now, we'll update the compute order of the output nodes
            for (var i = 0; i < newOrder.Count; i++)
            {
                if (newOrder[i] is { } baseNode)
                {
                    baseNode.computeOrder = i;
                }
            }
            
            // Update the graph's compute order
            inspector.graphView.UpdateComputeOrder();
            
            // Save the graph
            inspector.graphView.SaveGraphToDisk();
        }

        private class TransitionData
        {
            public BaseNode Node { get; set; }
            public int OriginalIndex { get; set; }
            public string DisplayName { get; set; }
        }

        /// <summary>
        /// Runs a State Machine node (similar to Actions.Invoke())
        /// </summary>
        /// <param name="node">The node to run</param>
        private static void RunNode(BaseGameCreatorNode node)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (node == null) return;
            
            try
            {
                // Create a simple Args context for standalone node execution
                
                var context = node.Context;
                if (!context)
                {
                    // Try to find a StateMachineRunner in the scene
                    var runner = FindAnyObjectByType<StateMachineRunner>();
                    if (runner)
                    {
                        context = runner.gameObject;
                    }
                    else
                    {
                        // Create a temporary context for testing
                        context = new GameObject("TempNodeContext");
                        Debug.LogWarning($"Created temporary context for node execution: {context.name}. Consider adding a StateMachineRunner to the scene for proper context.");
                    }
                }
                
                // Execute the node directly without going through the state machine graph
                var args = new Args(context, context);
                node.OnProcess(args);
                
                Debug.Log($"Successfully executed node: {node.GetCustomName()}", context);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error running node {node.GetCustomName()}: {ex.Message}", node.Context);
            }
        }
    }

    /// <summary>
    /// Node inspector object, you can inherit from this class to customize your node inspector.
    /// </summary>
    public class NodeInspector : ScriptableObject
    {
        /// <summary>Previously selected object by the inspector</summary>
        public Object previouslySelectedObject;

        /// <summary>List of currently selected nodes</summary>
        public HashSet<BaseNodeView> selectedNodes { get; private set; } = new();

        /// <summary>Reference to the graph view for edge operations</summary>
        public BaseGraphView graphView { get; set; }

        /// <summary>Triggered when the selection is updated</summary>
        public event Action nodeSelectionUpdated;

        /// <summary>Triggered when a node view is removed from the graph</summary>
        public event Action<BaseNodeView> nodeViewRemoved;

        private void OnEnable()
        {
            // Ensure the HashSet is initialized
            if (selectedNodes == null)
            {
                selectedNodes = new HashSet<BaseNodeView>();
            }
        }

        /// <summary>Updates the selection from the graph</summary>
        public virtual void UpdateSelectedNodes(HashSet<BaseNodeView> views)
        {
            // Ensure we don't assign null
            if (views == null)
            {
                selectedNodes?.Clear();
            }
            else
            {
                selectedNodes = views;
            }
            nodeSelectionUpdated?.Invoke();
        }

        public virtual void RefreshNodes() => nodeSelectionUpdated?.Invoke();

        public virtual void NodeViewRemoved(BaseNodeView view)
        {
            if (view != null && selectedNodes != null)
            {
                selectedNodes.Remove(view);
                nodeSelectionUpdated?.Invoke();
                nodeViewRemoved?.Invoke(view);
            }
        }
    }
}