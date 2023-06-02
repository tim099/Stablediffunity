using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDSetSeed : SDU_CMD
    {
        public long m_Seed = -1;
        override public string GetShortName() => $"{base.GetShortName()}({m_Seed})";
        override public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iTex2ImgSetting.RequireClearDic = true;
            iTex2ImgSetting.m_Seed = m_Seed;
            await Task.Delay(1);
        }
    }
}