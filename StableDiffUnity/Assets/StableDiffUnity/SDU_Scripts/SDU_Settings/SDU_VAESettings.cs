using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    public class SDU_VAESettings : UnityJsonSerializable, UCLI_FieldOnGUI
    {
        static private SDU_CancellationTokenSource s_CTS = new SDU_CancellationTokenSource();
        public const FolderEnum Folder = FolderEnum.VAE;

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
        public async UniTask ApplyToServer()//CancellationToken? iCancellationToken = null
        {
            string aCheckPoint = m_VAE;
            var aCTS = s_CTS.Create();
            try
            {
                await SetVAEAsync(aCheckPoint, aCTS.Token);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"ApplyToServer CheckPoint:{aCheckPoint},Exception:{e.Message}");
            }
            s_CTS.TryCancel(aCTS);

        }
        private static async UniTask SetVAEAsync(string iVAE, CancellationToken iCancellationToken)
        {
            Debug.Log($"SetVAEAsync Start, VAE:{iVAE}");

            if (!SDU_Server.ServerReady)
            {
                await UniTask.WaitUntil(() => SDU_Server.ServerReady, cancellationToken: iCancellationToken);
            }
            Debug.Log($"SetVAEAsync SDU_Server.ServerReady, VAE:{iVAE}");
            iCancellationToken.ThrowIfCancellationRequested();
            using (var client = RunTimeData.Stablediffunity_API.Client_SetVAE)
            {
                JsonData aJson = new JsonData();
                string aPath = string.Empty;
                foreach (var aVAE in RunTimeData.WebUISetting.m_SdVAEs)
                {
                    if(aVAE.name == iVAE)
                    {
                        aPath = aVAE.path;
                        break;
                    }
                }
                aJson["vae"] = iVAE;
                aJson["path"] = aPath;
                string aResult = await client.SendAsyncUniTask(iCancellationToken, aJson.ToJson());
                Debug.Log($"SetVAEAsync Done, VAE:{iVAE},aResult:{aResult}");
            }

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
                    RunTimeData.WebUISetting.Refresh(Folder).Forget();
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
                    RunTimeData.InstallSetting.OpenFolder(Folder);
                }
            }
            return this;
        }
    }
}