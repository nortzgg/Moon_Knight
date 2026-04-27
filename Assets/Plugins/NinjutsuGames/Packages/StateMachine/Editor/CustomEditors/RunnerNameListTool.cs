using System.Linq;
using System.Reflection;
using GameCreator.Editor.Common;
using GameCreator.Editor.Variables;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    public class RunnerNameVariableTool : TPolymorphicItemTool
    {
        private const string PROP_NAME = "m_Name";
        private const string PROP_VALUE = "m_Value";
        
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override string Title => Variable?.Title;

        private TVariable Variable => (ParentTool as RunnerNameListTool)?.NameList.Get(Index);
        
        protected override object Value => m_Property.GetValue<NameVariable>();

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public RunnerNameVariableTool(IPolymorphicListTool parentTool)
            : base(parentTool, 0)
        { }

        // OVERRIDERS: ----------------------------------------------------------------------------

        protected override void SetupBody()
        {
            m_Property.serializedObject.Update();

            var field = new PropertyField(m_Property);
            field.Bind(m_Property.serializedObject);
            
            field.RegisterValueChangeCallback(_ =>
            {
                m_Property.serializedObject.Update();
                UpdateHead();
            });

            m_Body.Add(field);
            UpdateBody(false);
        }

        protected override Texture2D GetIcon()
        {
            m_Property.serializedObject.Update();
            
            if(Variable == null) return Texture2D.whiteTexture;
            
            var instance = Variable.GetType()
                .GetField(PROP_VALUE, BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(Variable) as TValue;

            var iconAttrs = instance?.GetType()
                .GetCustomAttributes<ImageAttribute>()
                .FirstOrDefault();
            
            var icon = iconAttrs?.Image;
            return icon != null ? icon : Texture2D.whiteTexture;
        }

        protected override void OnChangePlayMode(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    m_Body?.SetEnabled(false);
                    break;
                
                default:
                    m_Body?.SetEnabled(true);
                    break;
            }
        }
    }
    
    public class RunnerNameListTool : NameListTool
    {
        protected const  string TITLE = "Variables";
        
        private const string NAME_ROOT_DATA = "GC-Runner-Data-Root";
        private const string NAME_HEAD_DATA = "GC-Runner-Data-Head";
        private const string NAME_BODY_DATA = "GC-Runner-Data-Body";

        private static readonly IIcon ICON_1 = new IconChevronDown(ColorTheme.Type.TextLight);
        private static readonly IIcon ICON_2 = new IconChevronRight(ColorTheme.Type.TextLight);
        
        protected readonly string USS_PATH = "GraphProcessorStyles/Runner";

        public override bool AllowReordering => true;
        public override bool AllowDuplicating => false;
        public override bool AllowDeleting  => true;
        public override bool AllowContextMenu => false;
        public override bool AllowCopyPaste => true;
        public override bool AllowInsertion => true;
        public override bool AllowBreakpoint => false;
        public override bool AllowDisable => false;
        public override bool AllowDocumentation => false;

        public RunnerNameListTool(SerializedProperty propertyRoot, string title, bool showEmptyMessage = false) : base(propertyRoot)
        {
            var baseStyle = Resources.Load<StyleSheet>(USS_PATH);
            if(baseStyle) styleSheets.Add(baseStyle);
            
            if(!string.IsNullOrEmpty(title)) SetupToggleHead(propertyRoot, title, showEmptyMessage);
        }

        private void SetupToggleHead(SerializedProperty propertyRoot, string title, bool showEmptyMessage)
        {
            var root = new VisualElement { name = NAME_ROOT_DATA };
            var head = new VisualElement { name = NAME_HEAD_DATA };
            var body = new VisualElement { name = NAME_BODY_DATA };
            
            root.Add(head);
            root.Add(body);
            
            var headIcon = new Image { pickingMode = PickingMode.Ignore };
            var headLabel = new Label(title) { pickingMode = PickingMode.Ignore };
            
            head.Add(headIcon);
            head.Add(headLabel);

            VisualElement emptyMessage = null;
            
            if(showEmptyMessage)
            {
                emptyMessage = new InfoMessage("This State Machine doesn't have any sub-state variables");
                body.Add(emptyMessage);
            }
            
            body.Add(m_Head);
            body.Add(m_Body);
            body.Add(m_Foot);
            
            head.RegisterCallback<ClickEvent>(_ => Toggle(propertyRoot, headIcon, body, emptyMessage));
            hierarchy.Add(root);
            
            RefreshHeader(propertyRoot, headIcon, body);
        }

        private static void Toggle(SerializedProperty property, Image headIcon, VisualElement body, VisualElement emptyMessage = null)
        {
            property.serializedObject.Update();
            property.isExpanded = !property.isExpanded;
            
            RefreshHeader(property, headIcon, body, emptyMessage);
        }
        
        private static void RefreshHeader(SerializedProperty property, Image headIcon, VisualElement body, VisualElement emptyMessage = null)
        {
            headIcon.image = property.isExpanded
                ? ICON_1.Texture
                : ICON_2.Texture;
            
            body.style.display = property.isExpanded
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            if (emptyMessage == null) return;
            var list = property.managedReferenceValue as NameList;
            emptyMessage.style.display = list?.Names.Length == 0
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
    
    public class RunnerNameSubListTool : RunnerNameListTool
    {
        private const string Title = "Sub-State Variables";

        public override bool AllowReordering => true;
        public override bool AllowDuplicating => false;
        public override bool AllowDeleting  => false;
        public override bool AllowContextMenu => false;
        public override bool AllowCopyPaste => true;
        public override bool AllowInsertion => false;
        public override bool AllowBreakpoint => false;
        public override bool AllowDisable => false;
        public override bool AllowDocumentation => false;

        public RunnerNameSubListTool(SerializedProperty propertyRoot) : base(propertyRoot, Title, true) {}

        protected override void SetupHead()
        {
            m_Head.style.display = DisplayStyle.None;
        }
    }

    public class BlackboardNameListTool : RunnerNameListTool
    {
        public override bool AllowReordering => true;
        public BlackboardNameListTool(SerializedProperty propertyRoot) : base(propertyRoot, string.Empty) {}
    }
}