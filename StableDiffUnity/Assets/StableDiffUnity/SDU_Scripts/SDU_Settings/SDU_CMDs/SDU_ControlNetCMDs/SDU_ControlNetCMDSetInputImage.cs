using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UCL.Core;
using UnityEngine;
namespace SDU
{
    public class SDU_ControlNetCMDSetInputImage : SDU_ControlNetCMD
    {
        public SDU_InputImage m_InputImage = new SDU_InputImage();
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            Debug.LogWarning($"Set SDU_InputImage FilePath:{m_InputImage.m_LoadImageSetting.FilePath}");
            iTex2ImgSetting.m_ControlNetSettings.RequireClearDic = true;
            iTex2ImgSetting.m_ControlNetSettings.m_InputImage.DeserializeFromJson(m_InputImage.SerializeToJson());
            await Task.Delay(1);
        }
    }
}
