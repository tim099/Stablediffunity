using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UnityEngine;
using System.IO;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using System.Linq;

namespace SDU
{
    public class SDU_DownloadFileSetting : UnityJsonSerializable, UCLI_FieldOnGUI, UCLI_ShortName
    {
        public string m_URL;
        public SDU_InstallFolderSetting m_FolderSetting = new SDU_InstallFolderSetting();

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_FileName = "NewFile";

        public string m_FileExtension = "safetensors";
        //public bool m_AutoRetryDownload = true;

        [UCL.Core.ATTR.UCL_HideOnGUI] 
        public string m_WebPageURL;

        public string FolderPath => m_FolderSetting.Path();
        public string FilePath { 
            get 
            {
                try
                {
                    return Path.Combine(FolderPath, $"{m_FileName}.{m_FileExtension}");
                }
                catch(System.Exception e)
                {
                    Debug.LogException(e);
                }
                return string.Empty;
            } 
        }
        public SDU_FileDownloader.DownloadHandle DownloadHandle => SDU_FileDownloader.GetDownloadFileHandle(m_URL, FilePath);
        public string GetShortName() => $"DownloadFileSetting({m_FileName})";
        private bool m_Show = false;
        private bool RequireClearDic { get; set; } = false;
        private string m_LoadSettingName;
        public override void DeserializeFromJson(JsonData iJson)
        {
            base.DeserializeFromJson(iJson);
            RequireClearDic = true;
        }
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            string aDownloadFileSettingPath = m_FolderSetting.GetDownloadSettingsFolderPath();
            const string KeyDownloadFileSettingPath = "DownloadFileSettingPath";
            bool aIsUpdatePath = aDownloadFileSettingPath != iDataDic.GetData(KeyDownloadFileSettingPath, aDownloadFileSettingPath);
            if (aIsUpdatePath)
            {
                RequireClearDic = true;
                m_LoadSettingName = string.Empty;
            }
            if (RequireClearDic)
            {
                iDataDic.Clear();
                RequireClearDic = false;
            }

            iDataDic.SetData(KeyDownloadFileSettingPath, aDownloadFileSettingPath);

            if (!Directory.Exists(aDownloadFileSettingPath))
            {
                UCL.Core.FileLib.Lib.CreateDirectory(aDownloadFileSettingPath);
                //SDU_FileInstall.CheckInstallEnv(aDownloadFileSettingPath);
            }
            using (var aScope = new GUILayout.HorizontalScope())
            {
                m_Show = UCL_GUILayout.Toggle(m_Show);
                
                using (var aScope2 = new GUILayout.VerticalScope("box"))
                {
                    GUILayout.Label($"{iFieldName}({m_FileName})", UCL_GUIStyle.LabelStyle);
                    if (m_Show)
                    {
                        using (var aScope3 = new GUILayout.VerticalScope("box"))
                        {
                            using (var aScope4 = new GUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Save Setting", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                                {
                                    string aSaveFileName = $"{m_FileName}.json";
                                    m_LoadSettingName = aSaveFileName;
                                    File.WriteAllText(Path.Combine(aDownloadFileSettingPath, aSaveFileName),
                                        SerializeToJson().ToJsonBeautify());
                                }
                                m_FileName = GUILayout.TextField(m_FileName);
                                if (GUILayout.Button("Open Folder", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                                {
                                    m_FolderSetting.OpenDownloadSettingsFolder();
                                }
                            }

                            using (var aScope4 = new GUILayout.HorizontalScope())
                            {
                                var aFiles = UCL.Core.FileLib.Lib.GetFilesName(aDownloadFileSettingPath, "*.json", SearchOption.TopDirectoryOnly).ToList();
                                if (aFiles.Count == 0) aFiles.Add(string.Empty);
                                if (string.IsNullOrEmpty(m_LoadSettingName))
                                {
                                    m_LoadSettingName = aFiles[0];
                                }
                                var aPath = Path.Combine(aDownloadFileSettingPath, m_LoadSettingName);
                                bool aIsFileExist = File.Exists(aPath);
                                if (GUILayout.Button("Load Setting",
                                    UCL_GUIStyle.GetButtonStyle(aIsFileExist ? Color.white : Color.red), GUILayout.ExpandWidth(false)))
                                {
                                    if (aIsFileExist)
                                    {
                                        var aJsonStr = File.ReadAllText(aPath);
                                        JsonData aJson = JsonData.ParseJson(aJsonStr);
                                        DeserializeFromJson(aJson);
                                    }
                                }
                                m_LoadSettingName = UCL.Core.UI.UCL_GUILayout.PopupAuto(m_LoadSettingName, aFiles, iDataDic.GetSubDic("LoadSettingName"), "PopupAuto");
                            }
                        }
                        if (!string.IsNullOrEmpty(m_URL) && !string.IsNullOrEmpty(m_FileName))
                        {
                            var aFilePath = FilePath;
                            var aHandle = DownloadHandle;
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
                                        GUILayout.Label($"File Downloaded: {aFilePath}");

                                    }
                                }
                                else
                                {
                                    if (SDU_FileDownloader.HasTmpFile(aFilePath))//Download interrupted
                                    {
                                        using (var aScope3 = new GUILayout.HorizontalScope())
                                        {
                                            if (GUILayout.Button("Continue Download", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                                            {
                                                SDU_FileDownloader.DownloadFileAsync(m_URL, aFilePath, true).Forget();
                                            }
                                            GUILayout.Label($"Downloaded Size:{SDU_FileDownloader.GetTmpFileSizeStr(aFilePath)}",
                                                UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));

                                            GUILayout.Space(60);
                                            if (GUILayout.Button("Delete Downloaded Cache File", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                                            {
                                                SDU_FileDownloader.RemoveTmpFile(aFilePath);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (GUILayout.Button("Download", UCL_GUIStyle.ButtonStyle))
                                        {
                                            SDU_FileDownloader.DownloadFileAsync(m_URL, aFilePath, true).Forget();
                                        }
                                    }

                                }
                            }
                            else
                            {
                                aHandle.OnGUI(iDataDic.GetSubDic("Handle"));
                            }

                            GUILayout.Space(30);
                        }
                        else
                        {
                            GUILayout.Space(50);
                        }
                        using (var aScope3 = new GUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Open Webpage", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                            {
                                //TestURL(m_WebPageURL).Forget();
                                System.Diagnostics.Process.Start(m_WebPageURL);
                            }
                            GUILayout.Label("WebPageURL", UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                            m_WebPageURL = GUILayout.TextField(m_WebPageURL);
                            //System.Diagnostics.Process.Start(RunTimeData.Ins.m_WebURL);
                        }
                        UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("DownloadFileSetting"), iFieldName, true);
                    }

                }

            }

            return this;
        }
        private async UniTask TestURL(string iURL)
        {
            using (var client = new SDU_Client.WebRequest(iURL, SDU_Client.Method.Get))
            {
                var responses = await client.SendWebRequestStringAsync();
                GUIUtility.systemCopyBuffer = responses;
                Debug.LogWarning($"TestURL:{responses}");
            }
        }
    }
}