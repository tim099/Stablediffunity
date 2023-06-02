using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDControlNet : SDU_CMD
    {
        public List<SDU_ControlNetCMD> m_ControlNetCMDs = new List<SDU_ControlNetCMD>();
        override public string GetShortName() => $"{base.GetShortName()}";
        public override List<SDU_CMD> GetCMDList()
        {
            var aList = new List<SDU_CMD>();
            foreach (var aCmd in m_ControlNetCMDs)
            {
                var aCMDCN = new SDU_CMDControlNet();
                aCMDCN.m_ControlNetCMDs.Add(aCmd);
                aList.Add(aCMDCN);
            }
            return aList;
        }
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.m_ControlNetSettings.RequireClearDic = true;
            var aCMDs = m_ControlNetCMDs.Clone();
            foreach (var aCMD in aCMDs)
            {
                if (iCancellationToken.IsCancellationRequested) break;
                await aCMD.TriggerCMD(iTex2ImgSetting, iCancellationToken);
            }
        }
    }
}

