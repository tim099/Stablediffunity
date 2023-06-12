using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    public class SDU_VAESettings : UnityJsonSerializable, UCLI_FieldOnGUI
    {
        public const string AutomaticKey = "Automatic";
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_VAE;

        public bool RequireClearDic { get; set; } = false;
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            Set(m_VAE);
        }
        public void Set(string iVAE)
        {
            RequireClearDic = true;
            m_VAE = iVAE;
            //Ask WebUI Server to SetVAE!!

        }
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            if (RequireClearDic)
            {
                RequireClearDic = false;
                iDataDic.Clear();
            }
            using (var aScope = new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Refresh", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.WebUISetting.RefreshVAEs().Forget();
                }

                GUILayout.Label(iFieldName, UCL.Core.UI.UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));

                var aNames = new List<string>();
                aNames.Add(AutomaticKey);
                foreach (var aVAE in RunTimeData.WebUISetting.m_SdVAEs)
                {
                    aNames.Add(aVAE.name);
                }
                
                if (!aNames.IsNullOrEmpty())
                {
                    string aNewVAE = UCL_GUILayout.PopupAuto(m_VAE, aNames, iDataDic, "Selected Model", 8);
                    if(aNewVAE != m_VAE)//Set VAE
                    {
                        Set(aNewVAE);
                    }
                }

                if (GUILayout.Button("Open Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.InstallSetting.OpenFolder(FolderEnum.VAE);
                }
            }
            return this;
        }
    }
}