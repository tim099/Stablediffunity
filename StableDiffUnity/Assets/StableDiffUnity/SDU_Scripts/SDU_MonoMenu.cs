using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SDU
{
    public class SDU_MonoMenu : MonoBehaviour
    {
        private void Start()
        {
            UCL.Core.EditorLib.Page.UCL_EditorPage.Create<SDU_EditorMenuPage>();//UCL.Core.UI.UCL_GUIPageController.Ins
            //UCL.Core.UI.UCL_GUIPageController.Ins.Push()
        }
        private void OnDestroy()
        {
            SDU.RunTimeData.SaveRunTimeData();
        }
    }
}