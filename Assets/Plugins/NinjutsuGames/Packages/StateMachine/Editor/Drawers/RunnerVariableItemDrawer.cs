using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using NinjutsuGames.StateMachine.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NinjutsuGames.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(RunnerVariableItem))]
    public class RunnerVariableItemDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var type = property.FindPropertyRelative("m_Type").FindPropertyRelative("m_String");
            var value = property.FindPropertyRelative("m_Value");
            var valueField = new PropertyField(value);
            
            root.Add(new SpaceSmallest());
            
            var toolPickName = new StateMachinePickTool(property, true, "Variable");
            toolPickName.EventChangeSelection += (selection, asset) =>
            {
                var stateMachine = asset as StateMachineAsset;
                if (stateMachine != null)
                {
                    var index = ArrayUtility.IndexOf(stateMachine.NameList.Names, selection);
                    var variable = stateMachine.NameList.Get(index);
                    if(variable != null)
                    {
                        if (variable.TypeID == ValueGameObject.TYPE_ID) value.SetManaged(GetGameObjectInstance.Create());
                        if (variable.TypeID == ValueString.TYPE_ID && value.GetManagedValue()?.GetType() != typeof(PropertyTypeGetString)) value.SetManaged(GetStringString.Create);
                        if (variable.TypeID == ValueNumber.TYPE_ID) value.SetManaged(GetDecimalDecimal.Create(0));
                        if (variable.TypeID == ValueBool.TYPE_ID) value.SetManaged(GetBoolTrue.Create);
                        if (variable.TypeID == ValueVector3.TYPE_ID) value.SetManaged(GetPositionVectorZero.Create());
                        if (variable.TypeID == ValueSprite.TYPE_ID) value.SetManaged(GetSpriteInstance.Create());
                        if (variable.TypeID == ValueColor.TYPE_ID) value.SetManaged(GetColorColorsWhite.Create);
                        if (variable.TypeID == ValueAnimClip.TYPE_ID) value.SetManaged(GetAnimationInstance.Create);
                        if (variable.TypeID == ValueTexture.TYPE_ID) value.SetManaged(GetTextureInstance.Create());
                        if (variable.TypeID == ValueMaterial.TYPE_ID) value.SetManaged(GetMaterialInstance.Create());
                        if (variable.TypeID == ValueAudioClip.TYPE_ID) value.SetManaged(GetAudioClip.Create);
                        if (variable.TypeID == ValueLocalList.TYPE_ID) value.SetManaged(new GetGameObjectLocalList());
                        
                        type.stringValue = variable.TypeID.String;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    valueField.style.display = value.managedReferenceValue == null ? DisplayStyle.None : DisplayStyle.Flex;
                    property.serializedObject.Update();
                }
            };
            toolPickName.RefreshPickList();
            root.Add(toolPickName);
            
            valueField.style.display = value.managedReferenceValue == null ? DisplayStyle.None : DisplayStyle.Flex;
            root.Add(valueField);
            return root;
        }
    }
}