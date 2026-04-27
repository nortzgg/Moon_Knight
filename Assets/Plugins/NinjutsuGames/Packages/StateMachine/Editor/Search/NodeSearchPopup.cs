using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Runtime.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    public class NodeSearchPopup : EditorWindow
    {
        private BaseGraphView _graphView;
        private TextField _searchField;
        private ListView _resultsList;
        private readonly List<BaseNodeView> _searchResults = new();
        private Label _noResultsLabel;

        private const string PlaceholderText = "Search node...";
        
        private static readonly IIcon ICON_SEARCH = new IconSearch(ColorTheme.Type.TextLight); // Placeholder icon
        private readonly string baseNodeStyle = "GraphProcessorStyles/NodeSearchPopup";
        
        private const int WIDTH = 400;
        private const int MIN_HEIGHT = 43;
        private const int ITEM_HEIGHT = 24;
        
        private const int TRANSITION_DURATION = 350;

        // STATIC PROPERTIES: ---------------------------------------------------------------------

        private static NodeSearchPopup Window;

        // MEMBERS: -------------------------------------------------------------------------------

        private string m_TypeTitle;
        
        private Type m_Type;
        private Action<Type> m_OnSelect;
        
        private VisualElement m_Container;
        private VisualElement m_Head;
        private VisualElement m_Body;
        private VisualElement m_Foot;
        
        private TextField m_SearchField;
        
        private VisualElement m_ContentSearch;


        // INITIALIZERS: --------------------------------------------------------------------------

        public static void Open(Vector2 position, BaseGraphView graphView)
        {
            if (Window)
            {
                Window.Close();
                return;
            }

            Window = CreateInstance<NodeSearchPopup>();
            Window._graphView = graphView;

            var rectActivator = new Rect(
                focusedWindow.position.x + position.x,
                focusedWindow.position.y + position.y - 80,
                400,
                100
            );
            
            var initialSize = new Vector2(
                WIDTH,
                MIN_HEIGHT
            );
            
            Window.ShowAsDropDown(rectActivator, initialSize);
            Window.minSize = new Vector2(WIDTH, MIN_HEIGHT);
            Window.maxSize = new Vector2(WIDTH, 600);
            
            // Register for layout events to auto-resize based on content
            Window.rootVisualElement.RegisterCallback<GeometryChangedEvent>(Window.OnGeometryChanged);
            Window._searchField.value = string.Empty;
            Window._searchField.Focus();
            Window.UpdateSearch();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ResizeWindowBasedOnContent();
        }

        private void OnDestroy()
        {
            Window = null;
        }

        private void CreateGUI()
        {
            
            rootVisualElement.RegisterCallback<DragUpdatedEvent>(e => { e.StopPropagation(); });
            
            // Stop mouse wheel propagation
            rootVisualElement.RegisterCallback<WheelEvent>(e => { e.StopPropagation(); });
            
            rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Escape) return;
                Close();
                evt.StopPropagation();
            }, TrickleDown.TrickleDown);

            // Load and apply the USS stylesheet
            var styleSheet = Resources.Load<StyleSheet>(baseNodeStyle);
            if(styleSheet) rootVisualElement.styleSheets.Add(styleSheet);
            
            // Set up the main container
            rootVisualElement.AddToClassList("node-search-view"); // Use USS class
            name = "NodeSearchView"; // Keep the name for identification if needed
            
            // Create the search header
            var searchHeader = new VisualElement();
            searchHeader.AddToClassList("node-search-header");
            
            // Add search icon
            var searchIcon = new Image
            {
                image = ICON_SEARCH.Texture,
                pickingMode = PickingMode.Ignore // Ignore mouse events
            };
            searchIcon.AddToClassList("node-search-icon");
            
            // Add search field
            _searchField = new TextField
            {
                textEdition =
                {
                    placeholder = PlaceholderText,
                },
                style =
                {
                    backgroundImage = ICON_SEARCH.Texture
                }
            };
            _searchField.AddToClassList("node-search-field");
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            _searchField.RegisterCallback<FocusInEvent>(OnSearchFieldFocusIn, TrickleDown.TrickleDown);
            _searchField.RegisterCallback<KeyDownEvent>(OnSearchFieldKeyDown, TrickleDown.TrickleDown);
            searchHeader.Add(_searchField);
            // searchHeader.Add(searchIcon);
            
            rootVisualElement.Add(searchHeader);
            
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
                selectionType = SelectionType.Single,
                makeNoneElement = () => null,
                fixedItemHeight = ITEM_HEIGHT,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            _resultsList.selectionChanged += OnResultSelected;
            _resultsList.RegisterCallback<KeyDownEvent>(OnResultListKeyDown, TrickleDown.TrickleDown);
            _resultsList.focusable = true; 
            resultsContainer.Add(_resultsList);
            
            rootVisualElement.Add(resultsContainer);
        }

        private void OnDisable()
        {
            _graphView?.ClearSelection();
            
            // Clear any highlights when hiding
            if (_graphView?.nodeViews != null)
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
                        // Hide();
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
                        // Hide();
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
            
            // Auto-resize the window based on content
            ResizeWindowBasedOnContent();
        }
        
        private void ResizeWindowBasedOnContent()
        {
            // Calculate the ideal height based on the number of search results
            var headerHeight = MIN_HEIGHT; // Approximate height of the search header
            var footerHeight = 10; // Extra padding at the bottom
            var noResultsHeight = 30; // Height of the "No results" label
            
            // Calculate the height needed for the results
            var resultsCount = _searchResults.Count;
            var maxVisibleItems = Math.Min(resultsCount, 10); // Limit to 10 visible items max
            var resultsHeight = maxVisibleItems * ITEM_HEIGHT;
            
            // If no results but search text exists, show the no results label
            if (resultsCount == 0 && !string.IsNullOrEmpty(_searchField.value))
            {
                resultsHeight = noResultsHeight;
            }
            
            // Calculate total height
            var totalHeight = headerHeight + resultsHeight + (resultsCount > 0 ? footerHeight : 0);
            
            // Apply the new size
            minSize = new Vector2(WIDTH, totalHeight);
            maxSize = new Vector2(WIDTH, totalHeight);
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
            rootVisualElement.experimental.animation.Start(
                _graphView.viewTransform.position, position, TRANSITION_DURATION,
                (e, pos) => _graphView.UpdateViewTransform(pos, _graphView.viewTransform.scale)
            );
            
            // Select the node
            // _graphView.ClearSelection();
            // _graphView.AddToSelection(nodeView);
        }

        // Helper methods to highlight/clear node visuals
        private void HighlightNode(BaseNodeView nodeView)
        {
            // Add a visual highlight to the node
            nodeView.AddToClassList("highlighted-node");
            nodeView.Highlight(null);
            
            nodeView.experimental.animation.Start(
                nodeView.transform.scale, new Vector3(1.1f, 1.1f, 1f), TRANSITION_DURATION,
                (e, scale) =>  nodeView.transform.scale = scale
            );
        }
        
        private void ClearNodeHighlight(BaseNodeView nodeView)
        {
            nodeView.ClearHighlight();
            nodeView.RemoveFromClassList("highlighted-node");
            nodeView.experimental.animation.Start(
                nodeView.transform.scale, Vector3.one, TRANSITION_DURATION,
                (e, scale) =>  nodeView.transform.scale = scale
            );
        }
    }
}