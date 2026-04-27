using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime.Variables
{
    [Serializable]
    public class FieldSetStateMachineList : TFieldSetVariable
    {
        [SerializeField] protected PropertyGetGameObject m_Runner = GetGameObjectSelf.Create();
        [SerializeField] protected StateMachineAsset m_Variable;
        [SerializeField] protected IdPathString m_Name;
        [SerializeReference] protected TListSetPick m_Select = new SetPickFirst();

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FieldSetStateMachineList(IdString typeID) //, IdString listID
        {
            m_TypeID = typeID;
            // m_List = new FieldSetLocalList(listID);
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
     
        public override void Set(object value, Args args)
        {
            if (m_Variable == null) return;
            var list = (LocalListVariables) m_Variable.Get(m_Name.String, m_Runner.Get(args));
            if(list && value != null) list.Set(m_Select, value, args);
        }
        
        public override object Get(Args args) => this.m_Variable != null
            ? this.m_Variable.Get(m_Name.String, m_Runner.Get(args))
            : null;
        
        public override string ToString() => this.m_Variable != null
            ? $"{m_Name.String}[{this.m_Select}]"
            : "(none)";
    }
}