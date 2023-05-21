/*
AutoHeader Test
to change the auto header please go to RCG_AutoHeader.cs
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core.UI;
using UnityEngine;
using System;
using System.Text;
using UCL.Core.EditorLib.Page;
using System.Text.RegularExpressions;
using UCL.Core.JsonLib;
using System.Linq;
using System.Threading.Tasks;
//using System.Diagnostics;

namespace SDU
{
    //[UCL.Core.ATTR.RequiresConstantRepaint]
    public class SDU_StableDiffusionPage : UCL_EditorPage
    {
        
        const string ConfigFilePathKey = "StableDiffusionPage.ConfigFilePath";

        #region static
        public static string ServerUrl => Data.m_WebURL;

        public static string ConfigFilePath
        {
            get
            {
                if (!PlayerPrefs.HasKey(ConfigFilePathKey))
                {
                    PlayerPrefs.SetString(ConfigFilePathKey, DefaultConfigFilePath);
                }
                return PlayerPrefs.GetString(ConfigFilePathKey);
            }
            set
            {
                PlayerPrefs.SetString(ConfigFilePathKey, value);
            }
        }
        
        public static string DefaultConfigFilePath => Path.Combine(InstallSetting.DefaultInstallRoot, "StableDiffusion.json");
        static public SDU_StableDiffusionPage Create() => UCL_EditorPage.Create<SDU_StableDiffusionPage>();

        [System.Serializable]
        public class ControlNetSettings
        {
            public List<string> m_ModelList = new List<string>();
        }
        [System.Serializable]
        public class WebUISettings
        {
            public ControlNetSettings m_ControlNetSettings = new ControlNetSettings();

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
        }

        [UCL.Core.ATTR.EnableUCLEditor]
        [System.Serializable]
        public class ResolutionSettings
        {
            public int m_Width = 1920;
            public int m_Height = 1080;
            public FullScreenMode m_FullScreenMode = FullScreenMode.Windowed;

            [UCL.Core.ATTR.UCL_FunctionButton]
            public void ApplyResolutionSetting()
            {
                Screen.SetResolution(m_Width, m_Height, m_FullScreenMode);
            }
        }
        //Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreenMode);


        [System.Serializable]
        public class Tex2ImgResults
        {
            public Dictionary<string, string> m_Infos = new Dictionary<string, string>();
        }
        public enum GenMode
        {
            None,
            GenTex2Img,
            //GenDepthTex2Img,
        }
        [System.Serializable]
        public class RunTimeData : UCL.Core.JsonLib.UnityJsonSerializable
        {
            public InstallSetting m_InstallSetting = new InstallSetting();
            public ResolutionSettings m_ResolutionSettings = new ResolutionSettings();
            public APISetting m_APISetting = new APISetting();
            public WebUISettings m_WebUISettings = new WebUISettings();


            [UCL.Core.ATTR.UCL_HideOnGUI] public Tex2ImgSettings m_Tex2ImgSettings = new Tex2ImgSettings();
            [UCL.Core.ATTR.UCL_HideOnGUI] public Tex2ImgResults m_Tex2ImgResults = new Tex2ImgResults();

            public bool m_RedirectStandardOutput = false;
            public bool m_AutoOpenWeb = true;
            public GenMode m_AutoGenMode = GenMode.None;
            public string m_WebURL = "http://127.0.0.1:7860";
            [HideInInspector] public int m_OutPutFileID = 0;
        }
        static public RunTimeData Data
        {
            get
            {
                if (s_RunTimeData == null)
                {
                    s_RunTimeData = LoadRunTimeData();
                }
                return s_RunTimeData;
            }
        }
        static RunTimeData s_RunTimeData = null;
        static public StableDiffusionAPI SD_API => Data.m_APISetting.m_StableDiffusionAPI;
        static public ControlNetAPI ControlNet_API => Data.m_APISetting.m_ControlNetAPI;
        
        static RunTimeData LoadRunTimeData()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    string aJsonStr = File.ReadAllText(ConfigFilePath);//PlayerPrefs.GetString(RunTimeDataKey);
                    JsonData aJson = JsonData.ParseJson(aJsonStr);
                    var aRunTimeData = JsonConvert.LoadDataFromJsonUnityVer<RunTimeData>(aJson);
                    if (aRunTimeData != null) return aRunTimeData;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return new RunTimeData();
        }
        static void SaveRunTimeData()
        {
            string aJsonStr = UCL.Core.JsonLib.JsonConvert.SaveDataToJsonUnityVer(Data).ToJsonBeautify();
            UCL.Core.FileLib.Lib.WriteAllText(ConfigFilePath, aJsonStr);
            //PlayerPrefs.SetString(RunTimeDataKey, aJsonStr);
        }
        #endregion
        public override bool IsWindow => true;
        public override string WindowName => $"StableDiffUnity GUI {SDU_EditorMenuPage.SDU_Version}";
        protected override bool ShowCloseButton => false;



        UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();
        int m_ProcessID = -1;
        //System.DateTime m_CheckProcessEndTime = DateTime.MinValue;
        GenMode m_GenMode = GenMode.None;
        bool m_StartGenerating = false;
        bool m_GeneratingImage = false;
        bool m_ServerReady = false;
        string m_ProgressStr = string.Empty;
        Texture2D m_Texture;
        ~SDU_StableDiffusionPage()
        {
            SaveRunTimeData();
        }
        public override void Init(UCL_GUIPageController iGUIPageController)
        {
            base.Init(iGUIPageController);
            LoadRunTimeData();
            Data.m_AutoGenMode = GenMode.None;
        }
        public override void OnClose()
        {
            SaveRunTimeData();
            SDU_WebUIStatus.Ins.Close();
            base.OnClose();
        }
        public async System.Threading.Tasks.ValueTask RefreshModels()
        {
            try
            {
                using (var client = new SDU_WebUIClient.SDU_WebRequest(SD_API.URL_SdModels, SDU_WebRequest.Method.Get))
                {
                    var responses = await client.SendWebRequestAsync();
                    Data.m_WebUISettings.m_Models.Clear();
                    Data.m_WebUISettings.m_ModelNames.Clear();
                    //Debug.LogWarning($"responses:{responses.ToJsonBeautify()}");
                    foreach (JsonData aModelJson in responses)
                    {
                        var aModel = JsonConvert.LoadDataFromJson<SDU_WebUIClient.Get.SdApi.V1.SdModels.Responses>(aModelJson);
                        Data.m_WebUISettings.m_Models.Add(aModel);
                        Data.m_WebUISettings.m_ModelNames.Add(aModel.model_name);
                        //Debug.LogWarning($"model_name:{aModel.model_name}");
                    }
                    Debug.LogWarning($"ModelNames:{Data.m_WebUISettings.m_ModelNames.ConcatString()}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            try
            {
                using (var client = new SDU_WebUIClient.SDU_WebRequest(SD_API.URL_CmdFlags, SDU_WebRequest.Method.Get))
                {
                    var responses = await client.SendWebRequestAsync();
                    Data.m_WebUISettings.m_CmdFlags = JsonConvert.LoadDataFromJson<SDU_WebUIClient.Get.SdApi.V1.CmdFlags.Responses>(responses);

                    var aLoraDir = Data.m_WebUISettings.m_CmdFlags.lora_dir;
                    if (Directory.Exists(aLoraDir))
                    {
                        var aLoras = Directory.GetFiles(aLoraDir, "*", SearchOption.AllDirectories);
                        Data.m_WebUISettings.m_LoraNames.Clear();
                        foreach (var aLora in aLoras)
                        {
                            if (!aLora.Contains(".txt") && !aLora.Contains(".png"))
                            {
                                Data.m_WebUISettings.m_LoraNames.Add(Path.GetFileNameWithoutExtension(aLora));
                            }
                        }
                    }
                    Debug.LogWarning($"_modelNamesForLora:{Data.m_WebUISettings.m_LoraNames.ConcatString()}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            try
            {
                //string aURL = SD_API.URL_CmdFlags;
                //Debug.LogError($"aURL:{aURL}");
                using (var client = new SDU_WebUIClient.SDU_WebRequest(ControlNet_API.URL_ModelLists, SDU_WebRequest.Method.Get))
                {
                    
                    var responses = await client.SendWebRequestAsync();
                    //{"model_list":["control_sd15_canny [fef5e48e]","control_sd15_depth [fef5e48e]","control_sd15_normal [fef5e48e]","control_sd15_openpose [fef5e48e]","controlnetPreTrained_cannyV10 [e3fe7712]"]}
                    if (responses.Contains("model_list"))
                    {
                        //Data.m_WebUISettings.m_ControlNetSettings.m_ModelList.Clear();
                        Data.m_WebUISettings.m_ControlNetSettings.m_ModelList = JsonConvert.LoadDataFromJson<List<string>>(responses["model_list"]);
                    }
                    
                    //Debug.LogWarning($"ControlNet_API responses:{responses.ToJson()}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

        }
        public static Tuple<string ,string> GetSaveImagePath()
        {
            var aDate = DateTime.Now;
            string aPath = Path.Combine(Data.m_InstallSetting.OutputPath, aDate.ToString("MM_dd_yyyy"));
            if (!Directory.Exists(aPath))
            {
                UCL.Core.FileLib.Lib.CreateDirectory(aPath);
            }
            string aFileName = $"{Data.m_OutPutFileID.ToString()}_{System.DateTime.Now.ToString("HHmmssff")}";
            Data.m_OutPutFileID = Data.m_OutPutFileID + 1;
            return Tuple.Create(aPath, aFileName);
        }
        private async System.Threading.Tasks.ValueTask GenerateImage(Tex2ImgSettings iSetting)
        {
            SaveRunTimeData();
            m_GenMode = GenMode.None;
            if (m_GeneratingImage) return;
            m_GeneratingImage = true;
            m_ProgressStr = "Generating Image 0%";
            try
            {
                //Debug.LogWarning($"Image generating started. DepthMode:{iDepthTexture != null}");
                //if(m_DepthTexture == null)
                //{
                //    m_DepthTexture = iDepthTexture;
                //}
                using (var client = new SDU_WebUIClient.SDU_WebRequest(SD_API.URL_Options, SDU_WebRequest.Method.Post))
                {
                    JsonData aJson = new JsonData();

                    aJson["sd_model_checkpoint"] = Data.m_Tex2ImgSettings.m_SelectedModel;
                    string aJsonStr = aJson.ToJson();
                    var aResultJson = await client.SendWebRequestAsyncString(aJsonStr);
                    Debug.LogWarning($"aResultJson:{aResultJson}");
                }
                using (var client = new SDU_WebUIClient.SDU_WebRequest(SD_API.URL_Txt2img, SDU_WebRequest.Method.Post))//Post.ControlNet.Txt2Img
                {
                    JsonData aJson = new JsonData();

                    aJson["sampler_index"] = iSetting.m_SelectedSampler;
                    aJson["prompt"] = iSetting.m_Prompt;
                    aJson["steps"] = iSetting.m_Steps;
                    aJson["negative_prompt"] = iSetting.m_NegativePrompt;
                    aJson["seed"] = iSetting.m_Seed;
                    aJson["cfg_scale"] = iSetting.m_CfgScale;
                    aJson["width"] = iSetting.m_Width;
                    aJson["height"] = iSetting.m_Height;

                    //if (iDepthTexture != null)
                    {
                        //byte[] iDepth = iDepthTexture.EncodeToPNG();
                        JsonData aAlwayson = new JsonData();
                        aJson["alwayson_scripts"] = aAlwayson;

                        {
                            JsonData aControlnet = Data.m_Tex2ImgSettings.m_ControlNetSettings.GetConfigJson();//new JsonData();
                            if(aControlnet != null)
                            {
                                aAlwayson["controlnet"] = aControlnet;
                            }
                            
                            //{
                            //    JsonData aArgs = new JsonData();
                            //    aControlnet["args"] = aArgs;
                            //    {
                            //        var aSetting = Data.m_Tex2ImgSettings.m_ControlNetSettings;
                            //        JsonData aArg1 = new JsonData();
                            //        //aArg1["module"] = "depth";
                            //        aArg1["input_image"] = Convert.ToBase64String(iDepth);
                            //        aArg1["model"] = aSetting.m_SelectedModel ;//"control_sd15_depth"
                            //        aArgs.Add(aArg1);
                            //    }
                            //}
                        }
                    }
                    ////body.denoising_strength = _denoisingStrength;

                    string aJsonStr = aJson.ToJson();
                    //Debug.LogWarning(aJsonStr);
                    
                    //GUIUtility.systemCopyBuffer = aJsonStr;
                    var aResultJson = await client.SendWebRequestAsync(aJsonStr);
                    Debug.LogWarning("Image generating Ended");
                    if(aResultJson == null)
                    {
                        throw new Exception("SendWebRequestAsync, aResultJson == null");
                    }
                    if (!aResultJson.Contains("images"))
                    {
                        throw new Exception($"SendWebRequestAsync, !responses.Contains(\"images\"),aResultJson:{aResultJson.ToJsonBeautify()}");
                    }
                    var aSavePath = GetSaveImagePath();
                    string aPath = aSavePath.Item1;
                    string aFileName = aSavePath.Item2;
                    Data.m_OutPutFileID = Data.m_OutPutFileID + 1;

                    var aFileTasks = new List<Task>();
                    var aImages = aResultJson["images"];
                    
                    Debug.LogWarning($"aImages.Count:{aImages.Count}");
                    
                    for (int i = 0; i < aImages.Count; i++)
                    {
                        var aImageStr = aImages[i].GetString();
                        var aSplitStr = aImageStr.Split(",");
                        foreach(var aSplit in aSplitStr)
                        {
                            Debug.LogWarning($"aSplit:{aSplit}");
                        }
                        
                        var aImageBytes = Convert.FromBase64String(aSplitStr[0]);
                        var aTexture = UCL.Core.TextureLib.Lib.CreateTexture(aImageBytes);


                        string aFilePath = Path.Combine(aPath, $"{aFileName}_{i}.png"); // M HH:mm:ss
                        Debug.Log($"aPath:{aPath},aFilePath:{aFilePath}");

                        aFileTasks.Add(File.WriteAllBytesAsync(aFilePath, aTexture.EncodeToPNG()));
                        if (i == 0)
                        {
                            if (m_Texture != null)
                            {
                                GameObject.DestroyImmediate(m_Texture);
                            }
                            m_Texture = aTexture;//UCL.Core.TextureLib.Lib.CreateTexture(aImageBytes);
                        }
                        else
                        {
                            GameObject.DestroyImmediate(aTexture);
                        }
                    }


                    //using (var clientInfo = new SDU_WebUIClient.Post.SdApi.V1.PngInfo(Data.m_StableDiffusionAPI.URL_PngInfo))
                    //{
                    //    var bodyInfo = clientInfo.GetRequestBody();
                    //    bodyInfo.SetImage(aImageBytes);

                    //    var responsesInfo = await clientInfo.SendRequestAsync(bodyInfo);

                    //    var dic = responsesInfo.Parse();
                    //    Data.m_Tex2ImgResults.m_Infos = dic;
                    //    Debug.LogWarning($"Seed:{dic.GetValueOrDefault("Seed")}");
                    //}
                }
            }
            finally
            {
                m_GeneratingImage = false;
                //if (m_DepthTexture != iDepthTexture)
                //{
                //    GameObject.DestroyImmediate(m_DepthTexture);
                //}
                //else
                //{
                //    m_DepthTexture = iDepthTexture;
                //}
                await Resources.UnloadUnusedAssets();
            }
        }
        protected override void ContentOnGUI()
        {
            GUILayout.Label($"StableDiffusion Time:{System.DateTime.Now.ToString("HH:mm:ss.ff")}", UCL_GUIStyle.LabelStyle);
            if(!SDU_ProcessList.ProcessStarted)
            {
                if (GUILayout.Button("StartServer"))
                {
                    //Witchpot.Editor.StableDiffusion.WebUISingleton.Start();
                    StartServer();
                }
            }
            else
            {
                if (GUILayout.Button("StopServer"))
                {
                    //Witchpot.Editor.StableDiffusion.WebUISingleton.Stop();
                    UnityEngine.Debug.Log($"Stop server. m_ProcessID:{m_ProcessID}");
                    if (!m_ServerReady)
                    {
                        SDU_ProcessList.CheckProcessEvent();
                    }
                    SDU_ProcessList.KillAllProcess();
                    m_ProcessID = -1;
                }
                if (m_ServerReady)
                {
                    if (GUILayout.Button("Refresh Models"))
                    {
                        RefreshModels().Forget();
                    }
                    if (!m_GeneratingImage)
                    {
                        m_GenMode = Data.m_AutoGenMode;
                        if (GUILayout.Button("GenerateImage"))
                        {
                            m_GenMode = GenMode.GenTex2Img;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(m_ProgressStr))
            {
                GUILayout.Label(m_ProgressStr, UCL_GUIStyle.LabelStyle);
            }
            GUILayout.BeginHorizontal();
            if (m_Texture != null)
            {
                GUILayout.Box(m_Texture, GUILayout.MaxHeight(256));
            }
            GUILayout.EndHorizontal();

            using(var aScope = new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
                {
                    SaveRunTimeData();
                }
                if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        s_RunTimeData = LoadRunTimeData();
                    }
                }
                var aConfigFilePath = ConfigFilePath;
                var aNewConfigFilePath = UCL_GUILayout.TextField("ConfigFilePath", aConfigFilePath);
                if (aNewConfigFilePath != aConfigFilePath)
                {
                    ConfigFilePath = aNewConfigFilePath;
                }
            }

            UCL.Core.UI.UCL_GUILayout.DrawObjectData(Data, m_Dic.GetSubDic("RunTimeData"), "Configs", false);
            
            Data.m_Tex2ImgSettings.OnGUI(m_Dic.GetSubDic("Tex2ImgSettings"));
            //m_Tex2ImgSettings
            if (!m_GeneratingImage && !m_StartGenerating)
            {
                switch (m_GenMode)
                {
                    case GenMode.GenTex2Img:
                        {
                            m_StartGenerating = true;
                            UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                            {
                                m_StartGenerating = false;
                                GenerateImage(Data.m_Tex2ImgSettings).Forget();
                            });
                            break;
                        }
                    //case GenMode.GenDepthTex2Img:
                    //    {
                    //        m_StartGenerating = true;
                    //        UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
                    //        {
                    //            m_StartGenerating = false;
                    //            var aCam = URP_Camera.CurCamera;
                    //            if (aCam != null)
                    //            {
                    //                var aSetting = Data.m_Tex2ImgSettings;

                    //                var aDepthTexture = aCam.CreateDepthImage(aSetting.m_Width, aSetting.m_Height);
                    //                GenerateImage(aSetting, aDepthTexture).Forget();
                    //            }
                    //        });

                    //        break;
                    //    }
                }
            }
            if (!UnityChan.IdleChanger.s_IdleChangers.IsNullOrEmpty())
            {
                GUILayout.Box($"Change Motion");
                for (int i = 0; i < UnityChan.IdleChanger.s_IdleChangers.Count; i++)
                {
                    var aIdleChanger = UnityChan.IdleChanger.s_IdleChangers[i];
                    
                    aIdleChanger.CustomOnGUI(i);
                }
            }
            //if (UnityChan.FaceUpdate.s_Ins != null)
            //{
            //    UnityChan.FaceUpdate.s_Ins.CustomOnGUI();
            //}

        }
        public void StartServer()
        {
            m_ServerReady = false;
            var aPythonRoot = CheckInstall(Data.m_InstallSetting.PythonInstallRoot, Data.m_InstallSetting.PythonZipPath, "Python");
            var aEnvInstallRoot = CheckInstall(Data.m_InstallSetting.EnvInstallRoot, Data.m_InstallSetting.EnvZipPath, "Env");
            var aWebUIRoot = CheckInstall(Data.m_InstallSetting.WebUIInstallRoot, Data.m_InstallSetting.WebUIZipPath, "WebUI");
            File.WriteAllText(Data.m_InstallSetting.PythonInstallPathFilePath, aPythonRoot);
            File.WriteAllText(Data.m_InstallSetting.WebUIInstallPathFilePath, aWebUIRoot);
            File.WriteAllText(Data.m_InstallSetting.CommandlindArgsFilePath, Data.m_InstallSetting.CommandlindArgs);

            var aPythonExePath = Data.m_InstallSetting.PythonExePath;//System.IO.Path.Combine(aEnvInstallRoot, Data.PythonExePath);
            UnityEngine.Debug.LogWarning($"PythonExePath:{aPythonExePath}");
            if (!System.IO.File.Exists(aPythonExePath))
            {
                Debug.LogError($"File not found, path:{aPythonExePath}");
                return;
            }

            if (SDU_ProcessList.RestoreEventsForListedProcess())
            {
                UnityEngine.Debug.LogWarning($"Server alredy started.");
                return;
            }
            SDU_ProcessList.PreCheckProcessEvent();
            var aProcess = new System.Diagnostics.Process();
            
            //string aBatPath = System.IO.Path.Combine(Data.RootPath, Data.WebUIScriptBatPath);
            string aRunPythonPath = Data.m_InstallSetting.RunPythonPath;
            string aBatPath = Data.m_InstallSetting.RunBatPath;//System.IO.Path.Combine(aEnvInstallRoot, Data.WebUIScriptBatPath);
            UnityEngine.Debug.LogWarning($"RunPythonPath:{aRunPythonPath},BatPath:{aBatPath}");

            aProcess.StartInfo.FileName = aPythonExePath;
            aProcess.StartInfo.Arguments = $"{aRunPythonPath} {aBatPath}";
            aProcess.StartInfo.Verb = string.Empty;

            aProcess.StartInfo.CreateNoWindow = false;
            aProcess.StartInfo.RedirectStandardOutput = Data.m_RedirectStandardOutput;

            if (Data.m_RedirectStandardOutput)
            {
                aProcess.StartInfo.RedirectStandardOutput = true;
                aProcess.StartInfo.UseShellExecute = false;
                SDU_ProcessList.Init(aProcess);
                SDU_ProcessList.s_OnOutputDataReceivedAct = OnOutputDataReceived;
            }
            else
            {
                aProcess.StartInfo.UseShellExecute = true;
                aProcess.StartInfo.RedirectStandardOutput = false;
            }

            aProcess.Start();

            if (Data.m_RedirectStandardOutput)
            {
                aProcess.BeginOutputReadLine();
            }

            SDU_ProcessList.AddProcessEvent(aProcess);
            m_ProcessID = aProcess.Id;
            UnityEngine.Debug.LogWarning($"Start server, m_ProcessID:{m_ProcessID}");

            SDU_WebUIStatus.Ins.SetServerStarted(true);
            SDU_WebUIStatus.Ins.ValidateConnectionContinuously(()=> {
                try
                {
                    m_ServerReady = true;
                    UnityEngine.Debug.LogWarning($"ValidateConnectionContinuously End");
                    SDU_ProcessList.CheckProcessEvent();
                    //aProcess.StandardOutput.ReadToEnd();
                    if (Data.m_AutoOpenWeb)
                    {
                        System.Diagnostics.Process.Start(Data.m_WebURL);
                        UnityEngine.Debug.LogWarning($"Open WebURL:{Data.m_WebURL}");
                    }
                    RefreshModels().Forget();
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }

            }).Forget();
        }
        private void OnOutputDataReceived(string iOutPut)
        {
            if(iOutPut.Contains("Total progress:"))
            {
                m_ProgressStr = iOutPut.Replace("Total progress:", string.Empty);
                UnityEngine.Debug.LogWarning($"Total progress:{m_ProgressStr}");
            }
            else if (iOutPut.Contains("Running on local URL:"))
            {
                string aURL = iOutPut.Replace("Running on local URL:", string.Empty);
                Data.m_WebURL = Regex.Replace(aURL, @"\s", string.Empty);
                //System.Diagnostics.Process.Start(Data.m_WebURL);
                //UnityEngine.Debug.LogWarning($"Open WebURL:{Data.m_WebURL}");
            }
            else
            {
                //UnityEngine.Debug.LogWarning($"DataReceived_:{iOutPut}");
            }
            UnityEngine.Debug.LogWarning($"DataReceived_:{iOutPut}");

        }
        private string CheckInstall(string iInstallRoot, string iZipAbsolutePath,string iInstallTarget)
        {
            if (Directory.Exists(iInstallRoot))//Install done
            {
                return iInstallRoot;
            }
            try
            {
                Debug.LogWarning($"Installing {iInstallTarget}");
                Debug.LogWarning($"zipAbsolutePath:{iZipAbsolutePath}");
                if (!File.Exists(iZipAbsolutePath))
                {
                    Debug.LogError($"ZipAbsolutePath:{iZipAbsolutePath},not found.");
                    return iInstallRoot;
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(iZipAbsolutePath, iInstallRoot, true);

                Debug.Log($"{iInstallTarget} installation finished");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            return iInstallRoot;
        }
    }
}