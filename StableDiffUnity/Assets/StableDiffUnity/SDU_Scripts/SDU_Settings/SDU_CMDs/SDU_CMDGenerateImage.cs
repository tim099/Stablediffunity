using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace SDU
{
    public class SDU_CMDGenerateImage : SDU_CMD
    {
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting)
        {
            await iTex2ImgSetting.GenerateImage();
            return;
        }
    }
}