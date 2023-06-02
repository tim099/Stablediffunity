using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace SDU
{
    public class SDU_CMDSetInputImage : SDU_CMD
    {
        public enum SetInputImageMode
        {
            InputImage,
            PrevGeneratedImage,
        }
        public SetInputImageMode m_SetInputImageMode = SetInputImageMode.InputImage;

        [UCL.Core.PA.Conditional("m_SetInputImageMode",false, SetInputImageMode.InputImage)]
        public SDU_InputImage m_InputImage = new SDU_InputImage();
        override public string GetShortName() => $"{base.GetShortName()}";
        override public async Task TriggerCMD(SDU_ImgSetting iImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            iImgSetting.RequireClearDic = true;
            SDU_InputImage aImage = null;
            switch (m_SetInputImageMode)
            {
                case SetInputImageMode.InputImage:
                    {
                        aImage = m_InputImage;
                        break;
                    }
                case SetInputImageMode.PrevGeneratedImage:
                    {
                        aImage = SDU_ImageGenerator.PrevGeneratedImage;
                        break;
                    }
            }
            iImgSetting.SetInputImage(aImage);
            await Task.Delay(1);
        }
    }
}
