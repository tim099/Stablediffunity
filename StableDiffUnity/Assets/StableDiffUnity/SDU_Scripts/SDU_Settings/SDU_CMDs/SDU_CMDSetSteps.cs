using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDSetSteps : SDU_CMD
    {
        [UCL.Core.PA.UCL_IntSlider(1, 150)]
        public int m_Steps = 20;
        override public string GetShortName() => $"{base.GetShortName()}({m_Steps})";
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting)
        {
            iTex2ImgSetting.m_Steps = m_Steps;
            await Task.Delay(1);
        }
    }
}
