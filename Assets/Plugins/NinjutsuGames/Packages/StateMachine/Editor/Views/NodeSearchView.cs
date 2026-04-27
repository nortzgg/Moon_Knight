using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    public class NodeSearchView : VisualElement
    {
        private readonly BaseGraphView _graphView;
        private readonly TextField _searchField;
        private readonly ListView _resultsList;
        private readonly List<BaseNodeView> _searchResults = new();
        private readonly Label _noResultsLabel;
        private readonly Label _placeHolderLabel;

        private const string PlaceholderText = "Search node...";
        
        private static readonly IIcon ICON_SEARCH = new IconSearch(ColorTheme.Type.TextLight); // Placeholder icon
        private readonly string baseNodeStyle = "GraphProcessorStyles/NodeSearchView";
        
        public NodeSearchView(BaseGraphView graphView)
        {
            _graphView = graphView;
            
            RegisterCallback<DragUpdatedEvent>(e => { e.StopPropagation(); });
            
            // Stop mouse wheel propagation
            RegisterCallback<WheelEvent>(e => { e.StopPropagation(); });

            // Load and apply the USS stylesheet
            var styleSheet = Resources.Load<StyleSheet>(baseNodeStyle);
            if(styleSheet) styleSheets.Add(styleSheet);
            
            // Set up the main container
            AddToClassList("node-search-view"); // Use USS class
            name = "NodeSearchView"; // Keep the name for identification if needed
            
            // Create the search header
            var searchHeader = new VisualElement();
            searchHeader.AddToClassList("node-search-header");
            
            // Add search icon
            var searchIcon = new Image { image = ICON_SEARCH.Texture };
            searchIcon.pickingMode = PickingMode.Ignore; // Ignore mouse events
            searchIcon.AddToClassList("node-search-icon");
            
            _placeHolderLabel = new Label(PlaceholderText);
            _placeHolderLabel.AddToClassList("placeholder-text-style");
            _placeHolderLabel.pickingMode = PickingMode.Ignore; // Ignore mouse events
            
            // Add search field
            _searchField = new TextField();
            _searchField.AddToClassList("node-search-field");
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            _searchField.RegisterCallback<FocusInEvent>(OnSearchFieldFocusIn, TrickleDown.TrickleDown);
            _searchField.RegisterCallback<KeyDownEvent>(OnSearchFieldKeyDown, TrickleDown.TrickleDown);
            searchHeader.Add(_searchField);
            searchHeader.Add(_placeHolderLabel);
            searchHeader.Add(searchIcon);
            
            Add(searchHeader);
            
            // Create results container
            var resultsContainer = new VisualElement();
            resultsContainer.AddToClassList("node-search-results-container");
            
            // Add "No results" label
            _noResultsLabel = new Label("No nodes found");
            _noResultsLabel.AddToClassList("node-search-no-results");
            _noResultsLabel.style.display = DisplayStyle.None; // Initial state
            resultsContainer.Add(_noResultsLabel);
            
            // Add results list
            _resultsList = new ListView
            {
                makeItem = () => {
                    var label = new Label();
                    label.AddToClassList("node-search-list-item-label"); // USS class for list item labels
                    var itemContainer = new VisualElement(); // Create a container for each item
                    itemContainer.AddToClassList("node-search-list-item");
                    itemContainer.Add(label);
                    return itemContainer; 
                },
                bindItem = (element, i) => {
                    var label = element.Q<Label>(); // Get the label from the item container
                    if (label == null || i >= _searchResults.Count) return;
                    var nodeView = _searchResults[i];
                    label.text = nodeView.title;
                },
                itemsSource = _searchResults,
                selectionType = SelectionType.Single
            };
            _resultsList.makeNoneElement = () => null;
            _resultsList.selectionChanged += OnResultSelected;
            _resultsList.RegisterCallback<KeyDownEvent>(OnResultListKeyDown, TrickleDown.TrickleDown);
            _resultsList.focusable = true; 
            resultsContainer.Add(_resultsList);
            
            Add(resultsContainer);
            
            // Initially hide the search view
            style.display = DisplayStyle.None;
            
            // Register keyboard shortcut (Ctrl+F)
            graphView.RegisterCallback<KeyDownEvent>(evt => {
                if(evt.keyCode == KeyCode.Escape) 
                {
                    Hide();
                    evt.StopPropagation();
                    return;
                }

                if (evt.keyCode != KeyCode.F || !evt.ctrlKey) return;
                if (style.display == DisplayStyle.Flex)
                {
                    Hide();
                }
                else
                {
                    // Get the current mouse position in window space
                    Vector2 mousePosition = evt.originalMousePosition;
                    Show(mousePosition);
                }
                evt.StopPropagation();
            });

            // Hide when clicking outside
            graphView.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (style.display == DisplayStyle.Flex && !ContainsPoint(this.WorldToLocal(evt.mousePosition)))
                {
                    Hide();
                }
            }, TrickleDown.TrickleDown);
        }

        public void Show(Vector2 position)
        {
            // Position the search view at the mouse position
            style.position = Position.Absolute;
            style.left = position.x;
            style.top = position.y;
            
            // Ensure the view stays within the bounds of the graph view
            schedule.Execute(() => {
                var graphRect = _graphView.contentRect;
                var searchRect = this.worldBound;
                
                // Check if the search view extends beyond the right edge
                if (searchRect.xMax > graphRect.xMax)
                {
                    style.left = position.x - (searchRect.xMax - graphRect.xMax);
                }
                
                // Check if the search view extends beyond the bottom edge
                if (searchRect.yMax > graphRect.yMax)
                {
                    style.top = position.y - (searchRect.yMax - graphRect.yMax);
                }
            });
            
            style.display = DisplayStyle.Flex;
            _searchField.value = string.Empty;
            _searchField.Focus();
            UpdateSearch();
        }
        
        // Overload for backward compatibility
        public void Show()
        {
            // Default to center of the graph view
            Vector2 centerPosition = new Vector2(
                _graphView.contentRect.width / 2,
                _graphView.contentRect.height / 2
            );
            Show(centerPosition);
        }
        
        public void Hide()
        {
            style.display = DisplayStyle.None;
            // Clear any highlights when hiding
            foreach (var nodeView in _graphView.nodeViews)
            {
                ClearNodeHighlight(nodeView);
            }
            _resultsList.selectedIndex = -1; // Clear selection when hiding
        }
        
        private void OnSearchFieldFocusIn(FocusInEvent evt)
        {
            _resultsList?.ClearSelection();
        }
        
        private void OnResultListKeyDown(KeyDownEvent evt)
        {
            if (_resultsList.itemsSource == null || _resultsList.itemsSource.Count == 0)
            {
                return;
            }

            var currentIndex = _resultsList.selectedIndex;
            var itemCount = _resultsList.itemsSource.Count;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    // If at the last item, go back to the input field
                    if (currentIndex == itemCount - 1)
                    {
                        _resultsList.ClearSelection();
                        _searchField.Focus();
                        evt.StopPropagation();
                    }
                    break;
                case KeyCode.UpArrow:
                    // If at the first item, go back to the input field
                    if (currentIndex == 0)
                    {
                        _resultsList.ClearSelection();
                        _searchField.Focus();
                        evt.StopPropagation();
                    }
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (currentIndex >= 0 && currentIndex < _searchResults.Count)
                    {
                        HighlightAndFocusNode(_searchResults[currentIndex]);
                        Hide();
                        evt.StopPropagation();
                    }
                    break;
            }
        }
        
        private void OnSearchFieldKeyDown(KeyDownEvent evt)
        {
            if (_resultsList.itemsSource == null || _resultsList.itemsSource.Count == 0)
            {
                return;
            }

            var itemCount = _resultsList.itemsSource.Count;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    // When in the input field, pressing down selects the first item
                    _resultsList.Focus();
                    _resultsList.selectedIndex = 0;
                    _resultsList.ScrollToItem(0);
                    if (_searchResults.Count > 0)
                    {
                        HighlightAndFocusNode(_searchResults[0]);
                    }
                    evt.StopPropagation();
                    break;
                case KeyCode.UpArrow:
                    // When in the input field, pressing up selects the last item
                    _resultsList.Focus();
                    _resultsList.selectedIndex = itemCount - 1;
                    _resultsList.ScrollToItem(itemCount - 1);
                    if (_searchResults.Count > 0 && itemCount > 0)
                    {
                        HighlightAndFocusNode(_searchResults[itemCount - 1]);
                    }
                    evt.StopPropagation();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    // If there's only one result, select it
                    if (_searchResults.Count == 1)
                    {
                        HighlightAndFocusNode(_searchResults[0]);
                        Hide();
                        evt.StopPropagation();
                    }
                    break;
            }
        }
        
        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            UpdateSearch();
        }
        
        private void UpdateSearch()
        {
            _searchResults.Clear();
            _placeHolderLabel.style.display = string.IsNullOrEmpty(_searchField.value) ? DisplayStyle.Flex : DisplayStyle.None;
            
            var searchText = _searchField.value.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(searchText))
            {
                // Find all nodes that match the search text
                foreach (var nodeView in _graphView.nodeViews)
                {
                    var nodeTitle = nodeView.title.ToLowerInvariant();
                    if (nodeTitle.Contains(searchText))
                    {
                        _searchResults.Add(nodeView);
                    }
                }
                
                // Sort results by relevance (exact matches first, then starts with, then contains)
                _searchResults.Sort((a, b) => {
                    var aTitle = a.title.ToLowerInvariant();
                    var bTitle = b.title.ToLowerInvariant();
                    
                    // Exact match
                    if (aTitle == searchText && bTitle != searchText)
                        return -1;
                    if (bTitle == searchText && aTitle != searchText)
                        return 1;
                    
                    // Starts with
                    if (aTitle.StartsWith(searchText) && !bTitle.StartsWith(searchText))
                        return -1;
                    if (bTitle.StartsWith(searchText) && !aTitle.StartsWith(searchText))
                        return 1;
                    
                    // Alphabetical
                    return string.Compare(aTitle, bTitle, StringComparison.Ordinal);
                });
            }
            
            // Update the list view
            _resultsList.itemsSource = _searchResults;
            _resultsList.Rebuild();
            
            // Show/hide "No results" label
            _noResultsLabel.style.display = _searchResults.Count == 0 && !string.IsNullOrEmpty(searchText) ? DisplayStyle.Flex : DisplayStyle.None;
            
            // Clear any previous highlights
            foreach (var nodeView in _graphView.nodeViews)
            {
                ClearNodeHighlight(nodeView);
            }
            
            // If there's only one result, select it automatically
            if (_searchResults.Count == 1)
            {
                _resultsList.selectedIndex = 0;
                HighlightAndFocusNode(_searchResults[0]);
            }
        }
        
        private void OnResultSelected(IEnumerable<object> selectedItems)
        {
            if (selectedItems.FirstOrDefault() is BaseNodeView selectedItem)
            {
                HighlightAndFocusNode(selectedItem);
            }
        }
        
        private void HighlightAndFocusNode(BaseNodeView nodeView)
        {
            // Clear any previous highlights
            foreach (var nv in _graphView.nodeViews)
            {
                ClearNodeHighlight(nv);
            }
            
            // Highlight the selected node
            HighlightNode(nodeView);
            
            // Center the view on the node
            var nodeRect = nodeView.GetPosition();
            var nodeCenter = nodeRect.center;
            var position = new Vector3(-nodeCenter.x, -nodeCenter.y, 0) * _graphView.viewTransform.scale.x; // Use x scale, assuming uniform scaling
            position.x += _graphView.contentRect.size.x / 2;
            position.y += _graphView.contentRect.size.y / 2;
            _graphView.UpdateViewTransform(position, _graphView.viewTransform.scale);
            
            // Select the node
            _graphView.ClearSelection();
            _graphView.AddToSelection(nodeView);
        }
        
        // Helper methods to highlight/clear node visuals
        private void HighlightNode(BaseNodeView nodeView)
        {
            // Add a visual highlight to the node
            nodeView.AddToClassList("highlighted-node");
            nodeView.Highlight(null);
            
            // Add a pulsing animation
            // Note: VisualElement.experimental.animation is not available in all Unity versions.
            // You might need to implement a custom animation or use a third-party library if this doesn't work.
            // For simplicity, we'll just change the scale directly here.
            nodeView.transform.scale = new Vector3(1.05f, 1.05f, 1f);
        }
        
        private void ClearNodeHighlight(BaseNodeView nodeView)
        {
            nodeView.ClearHighlight();
            nodeView.RemoveFromClassList("highlighted-node");
            nodeView.transform.scale = Vector3.one;
        }
    }
}