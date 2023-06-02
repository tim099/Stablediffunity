using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDSetNegativePrompt : SDU_CMD
    {
        public string m_NegativePrompt = string.Empty;
        override public string GetShortName() => $"{base.GetShortName()}({m_NegativePrompt.CutToMaxLength(20)})";
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.RequireClearDic = true;
            iTex2ImgSetting.m_NegativePrompt = m_NegativePrompt;
            await Task.Delay(1);
        }
    }
}