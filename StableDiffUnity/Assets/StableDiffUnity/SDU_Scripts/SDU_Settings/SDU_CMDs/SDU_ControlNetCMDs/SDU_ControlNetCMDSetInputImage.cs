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
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, int iTargetControlNetID, System.Threading.CancellationToken iCancellationToken)
        {
            var aSetting = iTex2ImgSetting.GetControlNetSetting(iTargetControlNetID);
            if(aSetting == null)
            {
                Debug.LogError("SDU_ControlNetCMDSetInputImage, aSetting == null");
                return;
            }
            Debug.LogWarning($"Set SDU_InputImage FilePath:{m_InputImage.m_LoadImageSetting.FilePath}");
            aSetting.SetInputImage(m_InputImage);
            await Task.Delay(1);
        }
    }
}
