using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace SDU
{
    public class SDU_CMDGroup : SDU_CMD
    {
        public List<SDU_CMD> m_CMDs = new List<SDU_CMD>();

        override public string GetShortName()
        {
            if (m_CMDs.IsNullOrEmpty()) return base.GetShortName();
            return $"[{m_CMDs.ConcatString((iCMD) => iCMD.GetShortName())}]";
        }
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting)
        {
            if (m_CMDs.IsNullOrEmpty()) return;

            foreach(var aCMD in m_CMDs)
            {
                await aCMD.TriggerCMD(iTex2ImgSetting);
            }
        }
    }
}