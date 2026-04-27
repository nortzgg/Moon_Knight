using NinjutsuGames.StateMachine.Editor;
using NinjutsuGames.StateMachine.Fusion.Runtime;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Fusion.Editor
{
    [CustomPropertyDrawer(typeof(NetworkingSettings), true)]
    public class FusionSettingsDrawer : NetworkSettingsDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return DrawSettings(property);
        }

        protected override void OnNetworkSyncChanged(BaseGameCreatorNode node, bool enabled)
        {
            node.networkingSettings.config =
                enabled ? new FusionNodeConfig() : new TNetworkConfig();
        }
    }
}