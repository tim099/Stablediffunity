using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.JsonLib;
using UnityEngine;
namespace SDU
{
    public class SDU_WebUICMD : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI, UCL.Core.UCLI_TypeList,
        UCL.Core.UI.UCLI_IsEnable
    {
        private static List<System.Type> s_Types = null;
        public IList<System.Type> GetAllTypes()
        {
            if (s_Types == null)
            {
                s_Types = new List<System.Type>();
                s_Types.Add(typeof(SDU_WebUICMDOutputTensors));
                s_Types.Add(typeof(SDU_WebUICMDLoadTensor));
                s_Types.Add(typeof(SDU_WebUICMDLoadJsonTensor));
                //
            }
            return s_Types;
        }
        public bool IsEnable { get => m_Enable; set => m_Enable = value; }
        [SerializeField] [UCL.Core.ATTR.UCL_HideOnGUI] private bool m_Enable = true;

        virtual public JsonData GetConfigJson()
        {
            JsonData aData = new JsonData();
            aData["Class"] = this.GetType().Name;
            aData["Data"] = GetConfigJsonData();
            return aData;
        }
        virtual protected JsonData GetConfigJsonData()
        {
            return JsonConvert.SaveFieldsToJsonUnityVer(this);
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName);


            return this;
        }
    }
}