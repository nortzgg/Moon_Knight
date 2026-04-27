using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(StateMachineList))]
    public class StateMachineListDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new StateMachineListTool(property);
        }
    }
}