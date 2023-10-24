using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UnityEngine;

namespace SDU
{
    public class SDU_LoraSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UCLI_ShortName, UCL.Core.UI.UCLI_IsEnable//, UCL.Core.UI.UCLI_FieldOnGUI
    {
        
        public static IList<string> LoraNames => RunTimeData.Ins.m_WebUISetting.m_LoraNames;
        [UCL.Core.ATTR.UCL_DropDown("LoraNames")]
        public string m_Lora;

        [UCL.Core.PA.UCL_Slider(0f, 1f)]
        public float m_Weight = 0.8f;

        [UCL.Core.ATTR.UCL_HideOnGUI]public bool m_IsEnable = true;
        public string GetShortName() => Prompt;
        public string Prompt => $"<lora:{m_Lora}:{m_Weight.ToString("0.##")}>";

        public bool IsEnable { get => m_IsEnable; set => m_IsEnable = value; }
        //object OnGUI(string iFieldName, UCL_ObjectDictionary iDic)
        //{

        //}
    }
}
