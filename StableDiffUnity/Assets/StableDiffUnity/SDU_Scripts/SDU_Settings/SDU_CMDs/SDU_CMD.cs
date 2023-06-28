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
                s_Types.Add(typeof(SDU_CMDSetSampler));
                s_Types.Add(typeof(SDU_CMDSetPrompt));
                s_Types.Add(typeof(SDU_CMDSetNegativePrompt));
                s_Types.Add(typeof(SDU_CMDSetSize));
                s_Types.Add(typeof(SDU_CMDSetSteps));
                s_Types.Add(typeof(SDU_CMDSetCfgScale));
                s_Types.Add(typeof(SDU_CMDSetSeed));
                s_Types.Add(typeof(SDU_CMDSetInputImage));

                s_Types.Add(typeof(SDU_CMDForeach));
                s_Types.Add(typeof(SDU_CMDControlNet));
            }
            return s_Types;
        }
        
        virtual public string GetTypeName(string iName) => iName.Replace("SDU_CMD", string.Empty);
        virtual public string GetShortName() => GetTypeName(GetType().Name);
        virtual public List<SDU_CMD> GetCMDList() => new List<SDU_CMD>() { this };

        virtual public async Task TriggerCMD(SDU_ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            await Task.Delay(1);
            return;
        }
        virtual public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
            return this;
        }
    }
}