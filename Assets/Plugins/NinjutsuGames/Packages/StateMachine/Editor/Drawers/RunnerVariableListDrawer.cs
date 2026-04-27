using GameCreator.Editor.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(RunnerVariableList))]
    public class RunnerVariableListDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var stateMachine = property.FindPropertyRelative("m_Node");
            root.Add(new SpaceSmaller());
            root.Add(new PropertyField(stateMachine));
            root.Add(new SpaceSmaller());
            root.Add(new RunnerVariableListTool(property));
            return root;
        }
    }
}