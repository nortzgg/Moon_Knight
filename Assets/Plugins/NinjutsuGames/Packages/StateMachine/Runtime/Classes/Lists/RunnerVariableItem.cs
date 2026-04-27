using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    [Description("A state machine runner variable.")]
    [Title("Runner Variable")]
    public class RunnerVariableItem : TPolymorphicItem<RunnerVariableItem>
    {
        [SerializeField] private IdPathString m_Name;
        [SerializeField] protected StateMachineAsset m_Variable;
        [SerializeReference] protected object m_Value;
        [SerializeField] protected IdString m_Type = IdString.EMPTY;

        public RunnerVariableItem(StateMachineAsset stateMachineAsset)
        {
            m_Variable = stateMachineAsset;
        }

        public object Value => m_Value;
        public string Name => m_Name.String;
        public override string Title => $"{(string.IsNullOrEmpty(Name) ? "(none)" : Name)} {m_Value}";
        
        public object GetValue(Args args)
        {
            if(m_Type == ValueGameObject.TYPE_ID) return (m_Value as TPropertyGet<PropertyTypeGetGameObject, GameObject>)?.Get(args);
            if(m_Type == ValueString.TYPE_ID) return (m_Value as TPropertyGet<PropertyTypeGetString, string>)?.Get(args);
            if(m_Type == ValueNumber.TYPE_ID) return (m_Value as TPropertyGet<PropertyTypeGetDecimal, double>)?.Get(args);
            if(m_Type == ValueBool.TYPE_ID) return (m_Value as TPropertyGet<PropertyTypeGetBool, bool>)?.Get(args);
            if(m_Type == ValueVector3.TYPE_ID) return (m_Value as TPropertyGet<TPropertyTypeGet<Vector3>, Vector3>)?.Get(args);
            if(m_Type == ValueSprite.TYPE_ID) return (m_Value as PropertyGetSprite)?.Get(args);
            if(m_Type == ValueColor.TYPE_ID) return (m_Value as PropertyGetColor)?.Get(args);
            if(m_Type == ValueAnimClip.TYPE_ID) return (m_Value as PropertyGetAnimation)?.Get(args);
            if(m_Type == ValueTexture.TYPE_ID) return (m_Value as PropertyGetTexture)?.Get(args);
            if(m_Type == ValueMaterial.TYPE_ID) return (m_Value as PropertyGetMaterial)?.Get(args);
            if(m_Type == ValueAudioClip.TYPE_ID) return (m_Value as PropertyGetAudio)?.Get(args);
            return m_Value;
        }
    }
}