using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.UI;
using UnityEngine;

namespace SDU
{
    public class SDU_LoraSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UCLI_ShortName, UCL.Core.UI.UCLI_IsEnable
        , UCL.Core.UCLI_NameOnGUI//, UCL.Core.UI.UCLI_FieldOnGUI
    {
        
        public static IList<string> LoraNames => RunTimeData.Ins.m_WebUISetting.m_LoraNames;
        [UCL.Core.ATTR.UCL_DropDown("LoraNames")]
        public string m_Lora;

        [UCL.Core.PA.UCL_Slider(0.01f, 1f)]
        public float m_Weight = 0.8f;

        [UCL.Core.ATTR.UCL_HideOnGUI]public bool m_IsEnable = true;
        public string GetShortName() => Prompt;
        public string Prompt => $"<lora:{m_Lora}:{m_Weight.ToString("0.##")}>";

        public bool IsEnable { get => m_IsEnable; set => m_IsEnable = value; }


        public void NameOnGUI(UCL_ObjectDictionary iDataDic, string iDisplayName)
        {
            using (var aScope2 = new GUILayout.HorizontalScope(GUILayout.Width(650)))
            {
                m_IsEnable = UCL_GUILayout.CheckBox(m_IsEnable);
                m_Lora = UCL_GUILayout.PopupAuto(m_Lora, LoraNames, iDataDic, "Lora", 6, GUILayout.Width(220));
                m_Weight = UCL_GUILayout.Slider("Weight", m_Weight, 0.01f, 1f, iDataDic.GetSubDic("Weight"));
            }
        }
        //object OnGUI(string iFieldName, UCL_ObjectDictionary iDic)
        //{

        //}
    }
}
