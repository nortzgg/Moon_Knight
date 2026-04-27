using System;
using System.Linq.Expressions;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    public static class TypeExtension
    {
        public static bool IsReallyAssignableFrom(this Type type, Type otherType)
        {
            if (type.IsAssignableFrom(otherType))
                return true;
            if (otherType.IsAssignableFrom(type))
                return true;

            try
            {
                var v = Expression.Variable(otherType);
                var expr = Expression.Convert(v, type);
                return expr.Method != null && expr.Method.Name != "op_Implicit";
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static T Clone<T>(this T target)
        {
            return (T) JsonUtility.FromJson(JsonUtility.ToJson(target), target.GetType());
        }
        
        public static InstructionList PartialClone(this InstructionList instructionList)
        {
            if (instructionList == null) return null;

            // Create a new list of instructions
            var clonedInstructions = new Instruction[instructionList.Length];
        
            for (var i = 0; i < instructionList.Length; i++)
            {
                clonedInstructions[i] = instructionList.Get(i); // Assuming shallow copy is enough
            }

            // Return a new `InstructionList` with the cloned instructions
            return new InstructionList(clonedInstructions);
        }
        
        public static ConditionList PartialClone(this ConditionList conditionList)
        {
            if (conditionList == null) return null;

            var clonedConditions = new Condition[conditionList.Length];
            for (var i = 0; i < conditionList.Length; i++)
            {
                clonedConditions[i] = conditionList.Get(i); // Assuming shallow copy is enough
            }

            return new ConditionList(clonedConditions);
        }
    }
}