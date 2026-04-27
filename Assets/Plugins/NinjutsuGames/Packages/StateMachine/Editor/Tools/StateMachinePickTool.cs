using System;
using System.Collections.Generic;
using System.Reflection;
using GameCreator.Editor.Common;
using GameCreator.Editor.Variables;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NinjutsuGames.StateMachine.Editor
{
    public class StateMachinePickTool : TNamePickTool
    {
        private TextField m_NameField;
        private VisualElement m_NameDropdown;
        private bool m_IgnoreFilter;
        private string m_FieldName = " ";

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public StateMachinePickTool(SerializedProperty property)
            : base(property, true, false, ValueNull.TYPE_ID)
        { }

        public StateMachinePickTool(SerializedProperty property, bool ignoreFilter, string fieldName)
            : base(property, true, false, ValueNull.TYPE_ID)
        {
            m_IgnoreFilter = ignoreFilter;
            m_FieldName = fieldName;
        }
        
        public StateMachinePickTool(SerializedProperty property, IdString typeID, bool allowCast)
            : base(property, false, allowCast, typeID)
        { }

        protected override Object Asset 
        {
            get
            {
                try
                {
                    // Check if the property is still valid before accessing it
                    if (m_PropertyVariable == null || m_PropertyVariable.serializedObject == null)
                        return null;
                    
                    if (m_PropertyVariable.serializedObject.targetObject == null)
                        return null;
                        
                    return m_PropertyVariable.objectReferenceValue;
                }
                catch (System.Exception)
                {
                    // SerializedProperty has been disposed or is no longer valid
                    return null;
                }
            }
        }
        public event Action<string, Object> EventChangeSelection;

        public void RefreshPickList()
        {
            RefreshPickList(Asset);
        }

        protected override void RefreshPickList(Object asset)
        {
            base.RefreshPickList(asset);

            m_NameField = new TextField(string.Empty)
            {
                bindingPath = m_PropertyName.propertyPath
            };
            
            m_NameField.Bind(m_Property.serializedObject);

            m_NameDropdown = new Image
            {
                image = ICON_DROPDOWN.Texture,
                name = NAME_DROPDOWN,
                focusable = true
            };
            
            m_NameDropdown.SetEnabled(asset != null);
            m_NameDropdown.RegisterCallback<MouseDownEvent>(evt =>
            {
                // if (evt.button == (int)MouseButton.LeftMouse)
                {
                    // m_NameDropdown.Focus();
                    evt.StopPropagation();
                }
            });
            m_NameDropdown.AddManipulator(new MouseDropdownManipulator(context =>
            {
                var listNames = GetVariablesList(asset);
                foreach (var entry in listNames)
                {
                    context.menu.AppendAction(
                        entry.Key,
                        menuAction =>
                        {
                            m_PropertyName.serializedObject.Update();
                            m_PropertyName.stringValue = menuAction.name;
                            
                            EventChangeSelection?.Invoke(menuAction.name, Asset);

                            m_PropertyName.serializedObject.ApplyModifiedProperties();
                            m_PropertyName.serializedObject.Update();
                        },
                        menuAction =>
                        {
                            if (menuAction.name != m_PropertyName.stringValue)
                            {
                                return entry.Value
                                    ? DropdownMenuAction.Status.Normal
                                    : DropdownMenuAction.Status.Disabled;
                            }
                            
                            return DropdownMenuAction.Status.Checked;
                        }
                    );
                }
            }));
            
            var nameContainer = new VisualElement { name = NAME_ROOT_NAME };
            
            nameContainer.Add(new Label(m_FieldName));
            nameContainer.Add(m_NameField);
            nameContainer.Add(m_NameDropdown);
            
            AlignLabel.On(nameContainer);

            Add(nameContainer);
        }

        private Dictionary<string, bool> GetVariablesList(Object asset)
        {
            var variable = asset as StateMachineAsset;
            
            if (variable == null) return new Dictionary<string, bool> {{ string.Empty, false }};

            var names = variable.GetType()
                .GetField("m_NameList", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(variable) as NameList;

            if (!m_IgnoreFilter) return FilterNames(names);
            var list = new Dictionary<string, bool> {{ string.Empty, false }};
            for (var i = 0; i < names?.Length; ++i)
            {
                var nameVariable = names.Get(i);
                list[nameVariable.Name] = true;
            }
            return list;
        }
    }
}