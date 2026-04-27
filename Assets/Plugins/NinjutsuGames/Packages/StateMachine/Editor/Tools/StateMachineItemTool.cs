using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Editor
{
	public class StateMachineItemTool : TPolymorphicItemTool
    {
        private readonly IIcon IconStateMachine = new IconStateMachine(ColorTheme.Type.Blue);
        private readonly IIcon IconPrefab = new IconCubeSolid(ColorTheme.Type.Blue);
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override object Value => m_Property.GetValue<StateMachineItem>();
        
        protected override Texture2D GetIcon()
        {
            m_Property.serializedObject.Update();
            var instance = Value as StateMachineItem;

            return instance?.Type switch
            {
                StateMachineItem.InstanceType.Asset => IconStateMachine.Texture,
                StateMachineItem.InstanceType.Prefab => IconPrefab.Texture,
                _ => null
            };
        }
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public StateMachineItemTool(IPolymorphicListTool parentTool, int index)
            : base(parentTool, index)
        { }
    }
}