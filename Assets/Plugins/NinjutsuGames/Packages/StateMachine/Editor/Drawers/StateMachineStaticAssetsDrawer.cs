using GameCreator.Editor.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(StateMachineStaticAssets))]
    public class StateMachineStaticAssetsDrawer : TBoxDrawer
    {
        private const string Info = "This is a list of State Machine Assets that automatically instantiate at runtime and persist throughout the game.";
        protected override void CreatePropertyContent(VisualElement container, SerializedProperty property)
        {
            var info = new InfoMessage(Info);
            container.Add(info);
            base.CreatePropertyContent(container, property);
        }
    }
}