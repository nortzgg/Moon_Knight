using GameCreator.Editor.Hub;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{

	public class StateMachineGraphWindow : BaseGraphWindow
	{
		private const int MIN_WIDTH = 800;
		private const int MIN_HEIGHT = 600;
		private static IIcon icon;

		private CustomToolbarView toolbarView;
		private MiniMapView minimap;
		
		private Label titleLabel;
		private Label pathLabel;

		protected override void OnDestroy()
		{
			base.OnDestroy();
			graphView?.Dispose();
		}
		
		[MenuItem("Game Creator/State Machine")]
		public static void ShowWindow()
		{
			var window = GetWindow<StateMachineGraphWindow>("State Machine");
			window.minSize = new Vector2(1000, 600);
			window.titleContent = new GUIContent("\u00A0\u00A0State Machine");
		}

		protected override void InitializeWindow(StateMachineAsset graph)
		{
			icon ??= new IconWindowHub(ColorTheme.Type.TextLight);
			titleContent = new GUIContent("State Machine", icon.Texture);
			minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);

			if (graphView == null)
			{
				graphView = new StateMachineGraphView(this);
				graphView.initialized -= UpdateInfo;
				graphView.initialized += UpdateInfo;
				minimap = new MiniMapView(graphView)
				{
					anchored = true,
					style =
					{
						backgroundColor = Color.clear,
						borderBottomColor = Color.clear,
						borderLeftColor = Color.clear,
						borderRightColor = Color.clear,
						borderTopColor = Color.clear,
					}
				};
				minimap.SetPosition(new Rect(1, 30, 200, 100));
				minimap.UpdatePresenterPosition();
				minimap.visible = graph.minimapVisible;
				graphView.Add(minimap);
				
				toolbarView = new CustomToolbarView(graphView, minimap);
				graphView.Add(toolbarView);
				
				var grid = new GridBackground();
				graphView.Insert(0, grid);
				grid.StretchToParentSize();
			}
			graphView.graph = graph;
			rootView.Add(graphView);
		}
		
		private void UpdateInfo()
		{
			if(graphView == null) return;
			if(titleLabel == null)
			{
				var root = new VisualElement();
				root.name = "StateMachineInfo";
				titleLabel = new Label();
				titleLabel.AddToClassList("StateMachineTitle");
				pathLabel = new Label();
				pathLabel.AddToClassList("StateMachinePath");
				root.Add(titleLabel);
				root.Add(pathLabel);
				graphView.Insert(1, root);
				root.pickingMode = PickingMode.Ignore;
				root.StretchToParentSize();
			}

			var path = string.Empty;
			if (StateMachineAsset.Active)
			{
				path = AssetDatabase.GetAssetPath(StateMachineAsset.Active);
				if (string.IsNullOrEmpty(path))
				{
					path = SceneManager.GetActiveScene().path;
				}
			}
			titleLabel.text = StateMachineAsset.Active ? StateMachineAsset.Active.name : "Select a State Machine";
			pathLabel.text = path;
		}

		protected override void OnSelectionChanged()
		{
			base.OnSelectionChanged();
			if(Selection.activeGameObject == null) return;
			var runner = Selection.activeGameObject.GetComponent<StateMachineRunner>();
			if(runner != null && runner.stateMachineAsset && runner.stateMachineAsset == StateMachineAsset.Active)
			{
				pathLabel.text = $"Runner: {runner.name}";
			}
			else
			{
				UpdateInfo();
			}
		}

		protected override void InitializeGraphView(BaseGraphView view)
		{
			toolbarView.UpdateButtonStatus();
			
			// Ensure the asset is marked as the active one
			if (graphView?.graph != null)
			{
				StateMachineAsset.Active = graphView.graph as StateMachineAsset;
				
				// Subscribe to graph changes to ensure variables are properly synchronized
				graphView.graph.onGraphChanges -= OnGraphChanges;
				graphView.graph.onGraphChanges += OnGraphChanges;
			}
		}
		
		private void OnGraphChanges(GraphChanges changes)
		{
			// When graph changes occur, ensure the asset is saved
			if (!graphView?.graph) return;
			EditorUtility.SetDirty(graphView.graph);
				
			// Find any runners using this asset and update them
			var runners = FindObjectsByType<StateMachineRunner>(FindObjectsSortMode.None);
			foreach (var runner in runners)
			{
				if (runner.stateMachineAsset == graphView.graph)
				{
					// Update the runner's variables from the asset
					var serializedObject = new SerializedObject(runner);
					serializedObject.Update();
					if (StateMachineRunnerEditor.Instance)
					{
						StateMachineRunnerEditor.Instance.SyncVariables(true);
					}
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}
