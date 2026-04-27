using System;
using System.Linq;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NinjutsuGames.StateMachine.Editor
{
    [Serializable]
    public abstract class BaseGraphWindow : EditorWindow
    {
        protected VisualElement rootView;
        protected VisualElement emptyView;
        protected BaseGraphView graphView;
        
        public StateMachineAsset Graph => graph;

        [SerializeField] protected StateMachineAsset graph;

        private readonly string graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

        public bool IsGraphLoaded => graphView != null && graphView.graph != null;
        public bool ShouldReload { get; set; }

        private bool reloadWorkaround;
        protected StateMachineRunner selectedRunner;

        // public event Action<StateMachineAsset> graphLoaded;
        // public event Action<StateMachineAsset> graphUnloaded;

        /// <summary>
        /// Called by Unity when the window is enabled / opened
        /// </summary>
        protected virtual void OnEnable()
        {
            InitializeRootView();

            if (graph != null)
                LoadGraph();
            else
                reloadWorkaround = true;
        }

        protected virtual void Update()
        {
            // Workaround for the Refresh option of the editor window:
            // When Refresh is clicked, OnEnable is called before the serialized data in the
            // editor window is deserialized, causing the graph view to not be loaded
            if (reloadWorkaround && graph != null)
            {
                LoadGraph();
                reloadWorkaround = false;
            }
        }

        private void LoadGraph()
        {
            // We wait for the graph to be initialized
            if (graph.isEnabled) InitializeGraph(graph);
            else graph.onEnabled += () => InitializeGraph(graph);
        }

        /// <summary>
        /// Called by Unity when the window is disabled (happens on domain reload)
        /// </summary>
        protected virtual void OnDisable()
        {
            if (graphView == null) return;

            // Use IDisposable for better resource management
            using (var cleanupScope = new EditorGraphCleanupScope(graphView))
            {
                graphView.CleanUp();
                if(graph) graphView.SaveGraphToDisk();
            }
        }

        /// <summary>
        /// Helper class to ensure graph cleanup happens properly using IDisposable pattern
        /// </summary>
        private class EditorGraphCleanupScope : IDisposable
        {
            private readonly BaseGraphView _graphView;

            public EditorGraphCleanupScope(BaseGraphView graphView)
            {
                _graphView = graphView;
            }

            public void Dispose()
            {
                // Perform any additional cleanup that might be needed
                // This ensures resources are released even if exceptions occur
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Called by Unity when the window is closed
        /// </summary>
        protected virtual void OnDestroy()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnFocus()
        {
            if(graph != null)
            {
                StateMachineAsset.Active = graph;
            }

            if (!ShouldReload) return;
            ShouldReload = false;
            Debug.Log($"Reloaded State Machine Graph");
            LoadGraph();
        }

        private void InitializeRootView()
        {
            rootView = rootVisualElement;
            rootView.name = "graphRootView";
            rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
            emptyView = new VisualElement
            {
                name = "emptyView",
                style =
                {
                    flexGrow = 1
                }
            };
            var titleLabel = new Button
            {
                text = "Open a State Machine"
            };
            titleLabel.clicked += () =>
            {
                SearchService.ShowObjectPicker(null, OnObjectPickerSelect, null, null, typeof(StateMachineAsset));
            };
            titleLabel.AddToClassList("state-machine-empty");
            emptyView.Add(titleLabel);
            rootView.Add(emptyView);
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnObjectPickerSelect(Object stateMachine)
        {
            if(!stateMachine) return;
            if(stateMachine is not StateMachineAsset asset) return;
            InitializeGraph(asset);
        }

        protected virtual void OnSelectionChanged()
        {
            if (!Selection.activeGameObject) return;
            if(graph != null) return;
            var runner = Selection.activeGameObject.GetComponent<StateMachineRunner>();
            if(runner && runner.stateMachineAsset) InitializeGraph(runner.stateMachineAsset);
        }

        public void InitializeGraph(StateMachineAsset graph)
        {
            // Validate the graph before initialization
            if (graph != null && !ValidateGraph(graph))
            {
                Debug.LogWarning($"Graph validation failed for '{graph.name}'. Some features may not work correctly.");
            }
            
            if (this.graph != null && graph != this.graph)
            {
                // Save the current graph if dirty
                if(EditorUtility.IsDirty(this.graph)) 
                {
                    EditorUtility.SetDirty(this.graph);
                    AssetDatabase.SaveAssets();
                }
            }

            this.graph = graph;
            if (graph != null)
            {
                StateMachineAsset.Active = graph;
            }

            if (graphView != null && graphView.parent == rootView) rootView.Remove(graphView);

            //Initialize will provide the BaseGraphView
            InitializeWindow(graph);

            graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

            if (graphView == null)
            {
                Debug.LogError("GraphView has not been added to the BaseGraph root view !");
                return;
            }

            graphView.Initialize(graph);
            InitializeGraphView(graphView);

            if (graph != null)
            {
                SetupGraphSceneHandling(graph);
            }
        }
        
        private bool ValidateGraph(StateMachineAsset graph)
        {
            if (graph == null) return false;
            
            // Check if this is an embedded graph and validate it
            if (graph.IsLinkedToScene())
            {
                var linkedScene = graph.GetLinkedScene();
                if (!linkedScene.IsValid())
                {
                    Debug.LogWarning($"Embedded graph '{graph.name}' is linked to an invalid scene");
                    return false;
                }
                
                // Check if there's a corresponding runner
                var runner = FindEmbeddedGraphRunner(graph);
                if (runner == null)
                {
                    Debug.LogWarning($"Embedded graph '{graph.name}' has no corresponding StateMachineRunner");
                    return false;
                }
                
                // Validate runner state
                if (!runner.isEmbedded)
                {
                    Debug.LogWarning($"Graph '{graph.name}' is linked to scene but runner '{runner.name}' is not marked as embedded");
                    return false;
                }
                
                if (runner.stateMachineAsset != graph)
                {
                    Debug.LogWarning($"Runner '{runner.name}' asset reference doesn't match graph '{graph.name}'");
                    return false;
                }
            }
            
            return true;
        }
        
        private StateMachineRunner FindEmbeddedGraphRunner(StateMachineAsset graph)
        {
            var runners = FindObjectsByType<StateMachineRunner>(FindObjectsSortMode.None);
            return runners.FirstOrDefault(r => r.isEmbedded && r.stateMachineAsset == graph);
        }
        
        private void SetupGraphSceneHandling(StateMachineAsset graph)
        {
            if (graph.IsLinkedToScene()) 
            {
                LinkGraphWindowToScene(graph.GetLinkedScene());
            }
            else 
            {
                graph.onSceneLinked += LinkGraphWindowToScene;
            }
            
            void LinkGraphWindowToScene(Scene scene)
            {
                EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;
                
                void CloseWindowWhenSceneIsClosed(Scene closedScene)
                {
                    EditorApplication.delayCall += ShouldClose;
                    
                    void ShouldClose()
                    {
                        EditorApplication.delayCall -= ShouldClose;
                        
                        // Handle embedded graph cleanup when scene closes
                        if (closedScene == scene)
                        {
                            var runner = FindEmbeddedGraphRunner(graph);
                            if (runner != null && !runner.gameObject.scene.IsValid())
                            {
                                Debug.Log($"Scene containing embedded graph '{graph.name}' was closed");
                                Unload();
                                EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
                                return;
                            }
                        }
                        
                        // Find StateMachineRunner in current scene
                        var stateMachineRunner = FindFirstObjectByType<StateMachineRunner>();
                        if (stateMachineRunner && graph == null)
                        {
                            graph = stateMachineRunner.stateMachineAsset;
                        }

                        if (scene.Equals(SceneManager.GetActiveScene()))
                        {
                            InitializeGraph(graph);
                            return;
                        }
                    
                        Unload();
                        EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
                    }
                }
            }
        }
        
        private void Unload()
        {
            if(graphView != null && rootView.Contains(graphView)) rootView.Remove(graphView);
            graphView = null;
        }

        public virtual void OnGraphDeleted()
        {
            if (graph != null && graphView != null)
                rootView.Remove(graphView);

            graphView = null;
        }

        protected abstract void InitializeWindow(StateMachineAsset graph);

        protected virtual void InitializeGraphView(BaseGraphView view)
        {
        }
    }
}