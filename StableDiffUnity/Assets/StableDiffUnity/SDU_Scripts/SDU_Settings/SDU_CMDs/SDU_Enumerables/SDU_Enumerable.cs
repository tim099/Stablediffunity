using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UCL.Core;
using UnityEngine;


namespace SDU
{
    
    public class SDU_Enumerable : UCL.Core.JsonLib.UnityJsonSerializable,
        UCL.Core.UCLI_TypeList, UCL.Core.UCLI_GetTypeName, UCL.Core.UI.UCLI_FieldOnGUI, IEnumerable
    {
        static List<System.Type> s_Types = null;
        public IList<System.Type> GetAllTypes()
        {
            if (s_Types == null)
            {
                s_Types = new List<System.Type>();
                s_Types.Add(typeof(SDU_EnumGroup));
                s_Types.Add(typeof(SDU_EnumControlNetInputImages));
            }
            return s_Types;
        }
        virtual public string GetTypeName(string iName) => iName.Replace("SDU_Enum", string.Empty);
        virtual public string GetShortName() => GetTypeName(GetType().Name);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
        virtual public SDU_Enumerator GetEnumerator()
        {
            return new SDU_Enumerator(new List<SDU_CMD>());
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
            return this;
        }
    }

    public class SDU_Enumerator : IEnumerator
    {
        public IList<SDU_CMD> m_CMDs;

        int m_Position = -1;

        public SDU_Enumerator(IList<SDU_CMD> iCMDs)
        {
            m_CMDs = iCMDs;
        }

        public bool MoveNext()
        {
            m_Position++;
            return (m_Position < m_CMDs.Count);
        }

        public void Reset()
        {
            m_Position = -1;
        }

        object IEnumerator.Current => Current;

        public SDU_CMD Current
        {
            get
            {
                try
                {
                    return m_CMDs[m_Position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}