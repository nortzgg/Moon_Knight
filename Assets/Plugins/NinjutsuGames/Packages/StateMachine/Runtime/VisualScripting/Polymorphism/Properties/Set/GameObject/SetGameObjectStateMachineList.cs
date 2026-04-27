using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Common;
using NinjutsuGames.StateMachine.Runtime.Variables;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Title("State Machine List Variable")]
    [Category("Variables/State Machine List Variable")]
    
    [Description("Sets the Game Object value of a State Machine List Variable")]
    [Image(typeof(IconStateMachineOverlayYellow), ColorTheme.Type.Teal, typeof(OverlayDot))]

    [Serializable] [HideLabelsInEditor]
    public class SetGameObjectStateMachineList : PropertyTypeSetGameObject
    {
        [SerializeField]
        protected FieldSetStateMachineList m_Variable = new(ValueLocalList.TYPE_ID);

        public override void Set(GameObject value, Args args) => m_Variable.Set(value, args);
        public override GameObject Get(Args args) => m_Variable.Get(args) as GameObject;
        
        public static PropertySetGameObject Create => new(
            new SetGameObjectStateMachineList()
        );
        
        public override string String => m_Variable.ToString();
    }
}