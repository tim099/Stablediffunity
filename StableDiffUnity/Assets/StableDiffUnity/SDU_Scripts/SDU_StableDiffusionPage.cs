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
        public static string ServerUrl => RunTimeData.Ins.m_WebURL;

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
        
        public static string DefaultConfigFilePath => Path.Combine(InstallSetting.DefaultInstallRoot, "Configs", "StableDiffusion.json");
        static public SDU_StableDiffusionPage Create() => UCL_EditorPage.Create<SDU_StableDiffusionPage>();

        static public StableDiffusionAPI SD_API => RunTimeData.Ins.m_APISetting.m_StableDiffusionAPI;
        static public ControlNetAPI ControlNet_API => RunTimeData.Ins.m_APISetting.m_ControlNetAPI;
        

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
        List<Texture2D> m_Textures = new List<Texture2D>();
        ~SDU_StableDiffusionPage()
        {
            ClearTextures();
            RunTimeData.SaveRunTimeData();
        }
        public override void Init(UCL_GUIPageController iGUIPageController)
        {
            base.Init(iGUIPageController);
            RunTimeData.LoadRunTimeData();
            RunTimeData.Ins.m_AutoGenMode = GenMode.None;
        }
        void ClearTextures()
        {
            if (m_Textures.IsNullOrEmpty()) return;
            foreach(var aTexture in m_Textures)
            {
                GameObject.DestroyImmediate(aTexture);
            }
            m_Textures.Clear();
        }
        public override void OnClose()
        {
            ClearTextures();
            RunTimeData.SaveRunTimeData();
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
                    RunTimeData.Ins.m_WebUISetting.m_Models.Clear();
                    RunTimeData.Ins.m_WebUISetting.m_ModelNames.Clear();
                    //Debug.LogWarning($"responses:{responses.ToJsonBeautify()}");
                    foreach (JsonData aModelJson in responses)
                    {
                        var aModel = JsonConvert.LoadDataFromJson<SDU_WebUIClient.Get.SdApi.V1.SdModels.Responses>(aModelJson);
                        RunTimeData.Ins.m_WebUISetting.m_Models.Add(aModel);
                        RunTimeData.Ins.m_WebUISetting.m_ModelNames.Add(aModel.model_name);
                        //Debug.LogWarning($"model_name:{aModel.model_name}");
                    }
                    Debug.LogWarning($"ModelNames:{RunTimeData.Ins.m_WebUISetting.m_ModelNames.ConcatString()}");
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
                    RunTimeData.Ins.m_WebUISetting.m_CmdFlags = JsonConvert.LoadDataFromJson<SDU_WebUIClient.Get.SdApi.V1.CmdFlags.Responses>(responses);

                    var aLoraDir = RunTimeData.Ins.m_WebUISetting.m_CmdFlags.lora_dir;
                    if (Directory.Exists(aLoraDir))
                    {
                        var aLoras = Directory.GetFiles(aLoraDir, "*", SearchOption.AllDirectories);
                        RunTimeData.Ins.m_WebUISetting.m_LoraNames.Clear();
                        foreach (var aLora in aLoras)
                        {
                            if (!aLora.Contains(".txt") && !aLora.Contains(".png"))
                            {
                                RunTimeData.Ins.m_WebUISetting.m_LoraNames.Add(Path.GetFileNameWithoutExtension(aLora));
                            }
                        }
                    }
                    Debug.LogWarning($"_modelNamesForLora:{RunTimeData.Ins.m_WebUISetting.m_LoraNames.ConcatString()}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            try
            {
                using (var client = ControlNet_API.Client_ModelLists)
                {
                    var responses = await client.SendWebRequestAsync();
                    if (responses.Contains("model_list"))
                    {
                        //Data.m_WebUISettings.m_ControlNetSettings.m_ModelList.Clear();
                        RunTimeData.Ins.m_WebUISetting.m_ControlNetData.m_ModelList = JsonConvert.LoadDataFromJson<List<string>>(responses["model_list"]);
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
            string aPath = Path.Combine(RunTimeData.Ins.m_InstallSetting.OutputPath, aDate.ToString("MM_dd_yyyy"));
            if (!Directory.Exists(aPath))
            {
                UCL.Core.FileLib.Lib.CreateDirectory(aPath);
            }
            string aFileName = $"{RunTimeData.Ins.m_OutPutFileID.ToString()}_{System.DateTime.Now.ToString("HHmmssff")}";
            RunTimeData.Ins.m_OutPutFileID = RunTimeData.Ins.m_OutPutFileID + 1;
            return Tuple.Create(aPath, aFileName);
        }
        private async System.Threading.Tasks.ValueTask GenerateImage(Tex2ImgSetting iSetting)
        {
            RunTimeData.SaveRunTimeData();
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

                    aJson["sd_model_checkpoint"] = RunTimeData.Ins.m_Tex2ImgSettings.m_SelectedModel;
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
                    //aJson["denoising_strength"] = iSetting.m_DenoisingStrength;
                    var aControlNetSettings = RunTimeData.Ins.m_Tex2ImgSettings.m_ControlNetSettings;
                    if (aControlNetSettings.m_EnableControlNet)
                    {
                        JsonData aAlwayson = new JsonData();
                        aJson["alwayson_scripts"] = aAlwayson;
                        {
                            JsonData aControlnet = RunTimeData.Ins.m_Tex2ImgSettings.m_ControlNetSettings.GetConfigJson();//new JsonData();
                            if(aControlnet != null)
                            {
                                aAlwayson["controlnet"] = aControlnet;
                            }
                        }
                    }
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
                    RunTimeData.Ins.m_OutPutFileID = RunTimeData.Ins.m_OutPutFileID + 1;

                    var aFileTasks = new List<Task>();
                    var aImages = aResultJson["images"];
                    
                    Debug.LogWarning($"aImages.Count:{aImages.Count}");
                    ClearTextures();
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
                        m_Textures.Add(aTexture);
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
                        m_GenMode = RunTimeData.Ins.m_AutoGenMode;
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
            foreach(var aTexture in m_Textures)
            {
                var aSize = SDU_Util.GetTextureSize(512, aTexture);
                GUILayout.Box(aTexture, GUILayout.Width(aSize.x), GUILayout.Height(aSize.y));
            }
            GUILayout.EndHorizontal();

            using(var aScope = new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.SaveRunTimeData();
                }
                if (File.Exists(ConfigFilePath))
                {
                    if (GUILayout.Button("Load", GUILayout.ExpandWidth(false)))
                    {
                        RunTimeData.ReloadRunTimeData();
                    }
                }

                var aConfigFilePath = ConfigFilePath;
                var aNewConfigFilePath = UCL_GUILayout.TextField("ConfigFilePath", aConfigFilePath);
                if (aNewConfigFilePath != aConfigFilePath)
                {
                    ConfigFilePath = aNewConfigFilePath;
                }
            }

            UCL.Core.UI.UCL_GUILayout.DrawObjectData(RunTimeData.Ins, m_Dic.GetSubDic("RunTimeData"), "Configs", false);
            
            RunTimeData.Ins.m_Tex2ImgSettings.OnGUI("Tex2Img", m_Dic.GetSubDic("Tex2ImgSettings"));
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
                                GenerateImage(RunTimeData.Ins.m_Tex2ImgSettings).Forget();
                            });
                            break;
                        }
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
            var aPythonRoot = CheckInstall(RunTimeData.Ins.m_InstallSetting.PythonInstallRoot, RunTimeData.Ins.m_InstallSetting.PythonZipPath, "Python");
            var aEnvInstallRoot = CheckInstall(RunTimeData.Ins.m_InstallSetting.EnvInstallRoot, RunTimeData.Ins.m_InstallSetting.EnvZipPath, "Env");
            var aWebUIRoot = CheckInstall(RunTimeData.Ins.m_InstallSetting.WebUIInstallRoot, RunTimeData.Ins.m_InstallSetting.WebUIZipPath, "WebUI");
            File.WriteAllText(RunTimeData.Ins.m_InstallSetting.PythonInstallPathFilePath, aPythonRoot);
            File.WriteAllText(RunTimeData.Ins.m_InstallSetting.WebUIInstallPathFilePath, aWebUIRoot);
            File.WriteAllText(RunTimeData.Ins.m_InstallSetting.CommandlindArgsFilePath, RunTimeData.Ins.m_InstallSetting.CommandlindArgs);

            var aPythonExePath = RunTimeData.Ins.m_InstallSetting.PythonExePath;//System.IO.Path.Combine(aEnvInstallRoot, Data.PythonExePath);
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
            string aRunPythonPath = RunTimeData.Ins.m_InstallSetting.RunPythonPath;
            string aBatPath = RunTimeData.Ins.m_InstallSetting.RunBatPath;//System.IO.Path.Combine(aEnvInstallRoot, Data.WebUIScriptBatPath);
            UnityEngine.Debug.LogWarning($"RunPythonPath:{aRunPythonPath},BatPath:{aBatPath}");

            aProcess.StartInfo.FileName = aPythonExePath;
            aProcess.StartInfo.Arguments = $"{aRunPythonPath} {aBatPath}";
            aProcess.StartInfo.Verb = string.Empty;

            aProcess.StartInfo.CreateNoWindow = false;
            aProcess.StartInfo.RedirectStandardOutput = RunTimeData.Ins.m_RedirectStandardOutput;

            if (RunTimeData.Ins.m_RedirectStandardOutput)
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

            if (RunTimeData.Ins.m_RedirectStandardOutput)
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
                    if (RunTimeData.Ins.m_AutoOpenWeb)
                    {
                        System.Diagnostics.Process.Start(RunTimeData.Ins.m_WebURL);
                        UnityEngine.Debug.LogWarning($"Open WebURL:{RunTimeData.Ins.m_WebURL}");
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
                RunTimeData.Ins.m_WebURL = Regex.Replace(aURL, @"\s", string.Empty);
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