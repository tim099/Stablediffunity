using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UCL.Core;
using UnityEngine;

namespace SDU
{
    public class SDU_CMD : UCL.Core.JsonLib.UnityJsonSerializable,
        UCL.Core.UCLI_TypeList, UCL.Core.UCLI_GetTypeName, UCL.Core.UI.UCLI_FieldOnGUI, UCL.Core.UCLI_ShortName
    {
        static List<System.Type> s_Types = null;
        public IList<System.Type> GetAllTypes()
        {
            if(s_Types == null)
            {
                s_Types = new List<System.Type>();
                s_Types.Add(typeof(SDU_CMDGroup));
                s_Types.Add(typeof(SDU_CMDGenerateImage));
                s_Types.Add(typeof(SDU_CMDSetCheckPoint));
            }
            return s_Types;
        }
        virtual public string GetTypeName(string iName) => iName.Replace("SDU_CMD", string.Empty);
        virtual public string GetShortName() => GetTypeName(GetType().Name);
        virtual public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting)
        {
            await Task.Delay(1);
            return;
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("GenerateImageCMD"), iFieldName, false);
            return this;
        }
    }
}