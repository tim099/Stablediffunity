using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UnityEngine;
using System.IO;
using UCL.Core.JsonLib;
using UCL.Core.UI;

namespace SDU
{
    public class SDU_DownloadFileSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public enum FileType
        {
            CheckPoint,
            Lora,
        }

        public string m_URL;
        public SDU_FolderSetting m_FolderSetting = new SDU_FolderSetting();
        public string m_FileName = "NewFile";
        public string m_FileExtension = "safetensors";
        //public FolderEnum m_Folder = FolderEnum.Lora;
        public bool m_RetryDownloadOnFail = false;

        [UCL.Core.ATTR.UCL_HideOnGUI] public string m_WebPageURL;
        public string FolderPath => m_FolderSetting.Path;
        public string FilePath => Path.Combine(FolderPath, $"{m_FileName}.{m_FileExtension}");
        public SDU_FileDownloader.DownloadHandle DownloadHandle => SDU_FileDownloader.GetDownloadFileHandle(m_URL, FilePath);

        private bool m_Show = false;
        private bool m_Loaded = false;
        private string m_LoadSettingName;
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            m_Loaded = true;
        }
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            if (m_Loaded)
            {
                iDataDic.Clear();
                m_Loaded = false;
            }
            string aDownloadFileSettingPath = RunTimeData.InstallSetting.GetDownloadSettingsFolderPath(m_FolderSetting.m_Folder);


            if (!Directory.Exists(aDownloadFileSettingPath))
            {
                SDU_FileInstall.CheckInstallEnv(aDownloadFileSettingPath);
            }
            using (var aScope = new GUILayout.HorizontalScope())
            {
                m_Show = UCL_GUILayout.Toggle(m_Show);
                
                using (var aScope2 = new GUILayout.VerticalScope("box"))
                {
                    GUILayout.Label(iFieldName, UCL_GUIStyle.LabelStyle);
                    if (m_Show)
                    {
                        var aFilePath = FilePath;

                        var aHandle = DownloadHandle;
                        UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("DownloadFileSetting"), iFieldName, true);
                        if (aHandle == null)
                        {
                            if (File.Exists(aFilePath))
                            {
                                using (var aScope3 = new GUILayout.HorizontalScope())
                                {
                                    //if (GUILayout.Button("Resume Download", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                                    //{
                                    //    SDU_FileDownloader.DownloadFileAsync(m_URL, aFilePath).Forget();
                                    //}
                                    if (GUILayout.Button("Copy Path", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                                    {
                                        GUIUtility.systemCopyBuffer = aFilePath;
                                    }
                                    GUILayout.Label($"File Downloaded:{aFilePath}");

                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Download", UCL_GUIStyle.ButtonStyle))
                                {
                                    SDU_FileDownloader.DownloadFileAsync(m_URL, aFilePath, m_RetryDownloadOnFail).Forget();
                                }
                            }
                        }
                        else
                        {
                            aHandle.OnGUI(iDataDic.GetSubDic("Handle"));
                        }

                        using (var aScope3 = new GUILayout.VerticalScope("box"))
                        {
                            using (var aScope4 = new GUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Save Setting", GUILayout.ExpandWidth(false)))
                                {
                                    string aSaveFileName = $"{m_FileName}.json";
                                    m_LoadSettingName = aSaveFileName;
                                    File.WriteAllText(Path.Combine(aDownloadFileSettingPath, aSaveFileName),
                                        SerializeToJson().ToJsonBeautify());
                                }

                                if (GUILayout.Button("Open Folder"))
                                {
                                    RunTimeData.InstallSetting.OpenDownloadSettingsFolder(m_FolderSetting.m_Folder);
                                }
                            }

                            using (var aScope4 = new GUILayout.HorizontalScope())
                            {
                                var aFiles = UCL.Core.FileLib.Lib.GetFilesName(aDownloadFileSettingPath, "*.json", SearchOption.TopDirectoryOnly);
                                if (!aFiles.IsNullOrEmpty())
                                {
                                    if (string.IsNullOrEmpty(m_LoadSettingName))
                                    {
                                        m_LoadSettingName = aFiles[0];
                                    }
                                    var aPath = Path.Combine(aDownloadFileSettingPath, m_LoadSettingName);
                                    if (File.Exists(aPath))
                                    {
                                        if (GUILayout.Button("Load Setting", GUILayout.ExpandWidth(false)))
                                        {
                                            var aJsonStr = File.ReadAllText(aPath);
                                            JsonData aJson = JsonData.ParseJson(aJsonStr);
                                            DeserializeFromJson(aJson);
                                        }
                                    }
                                    m_LoadSettingName = UCL.Core.UI.UCL_GUILayout.PopupAuto(m_LoadSettingName, aFiles, iDataDic.GetSubDic("LoadSettingName"), "PopupAuto");
                                }

                            }
                        }
                        using (var aScope3 = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("WebPageURL", UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                            m_WebPageURL = GUILayout.TextField(m_WebPageURL);
                            if (GUILayout.Button("Open Web", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                            {
                                //TestURL(m_WebPageURL).Forget();
                                System.Diagnostics.Process.Start(m_WebPageURL);
                            }

                            //System.Diagnostics.Process.Start(RunTimeData.Ins.m_WebURL);
                        }
                    }

                }

            }





            return this;
        }
        private async UniTask TestURL(string iURL)
        {
            using (var client = new SDU_WebUIClient.SDU_WebRequest(iURL, SDU_WebRequest.Method.Get))
            {
                var responses = await client.SendWebRequestStringAsync();
                GUIUtility.systemCopyBuffer = responses;
                Debug.LogWarning($"TestURL:{responses}");
            }
        }
    }
}