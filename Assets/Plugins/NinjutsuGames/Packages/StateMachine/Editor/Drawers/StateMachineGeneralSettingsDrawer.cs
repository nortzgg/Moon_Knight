using GameCreator.Editor.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;

namespace NinjutsuGames.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(StateMachineGeneralSettings))]
    public class StateMachineGeneralSettingsDrawer : TTitleDrawer
    {
        protected override string Title => "General";
    }
}