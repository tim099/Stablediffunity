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
                if (PlayerPrefs.HasKey(ConfigFilePathKey))//Old Path
                {
                    try
                    {
                        string aOldPath = PlayerPrefs.GetString(ConfigFilePathKey);

                        if (File.Exists(aOldPath))
                        {
                            //File.Move(aOldPath, value);
                            File.Delete(aOldPath);
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                PlayerPrefs.SetString(ConfigFilePathKey, value);

            }
        }
        public static string DefaultInstallRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
            "StableDiffUnity_V1");
        public static string DefaultConfigFilePath => Path.Combine(DefaultInstallRoot, "StableDiffusion.json");
        static public SDU_StableDiffusionPage Create() => UCL_EditorPage.Create<SDU_StableDiffusionPage>();
        [System.Serializable]
        public class InstallSetting
        {
            public string EnvInstallRoot = Path.Combine(DefaultInstallRoot, "Env");
            public string WebUIInstallRoot = Path.Combine(DefaultInstallRoot, "WebUI");
            public string PythonInstallRoot = Path.Combine(DefaultInstallRoot, "Python");
            public string CommandlindArgs = "--api --xformers";

            public string m_EnvZipFileName = "Env_V1.0.zip";
            public string m_WebUIZipFileName = "WebUI1.2.1.zip";
            public string m_PythonZipFileName = "Python_310.zip";

            public string InstallStableDiffusionRoot => Path.Combine(Application.streamingAssetsPath ,"InstallStableDiffUnity");
            public string EnvPath => Path.Combine(InstallStableDiffusionRoot , m_EnvZipFileName);
            public string WebUIPath => Path.Combine(InstallStableDiffusionRoot , m_WebUIZipFileName);
            public string PythonPath => Path.Combine(InstallStableDiffusionRoot, m_PythonZipFileName);
            public string RunPythonPath => Path.Combine(EnvInstallRoot, "run.py");

            //public string ConfigFilePath => Path.Combine(EnvInstallRoot, "Config.json");
            public string PythonInstallPathFilePath => Path.Combine(EnvInstallRoot, "PythonRoot.txt");
            public string WebUIInstallPathFilePath => Path.Combine(EnvInstallRoot, "WebUIInstallPath.txt");
            public string CommandlindArgsFilePath => Path.Combine(EnvInstallRoot, "CommandlindArgs.txt");

            /// <summary>
            /// inside Env folder
            /// </summary>
            public string RunBatPath => Path.Combine(EnvInstallRoot, "run.bat");
            /// <summary>
            /// PythonExePath inside Env folder
            /// </summary>
            public string PythonExePath => Path.Combine(PythonInstallRoot, @"python.exe");
        }

        [System.Serializable]
        public class StableDiffusionAPI
        {
            public string m_ApiSdModels = "/sdapi/v1/sd-models";
            public string m_ApiCmdFlags = "/sdapi/v1/cmd-flags";
            public string m_ApiOptions = "/sdapi/v1/options";
            public string m_ApiTxt2img = "/sdapi/v1/txt2img";
            public string m_ApiPngInfo = "/sdapi/v1/png-info";

            public string URL_SdModels => ServerUrl + m_ApiSdModels;
            public string URL_CmdFlags => ServerUrl + m_ApiCmdFlags;
            public string URL_Options => ServerUrl + m_ApiOptions;
            public string URL_Txt2img => ServerUrl + m_ApiTxt2img;
            public string URL_PngInfo => ServerUrl + m_ApiPngInfo;
        }
        [System.Serializable]
        public class WebUISettings
        {
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
        public class Tex2ImgSettings
        {
            public List<string> GetAllModelNames() => Data.m_WebUISettings.m_ModelNames;
            [UCL.Core.PA.UCL_List("GetAllModelNames")] public string m_SelectedModel;
            public List<string> GetAllSamplerNames() => Data.m_WebUISettings.m_Samplers;
            [UCL.Core.PA.UCL_List("GetAllSamplerNames")] public string m_SelectedSampler;

            public List<string> GetAllLoraNames() => Data.m_WebUISettings.m_LoraNames;
            [UCL.Core.PA.UCL_List("GetAllLoraNames")] public string m_SelectedLoraModel;


            public string m_Prompt;
            public string m_NegativePrompt;
            public int m_Width = 512;
            public int m_Height = 512;
            public int m_Steps = 20;
            public float m_CfgScale = 7;
            public long m_Seed = -1;

            [UCL.Core.ATTR.UCL_FunctionButton]
            public void AddLora()
            {
                m_Prompt += $"<lora:{m_SelectedLoraModel}:1>";
            }
        }
        [System.Serializable]
        public class Tex2ImgResults
        {
            public Dictionary<string, string> m_Infos = new Dictionary<string, string>();
        }

        [System.Serializable]
        public class RunTimeData : UCL.Core.JsonLib.UnityJsonSerializable
        {
            public InstallSetting m_InstallSetting = new InstallSetting();
            public StableDiffusionAPI m_StableDiffusionAPI = new StableDiffusionAPI();
            public WebUISettings m_WebUISettings = new WebUISettings();
            public Tex2ImgSettings m_Tex2ImgSettings = new Tex2ImgSettings();
            public Tex2ImgResults m_Tex2ImgResults = new Tex2ImgResults();
            public bool m_RedirectStandardOutput = false;
            public bool m_AutoOpenWeb = true;
            public string m_WebURL = "http://127.0.0.1:7860";
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
            string aJsonStr = UCL.Core.JsonLib.JsonConvert.SaveDataToJsonUnityVer(s_RunTimeData).ToJsonBeautify();
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
        }
        public override void OnClose()
        {
            SaveRunTimeData();
            SDU_WebUIStatus.Ins.Close();
            base.OnClose();
        }
        public async System.Threading.Tasks.ValueTask RefreshModels()
        {
            using (var client = new SDU_WebUIClient.Get.SdApi.V1.SdModels(Data.m_StableDiffusionAPI.URL_SdModels))
            {
                var responses = await client.SendRequestAsync();
                Data.m_WebUISettings.m_Models.Clear();
                Data.m_WebUISettings.m_ModelNames.Clear();
                foreach (var aModel in responses)
                {
                    Data.m_WebUISettings.m_Models.Add(aModel);
                    Data.m_WebUISettings.m_ModelNames.Add(aModel.model_name);
                    Debug.LogWarning($"model_name:{aModel.model_name}");
                }
                //Data.m_WebUISettings.m_ModelNames = responses.Select(x => x.model_name).ToList();
                Debug.LogWarning($"ModelNames:{Data.m_WebUISettings.m_ModelNames.ConcatString()}");
            }

            using (var client = new SDU_WebUIClient.Get.SdApi.V1.CmdFlags
                (Data.m_StableDiffusionAPI.URL_CmdFlags))
            {
                SDU_WebUIClient.Get.SdApi.V1.CmdFlags.Responses responses = await client.SendRequestAsync();
                Data.m_WebUISettings.m_CmdFlags = responses;
                Debug.LogWarning($"responses.lora_dir:{responses.lora_dir}");
                Data.m_WebUISettings.m_LoraNames = Directory.GetFiles(responses.lora_dir, "*", SearchOption.AllDirectories)
                    .Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
                Debug.LogWarning($"_modelNamesForLora:{Data.m_WebUISettings.m_LoraNames.ConcatString()}");
            }
        }
        private async System.Threading.Tasks.ValueTask GenerateImage()
        {
            if (m_GeneratingImage) return;
            m_GeneratingImage = true;
            m_ProgressStr = "Generating Image 0%";
            try
            {
                Debug.Log("Image generating started.");

                using (var client = new SDU_WebUIClient.Post.SdApi.V1.Options(Data.m_StableDiffusionAPI.URL_Options))
                {
                    var body = client.GetRequestBody();

                    body.sd_model_checkpoint = Data.m_Tex2ImgSettings.m_SelectedModel; //ModelsList[_selectedModel];

                    var responses = await client.SendRequestAsync(body);
                }
                using (var client = new SDU_WebUIClient.Post.SdApi.V1.Txt2Img(Data.m_StableDiffusionAPI.URL_Txt2img))
                {
                    var aSetting = Data.m_Tex2ImgSettings;
                    var body = new SDU_WebUIClient.Post.SdApi.V1.Txt2Img.RequestBody();//client.GetRequestBody(_stableDiffusionWebUISettings);
                    body.sampler_index = aSetting.m_SelectedSampler;
                    body.prompt = aSetting.m_Prompt;
                    body.steps = aSetting.m_Steps;
                    body.negative_prompt = aSetting.m_NegativePrompt;
                    body.seed = aSetting.m_Seed;
                    body.cfg_scale = aSetting.m_CfgScale;
                    //body.denoising_strength = _denoisingStrength;
                    body.width = aSetting.m_Width;
                    body.height = aSetting.m_Height;
                    var responses = await client.SendRequestAsync(body);
                    var aImageBytes = responses.GetImage();
                    if (m_Texture != null)
                    {
                        GameObject.DestroyImmediate(m_Texture);
                    }
                    m_Texture = UCL.Core.TextureLib.Lib.CreateTexture(aImageBytes);
                    //m_Texture
                    using (var clientInfo = new SDU_WebUIClient.Post.SdApi.V1.PngInfo(Data.m_StableDiffusionAPI.URL_PngInfo))
                    {
                        var bodyInfo = clientInfo.GetRequestBody();
                        bodyInfo.SetImage(aImageBytes);

                        var responsesInfo = await clientInfo.SendRequestAsync(bodyInfo);

                        var dic = responsesInfo.Parse();
                        Data.m_Tex2ImgResults.m_Infos = dic;
                        Debug.LogWarning($"Seed:{dic.GetValueOrDefault("Seed")}");
                    }
                }
                //using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img(_stableDiffusionWebUISettings))
                //{
                //    var body = client.GetRequestBody(_stableDiffusionWebUISettings);

                //    body.prompt = _prompt;
                //    body.steps = _steps;
                //    body.negative_prompt = _negativePrompt;
                //    body.seed = _seed;
                //    body.cfg_scale = _cfgScale;
                //    //body.denoising_strength = _denoisingStrength;
                //    body.width = _width;
                //    body.height = _height;

                //    var responses = await client.SendRequestAsync(body);

                //    using (var clientInfo = new StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo(_stableDiffusionWebUISettings))
                //    {
                //        var bodyInfo = clientInfo.GetRequestBody();
                //        bodyInfo.SetImage(responses.GetImage());

                //        var responsesInfo = await clientInfo.SendRequestAsync(bodyInfo);

                //        var dic = responsesInfo.Parse();

                //        Debug.Log($"Seed:{dic.GetValueOrDefault("Seed")}");
                //    }

                //    var texture = ImagePorter.GenerateTexture(responses.GetImage());

                //    if (ImagePorter.SaveImage(texture, _exportType))
                //    {
                //        Debug.Log("Image generating completed.");
                //    }
                //    else
                //    {
                //        Debug.LogWarning("Faled to save generated image.");
                //    }

                //    Image image = GetComponent<Image>();
                //    if (image != null)
                //    {
                //        if (ImagePorter.LoadIntoImage(texture, image))
                //        {
                //            Debug.Log($"Image loaded in {image.name}.", image);
                //            return;
                //        }
                //    }

                //    RawImage rawImage = GetComponent<RawImage>();
                //    if (rawImage != null)
                //    {
                //        if (ImagePorter.LoadIntoImage(texture, rawImage))
                //        {
                //            Debug.Log($"Image loaded in {rawImage.name}.", rawImage);
                //            return;
                //        }
                //    }

                //    Renderer renderer = GetComponent<Renderer>();
                //    if (renderer != null)
                //    {
                //        if (ImagePorter.LoadIntoImage(texture, renderer))
                //        {
                //            Debug.Log($"Image loaded in {renderer.name}.", renderer);
                //            return;
                //        }
                //    }
                //}
            }
            finally
            {
                m_GeneratingImage = false;
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
                        if (GUILayout.Button("GenerateImage"))
                        {
                            GenerateImage().Forget();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(m_ProgressStr))
            {
                GUILayout.Label(m_ProgressStr, UCL_GUIStyle.LabelStyle);
            }
            if (m_Texture != null)
            {
                GUILayout.Box(m_Texture);
            }
            //if (GUILayout.Button("Open Web"))
            //{
            //    System.Diagnostics.Process.Start(Data.m_WebURL);
            //}
            var aConfigFilePath = ConfigFilePath;
            var aNewConfigFilePath = UCL_GUILayout.TextField("ConfigFilePath", aConfigFilePath);
            if(aNewConfigFilePath != aConfigFilePath)
            {
                ConfigFilePath = aNewConfigFilePath;
            }
            UCL.Core.UI.UCL_GUILayout.DrawObjectData(Data, m_Dic.GetSubDic("RunTimeData"), "Configs", true);
        }
        public void StartServer()
        {
            m_ServerReady = false;
            var aPythonRoot = CheckInstallPython();
            var aEnvInstallRoot = CheckInstallEnv();
            var aWebUIRoot = CheckInstallWebUI();
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
        private string CheckInstallPython()
        {
            return CheckInstall(Data.m_InstallSetting.PythonInstallRoot, Data.m_InstallSetting.PythonPath, "Python");
        }
        private string CheckInstallEnv()
        {
            return CheckInstall(Data.m_InstallSetting.EnvInstallRoot, Data.m_InstallSetting.EnvPath, "Env");
        }
        private string CheckInstallWebUI()
        {
            return CheckInstall(Data.m_InstallSetting.WebUIInstallRoot, Data.m_InstallSetting.WebUIPath, "WebUI");
        }
    }
}