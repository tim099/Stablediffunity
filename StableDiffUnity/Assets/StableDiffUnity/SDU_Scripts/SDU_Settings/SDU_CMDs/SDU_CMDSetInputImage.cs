using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UCL.Core;
using UCL.Core.UI;

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
        public override object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
            if(m_SetInputImageMode == SetInputImageMode.PrevGeneratedImage)
            {
                if (SDU_ImageGenerator.PrevGeneratedImage != null)
                {
                    var aSubDic = iDataDic.GetSubDic("PrevGeneratedImage");
                    UCL.Core.UI.UCL_GUILayout.DrawObjectData(SDU_ImageGenerator.PrevGeneratedImage, aSubDic, "PrevGeneratedImage", false);
                    if (SDU_ImageGenerator.PrevGeneratedImage.ShowImageDetail)
                    {
                        if (GUILayout.Button("Clear PrevGeneratedImage", UCL_GUIStyle.ButtonStyle))
                        {
                            SDU_ImageGenerator.ClearPrevGeneratedImage();
                        }
                    }
                }

            }
            return this;
        }
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
