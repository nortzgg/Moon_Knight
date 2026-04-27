using GameCreator.Editor.Common;
using NinjutsuGames.StateMachine.Fusion.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Fusion.Editor
{
    [CustomPropertyDrawer(typeof(FusionNodeConfig), true)]
    public class FusionNodeConfigDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            SerializationUtils.CreateChildProperties(root, property, SerializationUtils.ChildrenMode.ShowLabelsInChildren, true);
            return root;
        }
    }
}