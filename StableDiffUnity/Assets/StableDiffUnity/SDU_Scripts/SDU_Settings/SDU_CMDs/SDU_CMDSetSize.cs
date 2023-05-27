using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDSetSize : SDU_CMD
    {
        public int m_Width = 512;
        public int m_Height = 512;
        override public string GetShortName() => $"{base.GetShortName()}({m_Width},{m_Height})";
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting)
        {
            iTex2ImgSetting.m_Width = m_Width;
            iTex2ImgSetting.m_Height = m_Height;
            await Task.Delay(1);
        }
    }
}