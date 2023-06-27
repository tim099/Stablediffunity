using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.JsonLib;
using UnityEngine;
namespace SDU
{
    public class SDU_WebUICMD : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI, UCL.Core.UCLI_TypeList
    {
        private static List<System.Type> s_Types = null;
        public IList<System.Type> GetAllTypes()
        {
            if (s_Types == null)
            {
                s_Types = new List<System.Type>();
                s_Types.Add(typeof(SDU_WebUICMDOutputTensors));
            }
            return s_Types;
        }
        virtual public JsonData GetConfigJson()
        {
            JsonData aData = new JsonData();
            aData["Class"] = this.GetType().Name;
            aData["Data"] = this.SerializeToJson();
            return aData;
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName);


            return this;
        }
    }
}