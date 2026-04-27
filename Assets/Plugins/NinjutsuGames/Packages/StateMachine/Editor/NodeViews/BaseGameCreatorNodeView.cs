using System;
using System.Linq;
using System.Reflection;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using NinjutsuGames.StateMachine.Runtime;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEditor.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    public class BaseGameCreatorNodeView : BaseNodeView
    {
        protected static readonly IIcon ICON_INSTRUCTION = new IconInstructions(ColorTheme.Type.Blue);
        protected static readonly IIcon ICON_CONDITION = new IconConditions(ColorTheme.Type.Green);
        protected static readonly IIcon ICON_BRANCH = new IconBranch(ColorTheme.Type.Green);
        protected static readonly IIcon ICON_STATE_MACHINE = new IconStateMachine(ColorTheme.Type.Blue);
        protected static readonly IIcon ICON_EVENT = new IconTriggers(ColorTheme.Type.Yellow);
        protected static readonly IIcon ICON_NONE = new IconMarker(ColorTheme.Type.White);
        protected static readonly IIcon ICON_ARROW = new IconArrowRight(ColorTheme.Type.White);
        protected static readonly IIcon ICON_COLLAPSED = new IconChevronLeft(ColorTheme.Type.White);
        protected static readonly IIcon ICON_EXPANDED = new IconChevronDown(ColorTheme.Type.White);

        protected readonly Image icon;

        public virtual Texture2D DefaultIcon => ICON_NONE.Texture;
        
        public virtual string DefaultIconName => null;
        
        private SerializedProperty NodeProperty
        {
            get
            {
                return nodeProperty ?? nodeTarget switch
                {
                    StartNode or ExitNode or ActionsNode => FindSerializedProperty("instructions"),
                    ConditionsNode => FindSerializedProperty("conditions"),
                    BranchNode => FindSerializedProperty("branch"),
                    TriggerNode => FindSerializedProperty("triggerEvent"),
                    StateMachineNode => FindSerializedProperty("stateMachine"),
                    _ => nodeProperty
                };
            }
        }

        protected Label counter;
        protected internal bool updateControls;
        private bool updateInspector;
        private bool isMouseOverControlsContainer;
        private string lastChange;
        private Image expandIcon;
        private SerializedProperty nodeProperty;
        
        public BaseGameCreatorNodeView() : base()
        {
            icon = new Image();
        }

        public void SetIcon(Texture iconTexture)
        {
            icon.image = iconTexture;
        }

        protected Texture2D GetIcon(SerializedProperty property)
        {
            var fieldType = TypeUtils.GetTypeFromProperty(property, true);
            var iconAttribute = fieldType?
                .GetCustomAttributes<ImageAttribute>()
                .FirstOrDefault();

            return iconAttribute != null ? iconAttribute.Image : Texture2D.whiteTexture;
        }

        public override void Enable()
        {
            updateControls = true;
            UpdateExecutionStateView();

            if (this.nodeTarget == null)
            {
                Debug.LogWarning("BaseGameCreatorNodeView: nodeTarget is null in Enable()");
                return;
            }

            var node = (BaseGameCreatorNode) this.nodeTarget;
            if (node == null)
            {
                Debug.LogWarning("BaseGameCreatorNodeView: Failed to cast nodeTarget to BaseGameCreatorNode");
                return;
            }

            CleanupEventHandlers(node);
                
            node.EventStartRunning += StartRunning;
            node.EventStopRunning += StopRunning;
            
            owner.graph.onGraphChanges += UpdateVisuals;
            SetIcon(DefaultIcon);
            icon.AddToClassList("node-icon");
            titleContainer.Insert(0, icon);
            UpdateIcon();
            UpdateName();
            InjectCustomStyle();

            if (node.hideControls) return;

            if (owner.graph.nodesExpanded) node.showControls = true;
            
            expandIcon = new Image
            {
                image = ICON_COLLAPSED.Texture,
            };
            expandIcon.AddToClassList("expand-icon");
            var expandButton = new Button(() =>
            {
                node.showControls = !node.showControls;
                expandIcon.image = node.showControls ? ICON_EXPANDED.Texture : ICON_COLLAPSED.Texture;
                controlsContainer.style.display = node.showControls ? DisplayStyle.Flex : DisplayStyle.None;
                if(node.showControls)
                {
                    updateControls = true;
                    AttemptDrawInspector();
                    owner.ClearSelection();
                    owner.AddToSelection(this);
                }
                else if(owner.selection.Contains(this))
                {
                    owner.RemoveFromSelection(this);
                }
            });
            expandButton.Add(expandIcon);
            expandButton.AddToClassList("expand-button");
            titleButtonContainer.Add(expandButton);

            expandIcon.image = node.showControls ? ICON_EXPANDED.Texture : ICON_COLLAPSED.Texture;
            controlsContainer.style.display = node.showControls ? DisplayStyle.Flex : DisplayStyle.None;
            if(node.showControls) AttemptDrawInspector();
            
            controlsContainer.RegisterCallback<MouseEnterEvent>(evt => isMouseOverControlsContainer = true);
            controlsContainer.RegisterCallback<MouseLeaveEvent>(evt => isMouseOverControlsContainer = false);
            
            controlsContainer.RegisterCallback<WheelEvent>(e => { if(node.showControls) e.StopPropagation(); });
            controlsContainer.RegisterCallback<MouseDownEvent>(e => { if(node.showControls) e.StopPropagation(); });
            
            controlsContainer.RegisterCallback<ContextualMenuPopulateEvent>(e => { if(node.showControls) e.StopPropagation(); });
        }
        
        private void CleanupEventHandlers(BaseGameCreatorNode node)
        {
            if (node == null) return;

            node.EventStartRunning -= StartRunning;
            node.EventStopRunning -= StopRunning;
        }


        protected internal void AttemptDrawInspector()
        {
            if (!updateControls) return;
            controlsContainer.Clear();
            DrawDefaultInspector();
            updateControls = false;
        }

        private void AttemptUpdateInspector()
        {
            if(!updateInspector) return;
            owner.RefreshNodeInspector(); 
            updateInspector = false;
        }

        private void UpdateVisuals(GraphChanges changes)
        {
            if (changes.nodeChanged == null || !changes.nodeChanged.Equals(nodeTarget)) return;
    
            bool needsControlsUpdate = false;
            bool needsInspectorUpdate = false;
    
            var node = (BaseGameCreatorNode)nodeTarget;
            bool controlsVisible = controlsContainer.style.display == DisplayStyle.Flex && node.showControls;
    
            if (controlsVisible)
            {
                needsControlsUpdate = changes.fromInspector && !isMouseOverControlsContainer;
                needsInspectorUpdate = !changes.fromInspector && isMouseOverControlsContainer;
            }
            else if (changes.fromInspector && !isMouseOverControlsContainer)
            {
                needsControlsUpdate = true;
            }
    
            if (needsControlsUpdate)
            {
                updateControls = true;
                AttemptDrawInspector();
            }
    
            if (needsInspectorUpdate)
            {
                updateInspector = true;
                AttemptUpdateInspector();
            }
    
            if (nodeTarget is not StartNode and not ExitNode)
            {
                UpdateIcon();
                UpdateName();
            }
    
            Update();

        }

        public override void Disable()
        {
            base.Disable();
            
            var node = (BaseGameCreatorNode) nodeTarget;
            CleanupEventHandlers(node);
            
            controlsContainer.UnregisterCallback<MouseEnterEvent>(evt => isMouseOverControlsContainer = true);
            controlsContainer.UnregisterCallback<MouseLeaveEvent>(evt => isMouseOverControlsContainer = false);
        }

        public void Reset()
        {
            var node = (BaseGameCreatorNode) nodeTarget;
            node.Reset();
            CleanupEventHandlers(node);
                
            node.EventStartRunning += StartRunning;
            node.EventStopRunning += StopRunning;
        }

        private void StartRunning(GameObject context)
        {
            owner.eventQueue.AddEvent(() => Highlight(context));
        }

        private void StopRunning(GameObject context, bool runningResult)
        {
            owner.eventQueue.AddEvent(() => UnHighlight(context, runningResult));
        }

        protected void InjectCustomStyle()
        {
            var border = this.Q("node-border");
            var overflowStyle = border.style.overflow;
            overflowStyle.value = Overflow.Visible;
            border.style.overflow = overflowStyle;

            // var selectionBorder = this.Q("selection-border");
            // selectionBorder.SendToBack();
        }

        protected Texture2D GetIcon(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return DefaultIcon;

            try {
                var fieldType = TypeUtils.GetTypeFromName(typeName);
                if (fieldType == null) return DefaultIcon;

                var attributes = fieldType.GetCustomAttributes<ImageAttribute>();

                var attribute = attributes.FirstOrDefault();
                return attribute?.Image ?? DefaultIcon;
            } catch (Exception ex) {
                Debug.LogWarning($"Error getting icon for {typeName}: {ex.Message}");
                return DefaultIcon;
            }
        }
        
        private string GetName(string typeName)
        {
            if(string.IsNullOrEmpty(typeName)) return null;
            var fieldType = TypeUtils.GetTypeFromName(typeName);
            return fieldType?.GetCustomAttributes<TitleAttribute>().FirstOrDefault()?.Title;
        }
        
        public void UpdateIcon()
        {
            if(nodeTarget == null) return;
            if(expandIcon != null) expandIcon.image = nodeTarget.showControls ? ICON_EXPANDED.Texture : ICON_COLLAPSED.Texture;

            string iconName = null;
            try {
                iconName = DefaultIconName;
            } catch (Exception ex) {
                Debug.LogWarning($"Error getting DefaultIconName: {ex.Message}");
            }

            SetIcon(GetIcon(iconName));
        }
        
        protected virtual void UpdateName()
        {
            if(nodeTarget is ExitNode or StartNode) return;
            if(!string.IsNullOrEmpty(nodeTarget.nodeCustomName)) return;
            var newName = GetName(DefaultIconName);
            switch (nodeTarget)
            {
                case StateMachineNode node:
                    newName = node.stateMachine == null ? "State Machine" : node.stateMachine.name;
                    break;
                case ActionsNode:
                    if(string.IsNullOrEmpty(newName)) newName = "Actions";
                    break;
                case ConditionsNode:
                    if(string.IsNullOrEmpty(newName)) newName = "Conditions";
                    break;
                case BranchNode:
                    if(string.IsNullOrEmpty(newName)) newName = "Branch";
                    break;
                case TriggerNode:
                    if(string.IsNullOrEmpty(newName)) newName = "Trigger";
                    break;
            }

            if(string.IsNullOrEmpty(newName)) return;
            title = newName;
        }

        public virtual void Update()
        {
            
        }

        protected override void DrawDefaultInspector(bool fromInspector = false)
        {
            var element = nodeTarget is StateMachineNode ? new PropertyField(NodeProperty, fromInspector ? "State Machine" : string.Empty) : new PropertyField(NodeProperty);
            if (element.Equals(null)) return;

            if (nodeTarget is ConditionsNode)
            {
                AddControlField("checkMode", "Check Mode", false);
                controlsContainer.Add(new SpaceSmaller());
            }
            
            element.Unbind();
            element.RegisterValueChangeCallback(e => { NotifyNodeChanged(fromInspector); });
            element.Bind(owner.serializedGraph);
            controlsContainer.Add(element);
            
            if(nodeTarget is BaseGameCreatorNode { useNetwork: true })
            {
                AddControlField("networkingSettings", "Networking Settings", false);
            }
        }
        
        protected void AddCounter(int count)
        {
            counter = new Label();
            counter.AddToClassList("counter");
            counter.text = count.ToString();
            titleContainer.Add(counter);
        }
        
        protected void UpdateCounter(int count)
        {
            counter.text = count.ToString();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // Add play option for BaseGameCreatorNode
            if (nodeTarget is BaseGameCreatorNode gameCreatorNode)
            {
                evt.menu.AppendAction("Run Node", e => RunNode(gameCreatorNode), RunNodeStatus);
                evt.menu.AppendSeparator();
            }
            
            base.BuildContextualMenu(evt);
        }

        private DropdownMenuAction.Status RunNodeStatus(DropdownMenuAction action)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                return DropdownMenuAction.Status.Disabled;
            
            if (nodeTarget is not BaseGameCreatorNode)
                return DropdownMenuAction.Status.Disabled;
                
            return DropdownMenuAction.Status.Normal;
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
                GameObject context = node.Context;
                if (context == null)
                {
                    // Try to find a StateMachineRunner in the scene
                    var runner = UnityEngine.Object.FindAnyObjectByType<StateMachineRunner>();
                    if (runner != null)
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
}