using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UCL.Core.JsonLib;
using UnityEngine;


namespace SDU
{
    public static class SDU_ImageGenerator
    {
        public static bool IsAvaliable => SDU_WebUIStatus.ServerReady && !GeneratingImage;
        public static bool GeneratingImage { get; private set; } = false;
        public static string ProgressStr = string.Empty;
        public static float ProgressVal = 0;

        static bool m_StartGenerating = false;
        public readonly static List<Texture2D> m_Textures = new List<Texture2D>();
        public static void GenerateImage(Tex2ImgSetting iSetting)
        {
            if (m_StartGenerating || !IsAvaliable)
            {
                return;
            }
            m_StartGenerating = true;
            UCL.Core.ServiceLib.UCL_UpdateService.AddAction(() =>
            {
                m_StartGenerating = false;
                GenerateImageAsync(iSetting).Forget();
            });
        }
        public static string DefaultImageOutputFolder()
        {
            string aPath = Path.Combine(RunTimeData.InstallSetting.OutputPath, DateTime.Now.ToString("MM_dd_yyyy"));
            if (!Directory.Exists(aPath))
            {
                UCL.Core.FileLib.Lib.CreateDirectory(aPath);
            }
            return aPath;
        }
        public static Tuple<string, string> GetSaveImagePath()
        {
            string aPath = DefaultImageOutputFolder();
            string aFileName = $"{System.DateTime.Now.ToString("HHmmssff")}_{RunTimeData.Ins.m_OutPutFileID.ToString()}";
            RunTimeData.Ins.m_OutPutFileID = RunTimeData.Ins.m_OutPutFileID + 1;
            return Tuple.Create(aPath, aFileName);
        }
        public static void ClearTextures()
        {
            if (m_Textures.IsNullOrEmpty()) return;
            foreach (var aTexture in m_Textures)
            {
                GameObject.DestroyImmediate(aTexture);
            }
            m_Textures.Clear();
        }
        public static async System.Threading.Tasks.ValueTask GenerateImageAsync(Tex2ImgSetting iSetting)
        {
            RunTimeData.SaveRunTimeData();
            if (!IsAvaliable)
            {
                Debug.LogError($"GenerateImageAsync !IsAvaliable," +
                    $"SDU_WebUIStatus.s_ServerReady:{SDU_WebUIStatus.ServerReady},GeneratingImage:{GeneratingImage}");
                return;
            }
            GeneratingImage = true;
            ProgressStr = "Generating Image Start";
            ClearTextures();
            //List<Texture2D> aTextures = new List<Texture2D>();
            int aBatchCount = iSetting.m_BatchCount;
            for (int aBatchID = 0; aBatchID < aBatchCount; aBatchID++)
            {
                try
                {
                    using (var client = RunTimeData.SD_API.Client_Options)
                    {
                        JsonData aJson = new JsonData();

                        aJson["sd_model_checkpoint"] = iSetting.m_CheckPoint.m_CheckPoint;
                        string aJsonStr = aJson.ToJson();
                        var aResultJson = await client.SendWebRequestStringAsync(aJsonStr);
                        //Debug.LogWarning($"aResultJson:{aResultJson}");
                    }
                    using (var aClient = RunTimeData.SD_API.Client_Txt2img)
                    {
                        JsonData aJson = iSetting.GetConfigJson();

                        string aJsonStr = aJson.ToJson();
                        //GUIUtility.systemCopyBuffer = aJsonStr;
                        var aValueTask = aClient.SendWebRequestAsync(aJsonStr);
                        var aTask = aValueTask.AsTask();

                        while (aTask.Status != TaskStatus.RanToCompletion)
                        {
                            bool aEndTask = false;
                            switch (aTask.Status)
                            {
                                case TaskStatus.Faulted:
                                case TaskStatus.Canceled:
                                    {
                                        aEndTask = true;
                                        break;
                                    }
                            }
                            if (aEndTask)
                            {
                                break;
                            }
                            using (var aClientProgress = RunTimeData.SD_API.Client_Progress)
                            {
                                JsonData aProgressJson = new JsonData();

                                var aProgress = await aClientProgress.SendWebRequestAsync();
                                if (aProgress.Contains("progress"))
                                {
                                    double aProgressVal = aProgress["progress"].GetDouble(0);
                                    ProgressStr = $"Generating Image[{aBatchID + 1}/{aBatchCount}] " +
                                        $"{(100f * aProgressVal).ToString("0.0")}%";
                                    ProgressVal = (float)aProgressVal;
                                }
                                //Debug.LogWarning($"m_ProgressStr:{m_ProgressStr}");
                            }
                            await Task.Delay(500);
                        }
                        switch (aTask.Status)
                        {
                            case TaskStatus.RanToCompletion:
                                {
                                    ProgressStr = "Generating Image Success";
                                    break;
                                }
                            default:
                                {
                                    ProgressStr = $"Generating Image Fail, TaskStatus:{aTask.Status}";
                                    GeneratingImage = false;
                                    return;
                                }
                        }
                        //JsonData aResultJson = await aClient.SendWebRequestAsync(aJsonStr);
                        JsonData aResultJson = aValueTask.Result;

                        Debug.LogWarning("Image generating Ended");
                        if (aResultJson == null)
                        {
                            throw new Exception("SendWebRequestAsync, aResultJson == null");
                        }
                        if (!aResultJson.Contains("images"))
                        {
                            throw new Exception($"SendWebRequestAsync, !responses.Contains(\"images\"),aResultJson:{aResultJson.ToJsonBeautify()}");
                        }
                        var aImageOutputSetting = iSetting.m_ImageOutputSetting;
                        var aSavePath = GetSaveImagePath();
                        string aPath = aImageOutputSetting.OutputFolderPath;//aSavePath.Item1;
                        string aFileName = aSavePath.Item2;
                        RunTimeData.Ins.m_OutPutFileID = RunTimeData.Ins.m_OutPutFileID + 1;

                        var aFileTasks = new List<Task>();
                        var aImages = aResultJson["images"];
                        bool aRemoveLastImageOutput = iSetting.m_ControlNetSettings.m_EnableControlNet && !aImageOutputSetting.m_OutputControlNetInputImage;
                        Debug.LogWarning($"aImages.Count:{aImages.Count}");
                        for (int i = 0; i < aImages.Count; i++)
                        {
                            var aImageStr = aImages[i].GetString();
                            var aSplitStr = aImageStr.Split(",");
                            foreach (var aSplit in aSplitStr)
                            {
                                Debug.LogWarning($"aSplit:{aSplit}");
                            }

                            var aImageBytes = Convert.FromBase64String(aSplitStr[0]);
                            var aTexture = UCL.Core.TextureLib.Lib.CreateTexture(aImageBytes);

                            if (aRemoveLastImageOutput && i == aImages.Count - 1)
                            {

                            }
                            else
                            {
                                string aFilePath = Path.Combine(aPath, $"{aFileName}_{i}.png"); // M HH:mm:ss
                                Debug.Log($"aPath:{aPath},aFilePath:{aFilePath}");

                                aFileTasks.Add(File.WriteAllBytesAsync(aFilePath, aTexture.EncodeToPNG()));
                            }

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
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {

                }
            }

            ProgressStr = string.Empty;
            GeneratingImage = false;
            ProgressVal = 0f;
            //m_Textures.Append(aTextures);
            await Resources.UnloadUnusedAssets();
        }
    }
}