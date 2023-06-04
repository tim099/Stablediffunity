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
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, int iTargetControlNetID, System.Threading.CancellationToken iCancellationToken)
        {
            var aSetting = iTex2ImgSetting.GetControlNetSetting(iTargetControlNetID);
            if (aSetting == null)
            {
                Debug.LogError("SDU_ControlNetCMDSetEnable, aSetting == null");
                return;
            }
            Debug.LogWarning($"Set EnableControlNet:{m_EnableControlNet}");
            aSetting.SetEnable(m_EnableControlNet);
            await Task.Delay(1);
        }
    }
}