using System.Collections.Generic;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    
    public class StateMachineListTool : TPolymorphicListTool
    {
        private const string NAME_BUTTON_ADD = "GC-Handles-Foot-Add";
        
        private static readonly IIcon ICON_ADD = new IconString(ColorTheme.Type.TextLight);

        // MEMBERS: -------------------------------------------------------------------------------

        private Button m_ButtonAdd;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string ElementNameHead => "GC-Handles-Head";
        protected override string ElementNameBody => "GC-Handles-Body";
        protected override string ElementNameFoot => "GC-Handles-Foot";
        
        protected override List<string> CustomStyleSheetPaths => new List<string>
        {
            EditorPaths.CHARACTERS + "StyleSheets/Handles"
        };

        public override bool AllowReordering => true;
        public override bool AllowDuplicating => true;
        public override bool AllowDeleting  => true;
        public override bool AllowContextMenu => true;
        public override bool AllowInsertion => false;
        public override bool AllowCopyPaste => true;
        public override bool AllowBreakpoint => false;
        public override bool AllowDisable => true;
        public override bool AllowDocumentation => false;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public StateMachineListTool(SerializedProperty property)
            : base(property, "m_List")
        {
            SerializedObject.Update();
            
            EditorApplication.playModeStateChanged += this.OnChangePlayMode;

            this.OnChangePlayMode(EditorApplication.isPlaying
                ? PlayModeStateChange.EnteredPlayMode
                : PlayModeStateChange.ExitingPlayMode
            );
        }
        
        ~StateMachineListTool()
        {
            EditorApplication.playModeStateChanged -= this.OnChangePlayMode;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void OnChangePlayMode(PlayModeStateChange state)
        { }
        
        // override 
        
        protected override VisualElement MakeItemTool(int index)
        {
            return new StateMachineItemTool(this, index);
        }

        protected override void SetupHead()
        { }

        protected override void SetupFoot()
        {
            base.SetupFoot();

            m_ButtonAdd = new Button(() =>
            {
                var insertIndex = PropertyList.arraySize;
                InsertItem(insertIndex, new StateMachineItem());
            })
            {
                name = NAME_BUTTON_ADD
            };

            m_ButtonAdd.Add(new Image { image = ICON_ADD.Texture });
            m_ButtonAdd.Add(new Label { text = "Add Asset..." });

            m_Foot.Add(m_ButtonAdd);
        }
    }
}