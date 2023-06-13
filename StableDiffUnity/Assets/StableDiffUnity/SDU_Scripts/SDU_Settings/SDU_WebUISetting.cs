using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
using UnityEngine;

namespace SDU
{
    [System.Serializable]
    public class WebUISetting : UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        [System.Serializable]
        public class SdModels : UCL.Core.UCLI_ShortName
        {
            public string title;
            public string model_name;
            public string hash;
            public string sha256;
            public string filename;
            public string config;

            public string GetShortName() => model_name;
        }
        [System.Serializable]
        public class SdVAE : UCL.Core.UCLI_ShortName
        {
            public string name;
            public string path;

            public string GetShortName() => name;
        }

        [System.Serializable]
        public class ControlNetData
        {
            public List<string> m_ModelList = new List<string>();
        }

        public ControlNetData m_ControlNetData = new ControlNetData();

        public List<string> m_ModelNames = new List<string>();
        public List<string> m_LoraNames = new List<string>();
        public List<SdVAE> m_SdVAEs = new List<SdVAE>();
        public List<string> m_Samplers = new List<string>
            {
                "Euler a",
                "Euler",
                "LMS",
                "Heun",
                "DPM2",
                "DPM2 a",
                "DPM++ 2S a",
                "DPM++ 2M",
                "DPM++ SDE",
                "DPM fast",
                "DPM adaptive",
                "LMS Karras",
                "DPM2 Karras",
                "DPM2 a Karras",
                "DPM++ 2S a Karras",
                "DPM++ 2M Karras",
                "DPM++ SDE Karras",
                "DDIM",
                "PLMS"
            };
        public List<SdModels> m_Models = new();
        public JsonData m_CmdFlags = new();

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {

            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("WebUISetting"), iFieldName, false);
            if (GUILayout.Button("Refresh Checkpoints", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
            {
                UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                {
                    RefreshCheckpoints().Forget();
                });
            }
            return this;
        }
        public async UniTask RefreshModels()
        {
            List<UniTask> aTasks = new List<UniTask>();
            aTasks.Add(RefreshCheckpoints());
            aTasks.Add(RefreshSamplers());
            aTasks.Add(RefreshLora());
            aTasks.Add(RefreshControlNetModels());
            aTasks.Add(RefreshVAEs());
            await UniTask.WhenAll(aTasks);
        }
        public async UniTask Refresh(FolderEnum iFolderEnum)
        {
            switch(iFolderEnum)
            {
                case FolderEnum.VAE:
                    {
                        await RefreshVAEs();
                        break;
                    }
                case FolderEnum.Lora:
                    {
                        await RefreshLora();
                        break;
                    }
                case FolderEnum.CheckPoints:
                    {
                        await RefreshCheckpoints();
                        break;
                    }
                case FolderEnum.ControlNetModel:
                    {
                        await RefreshControlNetModels();
                        break;
                    }
            }
        }
        public async UniTask RefreshVAEs()
        {
            try
            {
                using (var aClient = RunTimeData.Stablediffunity_API.Client_GetVAEs)
                {
                    var aResponsesStr = await aClient.SendWebRequestStringAsync();
                    Debug.LogWarning($"RefreshVAEs:{aResponsesStr}");
                    JsonData aJson = JsonData.ParseJson(aResponsesStr);
                    if (aJson.Contains("VAE"))
                    {
                        m_SdVAEs.Clear();
                        JsonConvert.LoadDataFromJson(m_SdVAEs, aJson["VAE"]);
                    }

                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        public async UniTask RefreshSamplers()
        {
            try
            {
                using (var aClient = RunTimeData.SD_API.Client_Samplers)
                {
                    var aResponses = await aClient.SendWebRequestAsync();
                    m_Samplers.Clear();
                    for(int i = 0; i < aResponses.Count; i++)
                    {
                        var aSampler = aResponses[i];
                        if (aSampler.Contains("name"))
                        {
                            m_Samplers.Add(aSampler["name"].GetString());
                        }
                    }
                    //m_CmdFlags = JsonConvert.LoadDataFromJson<SDU_WebUIClient.Get.SdApi.V1.CmdFlags.Responses>(aResponses);

                    Debug.LogWarning($"Samplers aResponses:{aResponses}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        public async UniTask RefreshCheckpoints()
        {
            try
            {
                using (var client = RunTimeData.SD_API.Client_RefreshCheckpoints)
                {
                    var responses = await client.SendWebRequestStringAsync();
                    //Debug.LogWarning($"Client_RefreshCheckpoints responses:{responses}");
                }
                using (var client = RunTimeData.SD_API.Client_SdModels)
                {
                    var responses = await client.SendWebRequestAsync();
                    m_Models.Clear();
                    m_ModelNames.Clear();
                    foreach (JsonData aModelJson in responses)
                    {
                        var aModel = JsonConvert.LoadDataFromJson<SdModels>(aModelJson);
                        m_Models.Add(aModel);
                        m_ModelNames.Add(aModel.model_name);
                    }
                    Debug.LogWarning($"ModelNames:{RunTimeData.Ins.m_WebUISetting.m_ModelNames.ConcatString()}");
                }
                //using(var aClient = RunTimeData.SD_API.Client_Docs)
                //{
                //    var responses = await aClient.SendWebRequestStringAsync();
                //    Debug.LogWarning($"Docs:{responses}");
                //}
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

        }
        public async UniTask RefreshLora()
        {
            try
            {
                using (var client = new SDU_WebUIClient.SDU_WebRequest(RunTimeData.SD_API.URL_CmdFlags, SDU_WebRequest.Method.Get))
                {
                    var responses = await client.SendWebRequestAsync();
                    m_CmdFlags = responses;
                    if (m_CmdFlags.Contains("lora_dir"))
                    {
                        string aLoraDir = m_CmdFlags["lora_dir"].GetString();
                        //var aLoraDir = m_CmdFlags.lora_dir;
                        if (Directory.Exists(aLoraDir))
                        {
                            var aLoras = Directory.GetFiles(aLoraDir, "*", SearchOption.TopDirectoryOnly);
                            m_LoraNames.Clear();
                            foreach (var aLora in aLoras)
                            {
                                if (!aLora.Contains(".txt") && !aLora.Contains(".png"))
                                {
                                    m_LoraNames.Add(Path.GetFileNameWithoutExtension(aLora));
                                }
                            }
                        }
                        Debug.LogWarning($"_modelNamesForLora:{m_LoraNames.ConcatString()}");
                    }

                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        public async UniTask RefreshControlNetModels()
        {
            try
            {
                using (var aClient = RunTimeData.ControlNet_API.Client_ModelLists)
                {
                    var aResponses = await aClient.SendWebRequestAsync();
                    const string Key = "model_list";
                    if (aResponses.Contains(Key))
                    {
                        RunTimeData.Ins.m_WebUISetting.m_ControlNetData.m_ModelList = 
                            JsonConvert.LoadDataFromJson<List<string>>(aResponses[Key]);
                    }
                    //Debug.LogWarning($"ControlNet_API responses:{responses.ToJson()}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}