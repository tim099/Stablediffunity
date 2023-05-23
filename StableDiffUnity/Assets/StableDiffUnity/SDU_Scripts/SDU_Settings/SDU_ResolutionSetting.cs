using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UnityEngine;


namespace SDU
{
    [UCL.Core.ATTR.EnableUCLEditor]
    [System.Serializable]
    public class ResolutionSetting : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public int m_Width = 1920;
        public int m_Height = 1080;
        public FullScreenMode m_FullScreenMode = FullScreenMode.Windowed;

        [UCL.Core.ATTR.UCL_FunctionButton("Apply Resolution Settings")]
        public void ApplyResolutionSetting()
        {
            Screen.SetResolution(m_Width, m_Height, m_FullScreenMode);
        }
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName);
            //if (GUILayout.Button("Apply Resolution Settings"))
            //{
            //    Screen.SetResolution(m_Width, m_Height, m_FullScreenMode);
            //}
            return this;
        }
    }
}