using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomToolbarView : ToolbarView
{
    private static readonly IIcon ICON_SEARCH = new IconSearch(ColorTheme.Type.TextLight); // Placeholder icon
    private static readonly IIcon ICON_MAP = new IconUICanvasGroup(ColorTheme.Type.TextLight);
    
    private readonly MiniMapView miniMapView;
    private static Image searchIcon;
    private static Image minimapIcon;

    public CustomToolbarView(BaseGraphView graphView, MiniMapView miniMapView) : base(graphView)
    {
        this.miniMapView = miniMapView;
    }

    protected override void AddButtons()
    {
        // Add the hello world button on the left of the toolbar
        // AddButton("Hello !", () => Debug.Log("Hello World"), left: false);

        // add the default buttons (center, show processor and show in project)
        base.AddButtons();

        // Local Search Button (search within current graph)
        var searchButton = new ToolbarButton(() =>
            {
                var centerPosition = new Vector2(
                    graphView.contentRect.width - 420,
                    40
                );
                NodeSearchPopup.Open(centerPosition, graphView);
            }) 
        {
            tooltip = "Search Nodes in Current Graph",
            style =
            {
                flexDirection = FlexDirection.Row,
            }
        };
        searchIcon = new Image { image = ICON_SEARCH.Texture };
        searchIcon.style.width = searchIcon.style.height = 14;
        searchButton.Add(searchIcon);
        var searchLabel = new Label("Search");
        searchButton.Add(searchLabel);
        toolbar.Add(searchButton);

        // Minimap Toggle
        var minimapToggle = new ToolbarToggle
        {
            tooltip = "Toggle Minimap",
            value = miniMapView.visible,
        };
        minimapIcon = new Image { image = ICON_MAP.Texture }; 
        minimapToggle.Add(minimapIcon);
        var minimapLabel = new Label("Minimap");
        minimapToggle.Add(minimapLabel);
        minimapToggle.RegisterValueChangedCallback(evt =>
        {
            miniMapView.visible = evt.newValue;
            if (!graphView.graph) return;
            graphView.graph.minimapVisible = evt.newValue;
            EditorUtility.SetDirty(graphView.graph);
        });
        toolbar.Add(minimapToggle);
    }

    public override void UpdateButtonStatus()
    {
        base.UpdateButtonStatus();
        searchIcon.image = ICON_SEARCH.Texture;
        minimapIcon.image = ICON_MAP.Texture;
    }
}