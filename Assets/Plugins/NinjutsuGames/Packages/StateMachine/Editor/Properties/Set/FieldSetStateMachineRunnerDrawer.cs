using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime;
using NinjutsuGames.StateMachine.Runtime.Variables;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(FieldSetStateMachineRunner))]
    public class FieldSetStateMachineRunnerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            
            var runner = property.FindPropertyRelative("m_Runner");
            var variable = property.FindPropertyRelative("m_Variable");
            var typeID = property.FindPropertyRelative("m_TypeID");

            var fieldVariable = new ObjectField(variable.displayName)
            {
                allowSceneObjects = false,
                objectType = typeof(StateMachineAsset),
                bindingPath = variable.propertyPath
            };
            
            var typeIDStr = typeID.FindPropertyRelative(IdStringDrawer.NAME_STRING);
            var typeIDValue = new IdString(typeIDStr.stringValue);
            
            var toolPickName = new StateMachinePickTool(
                property,
                typeIDValue,
                false
            );
            
            fieldVariable.RegisterValueChangedCallback(_ => toolPickName.OnChangeAsset());

            root.Add(new PropertyField(runner));

            AlignLabel.On(fieldVariable);
            root.Add(fieldVariable);
            root.Add(toolPickName);
            
            try
            {
                property.serializedObject.Update();
                if (StateMachineAsset.Active != null && 
                    variable != null && variable.serializedObject != null &&
                    variable.objectReferenceValue == null && 
                    variable.objectReferenceValue != StateMachineAsset.Active)
                {
                    variable.objectReferenceValue = StateMachineAsset.Active;
                    SerializationUtils.ApplyUnregisteredSerialization(property.serializedObject);
                }
            }
            catch (System.Exception)
            {
                // SerializedProperty has been disposed, ignore
            }

            return root;
        }
    }
}