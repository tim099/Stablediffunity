using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    public class SDU_CheckPointSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        static private SDU_CancellationTokenSource s_CTS = new SDU_CancellationTokenSource();
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_CheckPoint;
        
        public bool RequireClearDic { get; set; } = false;

        
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            //Set(m_CheckPoint);
            RequireClearDic = true;
        }
        public void Set(string iCheckPoint)
        {
            RequireClearDic = true;
            m_CheckPoint = iCheckPoint;
            //ApplyToServer().Forget();
        }
        public async UniTask ApplyToServer()//CancellationToken? iCancellationToken = null
        {
            string aCheckPoint = m_CheckPoint;
            var aCTS = s_CTS.Create();
            try
            {
                await SetCheckPointAsync(aCheckPoint, aCTS.Token);
            }
            catch(System.Exception e)
            {
                Debug.LogWarning($"ApplyToServer CheckPoint:{aCheckPoint},Exception:{e.Message}");
            }
            s_CTS.TryCancel(aCTS);

        }
        private static async UniTask SetCheckPointAsync(string iCheckPoint, CancellationToken iCancellationToken)
        {
            Debug.Log($"SetCheckPointAsync Start, CheckPoint:{iCheckPoint}");

            if (!SDU_Server.ServerReady)
            {
                await UniTask.WaitUntil(() => SDU_Server.ServerReady, cancellationToken: iCancellationToken);
            }
            Debug.Log($"SetCheckPointAsync SDU_Server.ServerReady, CheckPoint:{iCheckPoint}");
            iCancellationToken.ThrowIfCancellationRequested();
            using (var client = RunTimeData.SD_API.Client_Options)
            {
                JsonData aJson = new JsonData();

                aJson["sd_model_checkpoint"] = iCheckPoint;
                string aResult = await client.SendAsyncUniTask(iCancellationToken, aJson.ToJson());
                Debug.Log($"SetCheckPointAsync Done, CheckPoint:{iCheckPoint},aResult:{aResult}");
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
                    RunTimeData.WebUISetting.RefreshCheckpoints().Forget();
                }

                GUILayout.Label(iFieldName, UCL.Core.UI.UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));

                var aNames = RunTimeData.WebUISetting.m_ModelNames;
                if (!aNames.IsNullOrEmpty())
                {
                    var aCheckPoint = UCL_GUILayout.PopupAuto(m_CheckPoint, aNames, iDataDic, "Selected Model", 8);
                    if(aCheckPoint != m_CheckPoint) { 
                        Set(aCheckPoint);
                    }
                }

                if (GUILayout.Button("Open Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.InstallSetting.OpenFolder(FolderEnum.CheckPoints);
                }
            }
            return this;
        }
    }
}