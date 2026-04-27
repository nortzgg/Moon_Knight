using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    public class ToolbarView : VisualElement
    {
        public static ToolbarView Instance { get; private set; }
        
        protected BaseGraphView graphView;
        protected readonly Toolbar toolbar;
        private static Image expandIcon;
        private static Image blackboardIcon;
        private static Image openIcon;
        private static Image globalSearchIcon;

        private static readonly IIcon ICON_EXPAND = new IconExpand(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_COLLAPSE = new IconCollapse(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_BLACKBOARD = new IconListVariable(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_OPEN = new IconStateMachine(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_GLOBAL_SEARCH = new IconSearch(ColorTheme.Type.TextLight);

        public ToolbarView(BaseGraphView graphView)
        {
            name = "ToolbarView";
            this.graphView = graphView;

            graphView.initialized += AddButtons;

            toolbar = new Toolbar
            {
                style =
                {
                    height = 30
                }
            };
            Add(toolbar);
            Instance = this;
            
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.EnteredEditMode)
            {
                UpdateButtonStatus();
            }
        }
        
        protected virtual void AddButtons()
        {
            toolbar.Clear();
            var openButton = new ToolbarButton(OpenFilePopup.Open)
            {
                tooltip = "Open State Machine asset"
            };
            openIcon = new Image { image = ICON_OPEN.Texture };
            openButton.Add(openIcon);
            var openLabel = new Label("Open");
            openButton.Add(openLabel);
            var centerButton = new ToolbarButton(() => { graphView.ResetPositionAndZoom(); }) { text = "Center" };
            var blackboardToggle = new ToolbarToggle
            {
                value = graphView.GetPinnedElementStatus<BlackboardView>() != DropdownMenuAction.Status.Hidden
            };
            blackboardIcon = new Image { image = ICON_BLACKBOARD.Texture };
            blackboardToggle.Add(blackboardIcon);
            var blackboardLabel = new Label("Blackboard");
            blackboardToggle.tooltip = "Open Blackboard";
            blackboardToggle.Add(blackboardLabel);
            blackboardToggle.RegisterValueChangedCallback(evt =>
            {
                graphView.ToggleView<BlackboardView>();
            });
            var expandNodesToggle = new ToolbarToggle
            {
                value = graphView.NodesExpanded
            };
            expandIcon = new Image { image = graphView.NodesExpanded ? ICON_COLLAPSE.Texture : ICON_EXPAND.Texture };
            expandNodesToggle.tooltip = "Expand or Collapse Nodes";
            expandNodesToggle.Add(expandIcon);
            var nodesLabel = new Label("Nodes");
            expandNodesToggle.Add(nodesLabel);
            expandNodesToggle.value = graphView.NodesExpanded;
            expandNodesToggle.RegisterValueChangedCallback(evt =>
            {
                graphView.NodesExpanded = evt.newValue;
                expandIcon.image = evt.newValue ? ICON_COLLAPSE.Texture : ICON_EXPAND.Texture;
                // nodesLabel.text = evt.newValue ? "Collapse Nodes" : "Expand Nodes";
            });
            // Global Search Button (search across all state machines)
            var globalSearchButton = new ToolbarButton(StateMachineSearchProvider.OpenSearchWindow) 
            {
                tooltip = "Search Nodes Across All State Machines",
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };
            globalSearchIcon = new Image { image = ICON_GLOBAL_SEARCH.Texture };
            globalSearchIcon.style.width = globalSearchIcon.style.height = 14;
            globalSearchButton.Add(globalSearchIcon);
            var globalSearchLabel = new Label("Global");
            globalSearchButton.Add(globalSearchLabel);
            toolbar.Add(globalSearchButton);
            toolbar.Add(openButton);
            toolbar.Add(centerButton);
            toolbar.Add(blackboardToggle);
            toolbar.Add(expandNodesToggle);
            
            toolbar.Add(new ToolbarSpacer(){flex = true});
            toolbar.Add(new ToolbarSpacer());
            
            var showInProjectButton = new ToolbarButton(() => { EditorGUIUtility.PingObject(graphView.graph); }) { text = "Select" };
            showInProjectButton.tooltip = "Select asset in project view";
            toolbar.Add(showInProjectButton);
        }

        public virtual void UpdateButtonStatus()
        {
            // if (showParameters != null)
                // showParameters.value = graphView.GetPinnedElementStatus<BlackboardView>() != DropdownMenuAction.Status.Hidden;
            expandIcon.image = graphView.NodesExpanded ? ICON_COLLAPSE.Texture : ICON_EXPAND.Texture;
            blackboardIcon.image = ICON_BLACKBOARD.Texture;
            openIcon.image = ICON_OPEN.Texture;
            globalSearchIcon.image = ICON_GLOBAL_SEARCH.Texture;
        }
    }
}