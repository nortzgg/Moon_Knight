using GameCreator.Editor.Common;
using NinjutsuGames.StateMachine.Fusion.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Fusion.Editor
{
    [CustomEditor(typeof(StateMachineRunnerNetwork))]
    public class StateMachineRunnerNetworkEditor : UnityEditor.Editor
    {
        private static readonly StyleLength DefaultMarginTop = new(5);
        private const string InfoText = "This synchronizes variables and nodes between clients";

        // MEMBERS: -------------------------------------------------------------------------------
        
        // INITIALIZERS: --------------------------------------------------------------------------

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement
            {
                style =
                {
                    marginTop = DefaultMarginTop
                }
            };
            container.Add(new PropertyField(serializedObject.FindProperty("targetRunner")));
            container.Add(new InfoMessage(InfoText));
            return container;
        }
    }
}