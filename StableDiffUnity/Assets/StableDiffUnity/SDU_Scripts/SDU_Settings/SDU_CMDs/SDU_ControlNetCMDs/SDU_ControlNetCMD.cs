using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace SDU
{
    public class SDU_ControlNetCMD
    {
        virtual public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.m_ControlNetSettings.RequireClearDic = true;

            await Task.Delay(1);
        }
    }
}