using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDSetCfgScale : SDU_CMD
    {
        [UCL.Core.PA.UCL_Slider(1, 30)]
        public float m_CfgScale = 7;
        override public string GetShortName() => $"{base.GetShortName()}({m_CfgScale})";
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.RequireClearDic = true;
            iTex2ImgSetting.m_CfgScale = m_CfgScale;
            await Task.Delay(1);
        }
    }
}
