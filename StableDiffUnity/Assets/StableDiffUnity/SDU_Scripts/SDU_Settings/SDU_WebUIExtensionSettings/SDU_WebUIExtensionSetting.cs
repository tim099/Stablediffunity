using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    [System.Serializable]
    public class SDU_WebUIExtensionSetting : UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public class ConfigData
        {
            //public string FolderPath;
            [UCL.Core.ATTR.UCL_HideOnGUI] public string CurOutputImageName;
        }
        public ConfigData m_ConfigData = new ConfigData();

        public List<SDU_WebUICMD> m_WebUICMDs = new List<SDU_WebUICMD>();
        public override JsonData SerializeToJson()
        {
            return base.SerializeToJson();
        }
        /// <summary>
        /// Real JsonData that sent to WebUI
        /// </summary>
        /// <returns></returns>
        public JsonData GetConfigJson()
        {
            //if (string.IsNullOrEmpty(m_ConfigData.FolderPath))
            //{
            //    m_ConfigData.FolderPath = System.IO.Path.Combine(RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting.OutputFolderPath, "tensors");
            //}
            m_ConfigData.CurOutputImageName = SDU_ImageGenerator.CurOutputImageName;
            var aJson = JsonConvert.SaveFieldsToJsonUnityVer(m_ConfigData);
            if (m_WebUICMDs.Count > 0)
            {
                JsonData aWebUICMDs = new JsonData();
                aJson["WebUICMDs"] = aWebUICMDs;
                for (int i = 0; i < m_WebUICMDs.Count; i++)
                {
                    var aWebUICMD = m_WebUICMDs[i];
                    if(aWebUICMD.IsEnable)
                    {
                        aWebUICMDs.Add(aWebUICMD.GetConfigJson());
                    }
                }
            }

            return aJson;
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawObjExSetting aDrawObjExSetting = new()
            {
                OnShowField = () =>
                {
                    UCL.Core.UI.UCL_GUILayout.DrawObjectData(m_WebUICMDs, iDataDic.GetSubDic("WebUICMDs"), "WebUI CMDs");
                }
            };


            UCL.Core.UI.UCL_GUILayout.DrawField(m_ConfigData, iDataDic.GetSubDic("ConfigData"), iFieldName, iDrawObjExSetting: aDrawObjExSetting);

            
            return this;
        }
    }
}