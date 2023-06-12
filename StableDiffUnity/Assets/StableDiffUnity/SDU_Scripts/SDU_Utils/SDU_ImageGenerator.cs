using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;


namespace SDU
{
    public static class SDU_ImageGenerator
    {
        public static bool IsAvaliable => SDU_Server.ServerReady && !GeneratingImage;
        public static SDU_InputImage PrevGeneratedImage { get; private set; } = null;
        public static bool GeneratingImage { get; private set; } = false;
        public static string ProgressStr = string.Empty;
        public static float ProgressVal = 0;

        public readonly static List<Texture2D> s_Textures = new List<Texture2D>();
        public static void ClearPrevGeneratedImage()
        {
            PrevGeneratedImage = null;
        }
        public static void OnGUI(UCL_ObjectDictionary iDataDic)
        {
            if (GeneratingImage)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(ProgressStr, UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                if (ProgressVal > 0) GUILayout.HorizontalSlider(ProgressVal, 0f, 1f);
                GUILayout.EndHorizontal();
            }
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
        public static Tuple<string,string> SaveImage(Texture2D iTexture, string iSubFolderPath = null, SDU_ImageOutputSetting iSetting = null
            , bool iIncreaseID = true)
        {
            var aSavePath = GetSaveImagePath(iSetting, iIncreaseID);
            string aFolderPath = aSavePath.Item1;
            if (!string.IsNullOrEmpty(iSubFolderPath))
            {
                aFolderPath = Path.Combine(aFolderPath, iSubFolderPath);
            }
            string aFileName = $"Input_{aSavePath.Item2}.png";
            string aFilePath = Path.Combine(aFolderPath, aFileName); // M HH:mm:ss
            try
            {

                Debug.Log($"Save Image Path:{aFilePath}");
                if (!Directory.Exists(aFolderPath))
                {
                    UCL.Core.FileLib.Lib.CreateDirectory(aFolderPath);
                }

                File.WriteAllBytes(aFilePath, iTexture.EncodeToPNG());
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return new Tuple<string, string>(aFolderPath, aFileName);
        }
        public static Tuple<string, string> GetSaveImagePath(SDU_ImageOutputSetting iSetting = null, bool iIncreaseID = true)
        {
            if (iSetting == null)
            {
                iSetting = RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting;
            }

            string aPath = iSetting.OutputFolderPath;
            if (!Directory.Exists(aPath))
            {
                Directory.CreateDirectory(aPath);
            }
            if (iIncreaseID) ++iSetting.m_OutPutFileID;
            string aFileID = (iSetting.m_OutPutFileID).ToString();
            string aFileName = $"{System.DateTime.Now.ToString("HHmmssff")}_{aFileID}";
            return Tuple.Create(aPath, aFileName);
        }
        public static void ClearTextures()
        {
            if (s_Textures.IsNullOrEmpty()) return;
            foreach (var aTexture in s_Textures)
            {
                GameObject.DestroyImmediate(aTexture);
            }
            s_Textures.Clear();
        }
        public static async UniTask GenerateImageAsync(SDU_ImgSetting iSetting, CancellationToken iCancellationToken)
        {
            RunTimeData.SaveRunTimeData();
            if (!IsAvaliable)
            {
                Debug.LogError($"GenerateImageAsync !IsAvaliable," +
                    $"SDU_WebUIStatus.s_ServerReady:{SDU_Server.ServerReady},GeneratingImage:{GeneratingImage}");
                return;
            }
            GeneratingImage = true;
            ProgressStr = "Generating Image Start";
            ClearTextures();

            int aBatchCount = iSetting.m_BatchCount;
            for (int aBatchID = 0; aBatchID < aBatchCount; aBatchID++)
            {
                try
                {
                    //using (var client = RunTimeData.SD_API.Client_Options)
                    //{
                    //    JsonData aJson = new JsonData();

                    //    aJson["sd_model_checkpoint"] = iSetting.m_CheckPoint.m_CheckPoint;
                    //    var aResultJson = await client.SendWebRequestStringAsync(aJson.ToJson());
                    //    //Debug.LogWarning($"aResultJson:{aResultJson}");
                    //}
                    await iSetting.m_CheckPoint.ApplyToServer();
                    //SetCheckPointAsync
                    using (var aClient = iSetting.Client)
                    {
                        var aImageOutputSetting = iSetting.m_ImageOutputSetting;
                        int aEnabledControlNetCount = iSetting.GetEnabledControlNetSettings().Count;
                        bool aRemoveControlNetInputImage = !aImageOutputSetting.m_OutputControlNetInputImage && aEnabledControlNetCount > 0;
                        JsonData aJson = iSetting.GetConfigJson();
                        iSetting.m_ResultInfo = aJson;
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
                            if(iCancellationToken.IsCancellationRequested)
                            {
                                using (var aClientInterrupt = RunTimeData.SD_API.Client_Interrupt)
                                {
                                    await aClientInterrupt.SendWebRequestAsync();
                                }
                                break;
                            }
                            else
                            {
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
                        if (aResultJson.Contains("info"))
                        {
                            JsonData aInfo = aResultJson["info"];
                            iSetting.m_ResultInfo = aInfo;
                            //if(aInfo.GetString())
                            string aInfoJson = aInfo.GetString();
                            try
                            {
                                JsonData aInfoData = JsonData.ParseJson(aInfoJson);

                                iSetting.m_ResultInfo = aInfoData;
                            }
                            catch(System.Exception e)
                            {
                                Debug.LogException(e);
                            }
                            
                            //string aInfoJson = aInfo.ToJsonBeautify();
                            //Debug.LogWarning($"Result info:{aInfoJson}");
                            //GUIUtility.systemCopyBuffer = aInfoJson;
                        }
                        else
                        {
                            Debug.LogError("!aResultJson.Contains(\"info\")");
                        }
                        
                        var aSavePath = GetSaveImagePath(aImageOutputSetting);
                        string aFolderPath = aImageOutputSetting.OutputFolderPath;//aSavePath.Item1;
                        string aFileName = aSavePath.Item2;

                        var aFileTasks = new List<Task>();
                        var aImages = aResultJson["images"];

                        Debug.LogWarning($"aImages.Count:{aImages.Count}");
                        if (aImageOutputSetting.m_OutputGenerateImageSetting)
                        {
                            
                            string aFilePath = Path.Combine(aFolderPath, $"{aFileName}.json"); // M HH:mm:ss
                            Debug.Log($"aPath:{aFolderPath},aFilePath:{aFilePath}");
                            long aSeed = iSetting.m_Seed;
                            if (iSetting.m_ResultInfo.Contains("seed"))
                            {
                                iSetting.m_Seed = iSetting.m_ResultInfo["seed"].GetLong();
                            }
                            var aSettingJson = iSetting.SerializeToJson();
                            aFileTasks.Add(File.WriteAllTextAsync(aFilePath, aSettingJson.ToJsonBeautify()));
                            iSetting.m_Seed = aSeed;//restore Seed
                        }
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
                            //PrevGeneratedImage
                            if (aRemoveControlNetInputImage && i >= aImages.Count - aEnabledControlNetCount)
                            {

                            }
                            else
                            {

                                string aFileSaveName = $"{aFileName}_{i}.png";
                                string aFilePath = Path.Combine(aFolderPath, aFileSaveName); // M HH:mm:ss
                                Debug.Log($"aPath:{aFolderPath},aFilePath:{aFilePath}");

                                aFileTasks.Add(File.WriteAllBytesAsync(aFilePath, aTexture.EncodeToPNG()));

                                if (i == 0)
                                {
                                    if (PrevGeneratedImage == null)
                                    {
                                        PrevGeneratedImage = new SDU_InputImage();
                                    }
                                    else
                                    {
                                        PrevGeneratedImage.Clear();
                                    }
                                    PrevGeneratedImage.m_LoadImageSetting.m_FolderPath = aFolderPath;
                                    PrevGeneratedImage.m_LoadImageSetting.m_FileName = aFileSaveName;
                                }
                            }

                            s_Textures.Add(aTexture);
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