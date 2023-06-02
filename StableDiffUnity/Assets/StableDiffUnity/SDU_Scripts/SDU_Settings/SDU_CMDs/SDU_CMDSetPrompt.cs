using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDSetPrompt : SDU_CMD
    {
        public string m_Prompt = string.Empty;
        override public string GetShortName() => $"{base.GetShortName()}({m_Prompt.CutToMaxLength(20)})";
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.RequireClearDic = true;
            iTex2ImgSetting.m_Prompt = m_Prompt;
            await Task.Delay(1);
        }
    }
}