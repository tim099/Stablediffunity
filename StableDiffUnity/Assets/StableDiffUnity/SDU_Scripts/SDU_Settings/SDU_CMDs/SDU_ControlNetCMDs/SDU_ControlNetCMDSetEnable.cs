using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UCL.Core;
using UnityEngine;
namespace SDU
{
    public class SDU_ControlNetCMDSetEnable : SDU_ControlNetCMD
    {
        public bool m_EnableControlNet = true;
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            Debug.LogWarning($"Set EnableControlNet:{m_EnableControlNet}");
            iTex2ImgSetting.m_ControlNetSettings.RequireClearDic = true;
            iTex2ImgSetting.m_ControlNetSettings.m_EnableControlNet = m_EnableControlNet;
            await Task.Delay(1);
        }
    }
}