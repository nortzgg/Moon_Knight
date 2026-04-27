using GameCreator.Editor.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(StateMachineItem))]
    public class StateMachineItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var type = property.FindPropertyRelative("m_InstanceType");
            var fieldType = new PropertyField(type);
            
            var asset = property.FindPropertyRelative("m_Asset");
            var fieldAsset = new PropertyField(asset);
            
            var prefab = property.FindPropertyRelative("m_Prefab");
            var fieldPrefab = new PropertyField(prefab);
            
            fieldType.BindProperty(type);
            fieldAsset.BindProperty(asset);
            fieldPrefab.BindProperty(prefab);

            
            root.Add(new SpaceSmallest());
            root.Add(fieldType);
            root.Add(fieldAsset);
            root.Add(fieldPrefab);
            
            SetFieldVisibility((StateMachineItem.InstanceType)type.enumValueIndex);

            fieldType.RegisterValueChangeCallback(evt =>
            {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                
                SetFieldVisibility((StateMachineItem.InstanceType)evt.changedProperty.enumValueIndex);
            });

            return root;

            void SetFieldVisibility(StateMachineItem.InstanceType evt)
            {
                fieldAsset.style.display = evt == StateMachineItem.InstanceType.Asset ? DisplayStyle.Flex : DisplayStyle.None;
                fieldPrefab.style.display = evt == StateMachineItem.InstanceType.Prefab ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}