using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
using UnityEngine;

namespace SDU
{
    [System.Serializable]
    public class WebUISetting : UCL.Core.UI.UCLI_FieldOnGUI
    {
        [System.Serializable]
        public class ControlNetData
        {
            public List<string> m_ModelList = new List<string>();
        }

        public ControlNetData m_ControlNetData = new ControlNetData();

        public List<string> m_ModelNames = new List<string>();
        public List<string> m_LoraNames = new List<string>();
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
        public List<SDU_WebUIClient.Get.SdApi.V1.SdModels.Responses> m_Models = new();
        public SDU_WebUIClient.Get.SdApi.V1.CmdFlags.Responses m_CmdFlags = new();

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {

            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("WebUISetting"), iFieldName, false);
            if (GUILayout.Button("Refresh Checkpoints"))
            {
                UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                {
                    RefreshCheckpoints().Forget();
                });
            }
            return this;
        }
        public async System.Threading.Tasks.Task RefreshSamplers()
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
        public async System.Threading.Tasks.Task RefreshCheckpoints()
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
                        var aModel = JsonConvert.LoadDataFromJson<SDU_WebUIClient.Get.SdApi.V1.SdModels.Responses>(aModelJson);
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
        public async System.Threading.Tasks.Task RefreshLora()
        {
            try
            {
                using (var client = new SDU_WebUIClient.SDU_WebRequest(RunTimeData.SD_API.URL_CmdFlags, SDU_WebRequest.Method.Get))
                {
                    var responses = await client.SendWebRequestAsync();
                    m_CmdFlags = JsonConvert.LoadDataFromJson<SDU_WebUIClient.Get.SdApi.V1.CmdFlags.Responses>(responses);

                    var aLoraDir = m_CmdFlags.lora_dir;
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
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        public async System.Threading.Tasks.Task RefreshControlNetModels()
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