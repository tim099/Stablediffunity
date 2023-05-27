using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine.Networking;
using UCL.Core;
using System.Threading;

namespace SDU
{
    public static class SDU_FileDownloader
    {
        public class DownloadHandle
        {
            public string ProgressStr => $"{(100f*Progress).ToString("0.00")}% ,Downloaded Size:{DownloadedSize}";
            public float Progress { get; set;}
            public string ID { get; set; }
            public string FileName { get; set; }
            public string DownloadedSize { get; set; }
            public bool CancelDownload => CancellationTokenSource.IsCancellationRequested;
            public UnityWebRequest.Result Result { get; set; } = UnityWebRequest.Result.InProgress;
            public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
            public void OnGUI(UCL_ObjectDictionary iDataDic)
            {
                using (var aScope = new GUILayout.HorizontalScope("box"))
                {
                    if (GUILayout.Button("Cancel Download", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        CancellationTokenSource.Cancel();
                        //CancelDownload = true;
                    }
                    GUILayout.Label($"Downloading {FileName},Progress:{ProgressStr}");
                }
            }
        }
        public static Dictionary<string, DownloadHandle> DownloadingFiles => s_DownloadingFiles;
        private static Dictionary<string, DownloadHandle> s_DownloadingFiles = new Dictionary<string, DownloadHandle>();
        public static string GetDownloadFileHandleID(string iURL, string iFilePath)
        {
            return $"{iURL}_{iFilePath}";
        }
        public static DownloadHandle GetDownloadFileHandle(string iURL, string iFilePath)
        {
            string aID = GetDownloadFileHandleID(iURL, iFilePath);
            if (!s_DownloadingFiles.ContainsKey(aID))
            {
                return null;
            }
            return s_DownloadingFiles[aID];
        }
        public static void RemoveTmpFile(string iFilePath)
        {
            var aPath = GetTmpFilePath(iFilePath);
            if (File.Exists(aPath))
            {
                File.Delete(aPath);
            }
        }
        public static bool HasTmpFile(string iFilePath)
        {
            return File.Exists(GetTmpFilePath(iFilePath));
        }
        public static string GetTmpFileSizeStr(string iFilePath)
        {
            var aTmpFilePath = GetTmpFilePath(iFilePath);
            if (File.Exists(aTmpFilePath))
            {
                var aFileInfo = new FileInfo(aTmpFilePath);
                return $"{((aFileInfo.Length / (float)1048576)).ToString("0.00")}MB";
            }
            return $"File Not Exist, FilePath:{iFilePath}";
        }
        public static string GetTmpFilePath(string iFilePath)=> $"{iFilePath}.tmp";
        public static async UniTask DownloadFileAsync(string iURL, string iFilePath, bool iAutoRetryDownload = false)
        {
            if (File.Exists(iFilePath))
            {
                Debug.LogError($"DownloadFileAsync iFilePath:{iFilePath}");
                return;
            }
            string aID = GetDownloadFileHandleID(iURL,iFilePath);
            if(s_DownloadingFiles.ContainsKey(aID))
            {
                Debug.LogError($"DownloadFileAsync iFilePath:{iFilePath}, File already downloading");
                return;
            }
            var aHandle = new DownloadHandle();
            aHandle.ID = aID;
            aHandle.FileName = UCL.Core.FileLib.Lib.GetFileName(iFilePath);
            aHandle.Progress = 0;
            s_DownloadingFiles[aID] = aHandle;
            string aDir = Path.GetDirectoryName(iFilePath);
            if (!Directory.Exists(aDir))
            {
                Directory.CreateDirectory(aDir);
            }

            try
            {
                using (var aHeader = UnityWebRequest.Head(iURL))
                {
                    //    aHeader.SetRequestHeader("Content-Type", "application/json");
                    await aHeader.SendWebRequest();//.WithCancellation(aHandle.CancellationTokenSource.Token);
                                                   //long totalSize = long.Parse(aHeader.GetResponseHeader("Content-Length"));
                                                   //Debug.LogError($"totalSize{totalSize},aHeader:{aHeader.result}");
                    var aHeaders = aHeader.GetResponseHeaders();
                    foreach (var aKey in aHeaders.Keys)
                    {
                        Debug.Log($"{aKey},{aHeaders[aKey]},iURL:{iURL}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }



            string aTmpFilePath = GetTmpFilePath(iFilePath);

            using (var aUnityWebRequest = new UnityWebRequest(iURL))
            {
                aUnityWebRequest.method = UnityWebRequest.kHttpVerbGET;

                DownloadHandlerFile aDownloadHandlerFile = null;
                if (File.Exists(aTmpFilePath))
                {
                    var aFileInfo = new FileInfo(aTmpFilePath);
                    aUnityWebRequest.SetRequestHeader("Range", $"bytes={aFileInfo.Length}-");
                    Debug.Log($"Resume download bytes={aFileInfo.Length},aTmpFilePath:{aTmpFilePath}");
                    aDownloadHandlerFile = new DownloadHandlerFile(aTmpFilePath, true);
                    aHandle.DownloadedSize = GetTmpFileSizeStr(iFilePath);
                }
                if (aDownloadHandlerFile == null)
                {
                    aDownloadHandlerFile = new DownloadHandlerFile(aTmpFilePath);
                }
                //aDownloadHandlerFile.removeFileOnAbort = true;
                aUnityWebRequest.downloadHandler = aDownloadHandlerFile;
                System.DateTime aCheckTime = System.DateTime.Now;
                var aTask = aUnityWebRequest.SendWebRequest().WithCancellation(aHandle.CancellationTokenSource.Token);

                await UniTask.WaitUntil(() =>
                {
                    aHandle.Result = aUnityWebRequest.result;
                    if (aHandle.CancelDownload)
                    {
                        Debug.LogWarning($"{aHandle.FileName} ,Cancel Download");
                        aUnityWebRequest.Abort();
                        return true;
                    }
                    aHandle.Progress = aUnityWebRequest.downloadProgress;
                    if((System.DateTime.Now - aCheckTime).Seconds > 0.5f)
                    {
                        aCheckTime = System.DateTime.Now;

                        aHandle.DownloadedSize = GetTmpFileSizeStr(iFilePath);
                    }
                    if (aUnityWebRequest.result == UnityWebRequest.Result.InProgress)
                    {
                        return false;
                    }
                    return true;
                }, PlayerLoopTiming.FixedUpdate);

                
                

                switch (aUnityWebRequest.result)
                {
                    case UnityWebRequest.Result.Success:
                        {
                            aHandle.Progress = 1f;
                            System.IO.File.Move(aTmpFilePath, iFilePath);
                            Debug.Log($"DownloadFileAsync iURL:{iURL}");
                            break;
                        }
                    case UnityWebRequest.Result.ConnectionError:
                        {
                            if (iAutoRetryDownload && !aHandle.CancelDownload)
                            {
                                UCL.Core.ServiceLib.UCL_UpdateService.AddDelayAction(3f, () =>
                                {
                                    DownloadFileAsync(iURL, iFilePath, iAutoRetryDownload).Forget();
                                });
                            }
                            Debug.LogError($"DownloadFileAsync Fail, aUnityWebRequest.result:{aUnityWebRequest.result}");
                            break;
                        }
                    default:
                        {
                            Debug.LogError($"DownloadFileAsync Fail, aUnityWebRequest.result:{aUnityWebRequest.result}");
                            break;
                        }
                }

                s_DownloadingFiles.Remove(aID);
            }

        }
    }
}