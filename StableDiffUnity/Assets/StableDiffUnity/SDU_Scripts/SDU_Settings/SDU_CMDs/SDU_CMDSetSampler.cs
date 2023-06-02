using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace SDU
{
    public class SDU_CMDSetSampler : SDU_CMD
    {
        public SDU_SamplerSetting m_Sampler = new SDU_SamplerSetting();
        override public string GetShortName() => $"{base.GetShortName()}({m_Sampler.m_SelectedSampler})";
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.RequireClearDic = true;
            iTex2ImgSetting.m_Sampler.Set(m_Sampler);
            await Task.Delay(1);
        }
    }
}
