using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace SDU
{
    public class SDU_EnumGroup : SDU_Enumerable
    {
        public List<SDU_CMD> m_CMDs = new List<SDU_CMD>();

        override public string GetShortName()
        {
            if (m_CMDs.IsNullOrEmpty()) return base.GetShortName();
            return $"[{m_CMDs.ConcatString((iCMD) => iCMD.GetShortName())}]".CutToMaxLength(30);
        }
        override public SDU_Enumerator GetEnumerator()
        {
            return new SDU_Enumerator(m_CMDs.Clone());
        }
    }
}