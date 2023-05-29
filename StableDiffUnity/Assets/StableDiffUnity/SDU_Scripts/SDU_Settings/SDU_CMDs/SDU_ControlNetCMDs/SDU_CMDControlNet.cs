using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDControlNet : SDU_CMD
    {

        override public string GetShortName() => $"{base.GetShortName()}";
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.m_ControlNetSettings.RequireClearDic = true;

            await Task.Delay(1);
        }
    }
}

