using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Editor.Common;
using GameCreator.Editor.Variables;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    public class NameListView : TListView<Runtime.NameVariableRuntime>
    {
        private const string USS_PATH = EditorPaths.VARIABLES + "StyleSheets/NameList";

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string USSPath => USS_PATH;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public NameListView(Runtime.NameVariableRuntime runtime) : base(runtime)
        {
            runtime.EventChange -= OnChange;
            runtime.EventChange += OnChange;
        }
        
        ~NameListView()
        {
            m_Runtime.EventChange -= OnChange;
        }

        private void OnChange(string name)
        {
            Refresh();
        }

        // IMPLEMENTATIONS: -----------------------------------------------------------------------

        protected override void Refresh()
        {
            base.Refresh();
            using var enumerator = m_Runtime?.GetEnumerator();
            if (enumerator == null) return;

            foreach (var variable in m_Runtime)
            {
                Add(new NameVariableView(variable));
            }
        }
    }

    [CustomPropertyDrawer(typeof(Runtime.NameVariableRuntime))]
    public class NameVariablesRuntimeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var list = property.FindPropertyRelative("m_List");
            var runtime = property.GetValue<Runtime.NameVariableRuntime>();

            var target = property.serializedObject.targetObject;
            var isPrefab = PrefabUtility.IsPartOfPrefabAsset(target);
            switch (EditorApplication.isPlayingOrWillChangePlaymode && !isPrefab)
            {
                case true:
                    root.Add(new NameListView(runtime));
                    break;

                case false:
                    root.Add(new RunnerNameListTool(list, "Variables"));
                    break;
            }

            return root;
        }
    }

    [CustomEditor(typeof(StateMachineRunner))]
    public class StateMachineRunnerEditor : UnityEditor.Editor
    {
        private const string USS_PATH = EditorPaths.COMMON + "Structures/Save/StyleSheets/Save";
        public const string CLASS_HEAD = "gc-save-root";
        public const string NAME_HEAD = "GC-ListName-List-Head";
        private const float BUTTON_WIDTH = 70;

        private static readonly Length ERROR_MARGIN = new(10, LengthUnit.Pixel);
        private const string ERR_DUPLICATE_ID = "Another Runner component has the same ID";
        private const string EMBED_INFO = "Embedded graph only works in the scene that it was embedded. Can't be stored in Prefabs.";
        private const string CLASS_HEAD_BUTTON = "gc-save-btn";
        private static IIcon ICON_Embed;
        private static IIcon ICON_Detach;
        private static IIcon ICON_Open;
        private static IIcon ICON_OpenNew;
        private static IIcon ICON_Delete;
        private static IIcon ICON_Edit;
        
        // MEMBERS: -------------------------------------------------------------------------------

        private ErrorMessage m_Error;
        private InfoMessage m_Info;
        private StateMachineRunner runner;
        private RunnerNameListTool fieldRunnerList;
        private RunnerNameSubListTool fieldSubList;
        private SerializedProperty propertyList;
        private SerializedProperty propertySubList;
        private StateMachineAsset _lastAsset;
        private Button embedButton;
        private Image embedIcon;
        private Label embedLabel;
        private Button deleteButton;
        private Button openButton;
        private Button openNewButton;
        private PropertyField fieldGraph;
        private VisualElement root;

        public static StateMachineRunnerEditor Instance { get; private set; }
        public static Action OnListChanged;

        private bool _syncScheduled = false;

        // PAINT METHOD: --------------------------------------------------------------------------
        
        ~StateMachineRunnerEditor()
        {
            if(fieldRunnerList != null) fieldRunnerList.EventChangeSize -= OnChanged;
            GraphInspector.OnListChanged -= OnAssetVariableChange;
            BlackboardView.OnListChanged -= OnAssetVariableChange;
        }

        public override VisualElement CreateInspectorGUI()
        {
            Instance = this;
            runner = target as StateMachineRunner;

            root = new VisualElement
            {
                style =
                {
                    marginTop = new StyleLength(5)
                }
            };
            
            var sheets = StyleSheetUtils.Load(USS_PATH);
            foreach (var styleSheet in sheets) root.styleSheets.Add(styleSheet);

            var graph = serializedObject.FindProperty("stateMachineAsset");
            var runtimeProp = serializedObject.FindProperty("m_Runtime");
            var subRuntime = serializedObject.FindProperty("m_SubStatesRuntime");
            var saveUniqueID = serializedObject.FindProperty("m_SaveUniqueID");

            fieldGraph = new PropertyField(graph);

            fieldRunnerList = new RunnerNameListTool(runtimeProp.FindPropertyRelative("m_List"), "Variables");
            fieldSubList = new RunnerNameSubListTool(subRuntime.FindPropertyRelative("m_List"));
            propertyList = runtimeProp.FindPropertyRelative("m_List").FindPropertyRelative("m_Source");
            propertySubList = subRuntime.FindPropertyRelative("m_List").FindPropertyRelative("m_Source");
            
            _lastAsset = runner.stateMachineAsset;

            var fieldRuntime = new PropertyField(runtimeProp);
            var fieldSaveUniqueID = new PropertyField(saveUniqueID);
            m_Error = new ErrorMessage(ERR_DUPLICATE_ID)
            {
                style = {marginTop = ERROR_MARGIN}
            };
            
            m_Info = new InfoMessage(EMBED_INFO)
            {
                style = {marginTop = ERROR_MARGIN}
            };

            var head = new VisualElement
            {
                name = NAME_HEAD,
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.Center
                }
            };
            var left = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart
                }
            };
            var right = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd
                }
            };
            head.Add(left);
            head.Add(right);
            
            ICON_Open ??= new IconEdit(ColorTheme.Type.TextLight);
            ICON_OpenNew ??= new IconHotspot(ColorTheme.Type.TextLight);
            ICON_Embed ??= new IconCubeSolid(ColorTheme.Type.TextLight);
            ICON_Detach ??= new IconCubeOutline(ColorTheme.Type.TextLight);
            ICON_Delete ??= new IconCancel(ColorTheme.Type.TextLight);
            ICON_Edit ??= new IconBug(ColorTheme.Type.TextLight);
            
            openButton = new Button(() =>
            {
                var graphAsset = runner.stateMachineAsset;
                if (graphAsset == null) return;
                var window = EditorWindow.GetWindow<StateMachineGraphWindow>();
                window.InitializeGraph(graphAsset);
            })
            {
                tooltip = "Open State Machine",
            };
            openButton.AddToClassList(CLASS_HEAD_BUTTON);
            var image2 = new Image { image = ICON_Open.Texture };
            var label2 = new Label("Open");
            image2.AddToClassList("gc-save-image");
            label2.AddToClassList("gc-save-label");
            openButton.Add(image2);
            
            openNewButton = new Button(() =>
            {
                var graphAsset = runner.stateMachineAsset;
                if (graphAsset == null) return;
                var window = EditorWindow.CreateWindow<StateMachineGraphWindow>();
                window.InitializeGraph(graphAsset);
            })
            {
                tooltip = "Open in a new window",
            };
            openNewButton.AddToClassList(CLASS_HEAD_BUTTON);
            var image = new Image { image = ICON_OpenNew.Texture };
            var label = new Label("New Window");
            image.AddToClassList("gc-save-image");
            label.AddToClassList("gc-save-label");
            openNewButton.Add(image);
            
            embedButton = new Button(ToggleEmbed)
            {
                tooltip = runner.isEmbedded ? "Detach State Machine Asset" : "Embed State Machine Asset",
                style =
                {
                    width = new StyleLength(BUTTON_WIDTH)
                }
            };
            embedIcon = new Image { image = runner.isEmbedded ? ICON_Detach.Texture : ICON_Embed.Texture };
            embedIcon.AddToClassList("gc-save-image");
            embedLabel = new Label(runner.isEmbedded ? "Detach" : "Embed");
            embedLabel.AddToClassList("gc-save-label");
            embedButton.AddToClassList(CLASS_HEAD_BUTTON);
            embedButton.Add(embedIcon);
            embedButton.Add(embedLabel);

            deleteButton = new Button(() =>
            {
                if (!EditorUtility.DisplayDialog("Clear embedded State Machine", 
                        "This action is permanent.", "Yes",
                        "No"))
                {
                    return;
                }
                runner.stateMachineAsset = runner.originalAsset;
                runner.originalAsset = null;
                runner.cloneAsset = null;
                runner.isEmbedded = false;
                DestroyImmediate(runner.cloneAsset);
                UpdateVisualState();
                SetActiveStateMachine();
            })
            {
                tooltip = "Clear embedded State Machine data",
                style =
                {
                    width = new StyleLength(60f)
                }
            };

            var deleteIcon = new Image { image = ICON_Delete.Texture };
            deleteIcon.AddToClassList("gc-save-image");
            deleteButton.AddToClassList(CLASS_HEAD_BUTTON);
            deleteButton.Add(deleteIcon);
            var deleteLabel = new Label("Clear");
            deleteLabel.AddToClassList("gc-save-label");
            deleteButton.Add(deleteLabel);
            
            UpdateVisualState();

            left.Add(openButton);
            left.Add(openNewButton);
            
            right.Add(deleteButton);
            right.Add(embedButton);
            
            root.Add(fieldGraph);
            root.Add(new SpaceSmall());
            root.Add(head);
            root.Add(m_Info);
            root.Add(new SpaceSmaller());

            // Register property change callback once
            root.RegisterCallback<SerializedPropertyChangeEvent>(evt => OnChanged(0));
            
            // Call sync functions once on creation.
            SyncVariables(true);
            SyncSubVariables();

            switch (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                case true:
                    root.Add(fieldRuntime);
                    break;

                case false:
                    root.Add(fieldRunnerList);
                    root.Add(fieldSubList);
                    fieldRunnerList.EventChangeSize -= OnChanged;
                    fieldRunnerList.EventChangeSize += OnChanged;
                    GraphInspector.OnListChanged -= OnAssetVariableChange;
                    GraphInspector.OnListChanged += OnAssetVariableChange;
                    BlackboardView.OnListChanged -= OnAssetVariableChange;
                    BlackboardView.OnListChanged += OnAssetVariableChange;
                    break;
            }
            
            root.Add(m_Error);
            root.Add(fieldSaveUniqueID);

            RefreshErrorID();
            PrefabCheck();
            
            fieldGraph.RegisterValueChangeCallback(OnGraphChange);
            fieldSaveUniqueID.RegisterValueChangeCallback(_ => { RefreshErrorID(); });
            return root;

            void ToggleEmbed()
            {
                if(!runner.isEmbedded) EmbedAsset();
                else DetachAsset();
            }
        }

        private void PrefabCheck()
        {
            if(!target) return;
            
            // Simple embedded graph validation when inspector is displayed
            ValidateEmbeddedGraphInInspector();
        }
        
        /// <summary>
        /// Validates embedded graph and offers to fix if needed (only when inspector is shown)
        /// </summary>
        private void ValidateEmbeddedGraphInInspector()
        {
            if (!runner || !runner.isEmbedded) return;
            
            // Check if this runner needs fixing
            if (NeedsEmbeddedGraphFix())
            {
                var result = EditorUtility.DisplayDialog(
                    "Embedded Graph Issue Detected",
                    $"The StateMachine Runner '{runner.name}' has an embedded graph issue that can be automatically fixed.\n\n" +
                    $"Issue: {GetEmbeddedGraphIssueDescription()}\n\n" +
                    $"Would you like to fix this automatically?",
                    "Fix Now",
                    "Ignore"
                );
                
                if (result)
                {
                    FixEmbeddedGraphIssue();
                }
            }
        }
        
        /// <summary>
        /// Checks if the runner needs embedded graph fixing
        /// </summary>
        private bool NeedsEmbeddedGraphFix()
        {
            // Issue 1: Embedded but missing assets
            if (!runner.cloneAsset || !runner.stateMachineAsset)
            {
                return runner.originalAsset != null; // Can only fix if we have original
            }
            
            // Issue 2: Asset reference mismatch
            if (runner.cloneAsset != runner.stateMachineAsset)
            {
                return true;
            }
            
            // Issue 3: Clone asset not properly linked (for scene objects)
            if (!PrefabUtility.IsPartOfPrefabAsset(runner) && 
                runner.cloneAsset && !runner.cloneAsset.IsLinkedToScene())
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets a user-friendly description of the embedded graph issue
        /// </summary>
        private string GetEmbeddedGraphIssueDescription()
        {
            if (!runner.cloneAsset || !runner.stateMachineAsset)
            {
                return "Missing embedded graph assets (likely lost during prefab conversion)";
            }
            
            if (runner.cloneAsset != runner.stateMachineAsset)
            {
                return "Embedded graph asset references are mismatched";
            }
            
            if (!PrefabUtility.IsPartOfPrefabAsset(runner) && 
                runner.cloneAsset && !runner.cloneAsset.IsLinkedToScene())
            {
                return "Embedded graph not properly linked to scene";
            }
            
            return "Unknown embedded graph issue";
        }
        
        /// <summary>
        /// Fixes the embedded graph issue
        /// </summary>
        private void FixEmbeddedGraphIssue()
        {
            try
            {
                var wasFixed = PrefabUtility.IsPartOfPrefabAsset(runner) ? FixPrefabEmbeddedGraph() : FixSceneEmbeddedGraph();
                if (wasFixed)
                {
                    EditorUtility.SetDirty(runner);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Successfully fixed embedded graph for '{runner.name}'", runner);
                    
                    // Force inspector refresh to show updated state
                    Repaint();
                }
                else
                {
                    Debug.LogWarning($"Could not fix embedded graph for '{runner.name}' - please check manually", runner);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to fix embedded graph for '{runner.name}': {e.Message}", runner);
            }
        }
        
        /// <summary>
        /// Fixes embedded graph for prefab objects
        /// </summary>
        private bool FixPrefabEmbeddedGraph()
        {
            var prefabPath = AssetDatabase.GetAssetPath(runner);
            if (string.IsNullOrEmpty(prefabPath)) return false;
            
            // Issue 1: Missing clone/main assets - recreate from original
            if (runner.originalAsset && (!runner.cloneAsset || !runner.stateMachineAsset))
            {
                var clone = Instantiate(runner.originalAsset);
                clone.name = $"{runner.gameObject.name} State Machine (Embedded)";
                
                AssetDatabase.AddObjectToAsset(clone, prefabPath);
                
                runner.cloneAsset = clone;
                runner.stateMachineAsset = clone;
                
                EditorUtility.SetDirty(clone);
                return true;
            }
            
            // Issue 2: Asset reference mismatch
            if (runner.cloneAsset && runner.stateMachineAsset != runner.cloneAsset)
            {
                runner.stateMachineAsset = runner.cloneAsset;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Fixes embedded graph for scene objects
        /// </summary>
        private bool FixSceneEmbeddedGraph()
        {
            // Issue 1: Missing clone/main assets - recreate from original
            if (runner.originalAsset && (!runner.cloneAsset || !runner.stateMachineAsset))
            {
                var clone = Instantiate(runner.originalAsset);
                clone.name = $"{runner.gameObject.name} State Machine (Embedded)";
                clone.LinkToScene(runner.gameObject.scene);
                
                runner.cloneAsset = clone;
                runner.stateMachineAsset = clone;
                
                EditorUtility.SetDirty(clone);
                return true;
            }
            
            // Issue 2: Asset reference mismatch
            if (runner.cloneAsset && runner.stateMachineAsset != runner.cloneAsset)
            {
                runner.stateMachineAsset = runner.cloneAsset;
                return true;
            }
            
            // Issue 3: Clone asset not linked to scene
            if (runner.cloneAsset && !runner.cloneAsset.IsLinkedToScene())
            {
                runner.cloneAsset.LinkToScene(runner.gameObject.scene);
                EditorUtility.SetDirty(runner.cloneAsset);
                return true;
            }
            
            return false;
        }

        private void SetActiveStateMachine()
        {
            if(!runner.stateMachineAsset) return;
            StateMachineAsset.Active = runner.stateMachineAsset;
            var window = EditorWindow.GetWindow<StateMachineGraphWindow>();
            window.InitializeGraph(runner.stateMachineAsset);
        }

        private void OpenGraph()
        {
            root.schedule.Execute(SetActiveStateMachine).ExecuteLater(10);
        }

        private void UpdateVisualState()
        {
            // Auto-fix embedded state if clone asset is missing
            if (runner.isEmbedded && !runner.cloneAsset) 
            {
                Debug.LogWarning($"Embedded runner '{runner.name}' missing clone asset - auto-fixing", runner);
                runner.isEmbedded = false;
                EditorUtility.SetDirty(runner);
            }
            
            // Validate embedded graph integrity
            if (runner.isEmbedded && runner.cloneAsset && !runner.cloneAsset.IsLinkedToScene())
            {
                // Debug.Log($"Re-linking embedded graph to scene for '{runner.name}'", runner);
                runner.cloneAsset.LinkToScene(runner.gameObject.scene);
                EditorUtility.SetDirty(runner.cloneAsset);
            }
            
            // Update UI state
            m_Info.style.display = runner.isEmbedded ? DisplayStyle.Flex : DisplayStyle.None;
            fieldGraph.SetEnabled(!runner.isEmbedded);
            embedButton.tooltip = runner.isEmbedded ? "Detach State Machine Asset" : "Embed State Machine Asset";
            embedIcon.image = runner.isEmbedded ? ICON_Detach.Texture : ICON_Embed.Texture;
            embedLabel.text = runner.isEmbedded ? "Detach" : "Embed";
            
            // Enable embed button for prefabs (now supported with sub-asset storage)
            var isPrefab = target != null && PrefabUtility.IsPartOfPrefabAsset(target);
            embedButton.SetEnabled(true); // Always enabled now
            
            // Update button states
            deleteButton.SetEnabled(runner.cloneAsset != null);
            openButton.SetEnabled(runner.stateMachineAsset != null);
            openNewButton.SetEnabled(runner.stateMachineAsset != null);
            
            // Show info for embedded graphs in prefabs
            if (isPrefab && runner.isEmbedded)
            {
                Debug.Log($"Embedded graph in prefab '{runner.name}' uses sub-asset storage for better compatibility.", runner);
            }
        }

        private StateMachineAsset GetCopy()
        {
            StateMachineAsset newInstance;
            if (!runner.stateMachineAsset)
            {
                newInstance = CreateInstance<StateMachineAsset>();
                newInstance.name = $"{runner.gameObject.name} State Machine";
            }
            else
            {
                newInstance = Instantiate(runner.stateMachineAsset);
            }
            return newInstance;
        }
        
        private void EmbedAsset()
        {
            // Store original asset reference
            runner.originalAsset = runner.stateMachineAsset;
            
            // Create or reuse clone
            var newInstance = runner.cloneAsset ?? GetCopy();
            newInstance.name = $"{runner.gameObject.name} State Machine (Embedded)";
            
            // Set up embedded references
            runner.stateMachineAsset = newInstance;
            runner.cloneAsset = newInstance;
            runner.isEmbedded = true;
            
            // Handle prefab vs scene linking
            var isPrefab = PrefabUtility.IsPartOfPrefabAsset(target);
            if (isPrefab)
            {
                // For prefabs, store as sub-asset
                var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(runner);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    try
                    {
                        AssetDatabase.AddObjectToAsset(newInstance, prefabPath);
                        Debug.Log($"Stored embedded graph as prefab sub-asset for '{runner.name}'", runner);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to create prefab sub-asset: {e.Message}", runner);
                    }
                }
            }
            else
            {
                // For scene objects, link to scene
                runner.stateMachineAsset.LinkToScene(runner.gameObject.scene);
            }
            
            // Ensure variables are synced
            if (runner.originalAsset?.NameList != null)
            {
                // Copy variables from original to embedded
                StateMachineAsset.SyncVariables(runner.originalAsset.NameList, runner.stateMachineAsset.NameList);
                SyncVariables(false); // Sync runner variables without removing
            }
            
            // Mark objects as dirty
            EditorUtility.SetDirty(runner);
            EditorUtility.SetDirty(runner.stateMachineAsset);
            
            UpdateVisualState();
            OpenGraph();
        }
        
        private void DetachAsset()
        {
            // Validate we have the necessary references
            if (!runner.originalAsset)
            {
                Debug.LogWarning("Cannot detach: Original asset reference is missing", runner);
                return;
            }
            
            // Store the embedded clone for potential future use
            runner.cloneAsset = runner.stateMachineAsset;
            
            // Sync variables back to original before detaching
            if (runner.stateMachineAsset?.NameList != null && runner.originalAsset?.NameList != null)
            {
                StateMachineAsset.SyncVariables(runner.stateMachineAsset.NameList, runner.originalAsset.NameList);
                EditorUtility.SetDirty(runner.originalAsset);
            }
            
            // Restore original asset
            runner.stateMachineAsset = runner.originalAsset;
            runner.isEmbedded = false;
            
            // Sync runner variables with the restored asset
            SyncVariables(false);
            
            // Mark as dirty
            EditorUtility.SetDirty(runner);
            
            UpdateVisualState();
            OpenGraph();
        }

        private void OnDisable()
        {
            Instance = null;
            GraphInspector.OnListChanged -= OnAssetVariableChange;
            BlackboardView.OnListChanged -= OnAssetVariableChange;
            if(fieldRunnerList != null) fieldRunnerList.EventChangeSize -= OnChanged;
        }

        private void OnAssetVariableChange()
        {
            if(fieldRunnerList == null) return;
            SyncVariables(true);
            SyncSubVariables();
        }

        // Schedule the sync callback once per frame to avoid multiple syncs
        private void OnChanged(int size)
        {
            if (!_syncScheduled)
            {
                _syncScheduled = true;
                EditorApplication.delayCall += SyncStateMachineVariables;
            }
            OnListChanged?.Invoke();
        }

        private void SyncStateMachineVariables()
        {
            EditorApplication.delayCall -= SyncStateMachineVariables;
            _syncScheduled = false;
            if(!runner.stateMachineAsset) return;
            
            // Always sync from runner to asset to ensure variables aren't lost
            runner.stateMachineAsset.SyncVariablesInternal(runner.Runtime.List);
            
            // Mark the asset as dirty to ensure changes are saved
            EditorUtility.SetDirty(runner.stateMachineAsset);
        }

        private void OnGraphChange(SerializedPropertyChangeEvent evt)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            
            if (runner.stateMachineAsset == _lastAsset) return;

            serializedObject.Update();
            fieldRunnerList.PropertyList.ClearArray();
            fieldRunnerList.Clear();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            if (runner.stateMachineAsset != _lastAsset)
            {
                _lastAsset = runner.stateMachineAsset;
                SyncVariables();
                SyncSubVariables();
            }

            fieldRunnerList.EventChangeSize -= OnChanged;
            fieldRunnerList.EventChangeSize += OnChanged;
        }

        public void SyncVariables(bool remove = false)
        {
            // Early exit conditions
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (fieldRunnerList == null) return;
            if (runner.stateMachineAsset == null || runner.stateMachineAsset.NameList == null) return;
            
            // Refresh our runner reference and cache the current asset reference
            runner = target as StateMachineRunner;
            _lastAsset = runner.stateMachineAsset;
            
            serializedObject.Update();
            
            var changed = false;
            
            // Cache the arrays of names
            var assetNames = runner.stateMachineAsset.NameList.Names;
            var runtimeNames = runner.Runtime.List.Names;
            
            // Precompute hash set for fast lookup in runtime list
            var runtimeNamesSet = new HashSet<string>(runtimeNames);
            
            // 1. Synchronize: for each asset variable...
            for (var i = 0; i < assetNames.Length; i++)
            {
                var assetName = assetNames[i];
                
                // If the runtime list already has this variable...
                if (runtimeNamesSet.Contains(assetName))
                {
                    // Find the runner index of this variable.
                    var runnerIndex = Array.IndexOf(runtimeNames, assetName);
                    if (runnerIndex < 0) continue;
                    var assetVar = runner.stateMachineAsset.NameList.Get(i);
                    var runnerVar = runner.Runtime.List.Get(runnerIndex);
                    // If types differ, remove the runner variable.
                    if (assetVar.Type == runnerVar.Type) continue;
                    fieldRunnerList?.DeleteItem(runnerIndex);
                    changed = true;
                }
                else // The asset variable is missing in runtime: insert it.
                {
                    var value = runner.stateMachineAsset.NameList.Get(i);
                    fieldRunnerList?.InsertItem(propertyList.arraySize, value.Copy);
                    changed = true;
                }
            }
            
            // 2. (Optional) Remove variables from runtime that no longer exist in the asset.
            if (remove)
            {
                // Precompute a hash set for asset names.
                var assetNamesSet = new HashSet<string>(assetNames);
                // Iterate backwards to safely remove items.
                for (var i = runtimeNames.Length - 1; i >= 0; i--)
                {
                    var runnerName = runtimeNames[i];
                    if (assetNamesSet.Contains(runnerName)) continue;
                    fieldRunnerList?.DeleteItem(i);
                    changed = true;
                }
            }
            
            
            // If any changes occurred, refresh the UI and mark the asset dirty.
            if (!changed) return;
            fieldRunnerList?.Refresh();
            serializedObject.Update();
            EditorUtility.SetDirty(runner);
        }

        private void SyncSubVariables()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(fieldRunnerList == null) return;
            if(runner.stateMachineAsset == null) return;
            if(runner.stateMachineAsset.NameList == null) return;
            
            var changed = false;
            var subgraphs = runner.stateMachineAsset.nodes.OfType<StateMachineNode>();
            
            // Add variables from sub state machines
            foreach (var subgraph in subgraphs)
            {
                if(subgraph.stateMachine == null) continue;
                var subgraphVariables = subgraph.stateMachine.NameList.Names;
                foreach (var subgraphVariable in subgraphVariables)
                {
                    var index = subgraph.stateMachine.NameList.Names.ToList().IndexOf(subgraphVariable);
                    var value = subgraph.stateMachine.NameList.Get(index);
                    
                    var nameVar = subgraph.stateMachine.NameList.Names[index];
                    var exists = runner.SubRuntime.List.Names.Any(v => v == nameVar);
                    if(exists)
                    {
                        var runnerIndex = runner.SubRuntime.List.Names.ToList().IndexOf(nameVar);
                        if (subgraph.stateMachine.NameList.Get(index).Type != runner.SubRuntime.List.Get(runnerIndex).Type)
                        {
                            fieldSubList?.DeleteItem(runnerIndex);
                            changed = true;
                        }
                        continue;
                    }
                    
                    fieldSubList?.InsertItem(propertySubList.arraySize, value.Copy);
                    changed = true;
                }
            }
            
            // Remove duplicates
            var duplicates = runner.SubRuntime.List.Names
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            foreach (var duplicate in duplicates)
            {
                var indexes = runner.SubRuntime.List.Names.Select((name, index) => new {name, index})
                    .Where(x => x.name == duplicate)
                    .Select(x => x.index)
                    .ToList();
                for (var i = 1; i < indexes.Count; i++)
                {
                    fieldSubList?.DeleteItem(indexes[i]);
                    changed = true;
                }
            }
            
            // Remove variables that doesn't exist in the sub State Machines
            var removeList = runner.SubRuntime.List.Names.Where(v => !subgraphs.Any(s => s.stateMachine.NameList.Names.Contains(v)));
            for (var i = 0; i < runner.SubRuntime.List.Names.Length; i++)
            {
                var nameVar = runner.SubRuntime.List.Names[i];
                if(removeList.Contains(nameVar))
                {
                    fieldSubList?.DeleteItem(i);
                    changed = true;
                }
            }
            
            if(!changed) return;
            
            fieldSubList?.Refresh();
            serializedObject.Update();
        }

        private void RefreshErrorID()
        {
            if(!target) return;
            serializedObject.Update();
            m_Error.style.display = DisplayStyle.None;

            if (PrefabUtility.IsPartOfPrefabAsset(target)) return;

            var saveUniqueID = serializedObject.FindProperty("m_SaveUniqueID");

            var itemID = saveUniqueID
                .FindPropertyRelative(SaveUniqueIDDrawer.PROP_UNIQUE_ID)
                .FindPropertyRelative(UniqueIDDrawer.SERIALIZED_ID)
                .FindPropertyRelative(IdStringDrawer.NAME_STRING)
                .stringValue;

            var variables = FindObjectsByType<TLocalVariables>(FindObjectsSortMode.None);
            foreach (var variable in variables)
            {
                if (variable.SaveID != itemID || variable == target) continue;
                m_Error.style.display = DisplayStyle.Flex;

                return;
            }
        }
        
        [MenuItem("GameObject/Game Creator/State Machine/Runner", false, 0)]
        public static void CreateElement(MenuCommand menuCommand)
        {
            var instance = new GameObject("StateMachineRunner");
            instance.AddComponent<StateMachineRunner>();
            
            GameObjectUtility.SetParentAndAlign(instance, menuCommand?.context as GameObject);

            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }
    }
}