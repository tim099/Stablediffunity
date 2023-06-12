using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace SDU
{
    public class SDU_CMDSetCheckPoint : SDU_CMD
    {
        public SDU_CheckPointSetting m_CheckPointSetting = new SDU_CheckPointSetting();
        override public string GetShortName() => $"{base.GetShortName()}({m_CheckPointSetting.m_CheckPoint})";
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.m_CheckPoint.Set(m_CheckPointSetting.m_CheckPoint);
            await Task.Delay(1);
        }
    }
}