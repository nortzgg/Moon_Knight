using System.Linq;
using System.Reflection;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Editor
{
	public class RunnerVariableItemTool : TPolymorphicItemTool
    {
        private readonly IIcon Icon = new IconNull(ColorTheme.Type.TextLight);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override object Value => m_Property.GetValue<RunnerVariableItem>();
        
        protected override Texture2D GetIcon()
        {
            m_Property.serializedObject.Update();
            var instance = Value as RunnerVariableItem;

            var main = instance?.Value?.GetType();
            var iconField = main?.GetField("m_Property", BindingFlags.NonPublic | BindingFlags.Instance);
            var iconAttrs = iconField?.GetValue(instance?.Value).GetType().GetCustomAttributes<ImageAttribute>();
            var icon = iconAttrs?.FirstOrDefault()?.Image;
            return icon != null ? icon : Icon.Texture;
        }
        
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public RunnerVariableItemTool(RunnerVariableListTool parentTool, int index)
            : base(parentTool, index)
        {
            m_Property.serializedObject.Update();
            m_Property.FindPropertyRelative("m_Variable").objectReferenceValue = parentTool.StateMachineAsset;
            m_Property.serializedObject.ApplyModifiedProperties();
        }
    }
}