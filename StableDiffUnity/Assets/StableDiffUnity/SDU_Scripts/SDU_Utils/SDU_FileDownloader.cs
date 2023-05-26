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
            public string ProgressStr => $"{(100f*Progress).ToString("0.00")}%";
            public float Progress { get; set;}
            public string ID { get; set; }
            public string FileName { get; set; }
            public bool CancelDownload => CancellationTokenSource.IsCancellationRequested;

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
        public static async UniTask DownloadFileAsync(string iURL, string iFilePath)
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

            //using (var aHeader = UnityWebRequest.Head(iURL))
            //{
            //    //    aHeader.SetRequestHeader("Content-Type", "application/json");
            //    await aHeader.SendWebRequest();//.WithCancellation(aHandle.CancellationTokenSource.Token);
            //    long totalSize = long.Parse(aHeader.GetResponseHeader("Content-Length"));
            //    Debug.LogError($"totalSize{totalSize},aHeader:{aHeader.result}");
            //}


            string aTmpFilePath = $"{iFilePath}.tmp";

            var aUnityWebRequest = new UnityWebRequest(iURL);
            aUnityWebRequest.method = UnityWebRequest.kHttpVerbGET;

            DownloadHandlerFile aDownloadHandlerFile = null;
            if (File.Exists(aTmpFilePath))
            {
                var aFileInfo = new FileInfo(aTmpFilePath);
                aUnityWebRequest.SetRequestHeader("Range", $"bytes={aFileInfo.Length}-");
                Debug.Log($"Resume download bytes={aFileInfo.Length},aTmpFilePath:{aTmpFilePath}");
                aDownloadHandlerFile = new DownloadHandlerFile(aTmpFilePath, true);
            }
            if(aDownloadHandlerFile == null)
            {
                aDownloadHandlerFile = new DownloadHandlerFile(aTmpFilePath);
            }
            //aDownloadHandlerFile.removeFileOnAbort = true;
            aUnityWebRequest.downloadHandler = aDownloadHandlerFile;

            var aTask = aUnityWebRequest.SendWebRequest().WithCancellation(aHandle.CancellationTokenSource.Token);
            await UniTask.WaitUntil(() =>
            {
                if (aHandle.CancelDownload)
                {
                    Debug.LogWarning($"{aHandle.FileName} ,Cancel Download");
                    aUnityWebRequest.Abort();
                    return true;
                }
                aHandle.Progress = aUnityWebRequest.downloadProgress;
                if (aUnityWebRequest.result == UnityWebRequest.Result.InProgress)
                {
                    return false;
                }
                return true;
            }, PlayerLoopTiming.FixedUpdate);

            aHandle.Progress = 1f;
            s_DownloadingFiles.Remove(aID);

            switch (aUnityWebRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    {
                        System.IO.File.Move(aTmpFilePath, iFilePath);
                        Debug.Log($"DownloadFileAsync iURL:{iURL}");
                        break;
                    }
                default:
                    {
                        Debug.LogError($"DownloadFileAsync Fail, aUnityWebRequest.result:{aUnityWebRequest.result}");
                        break;
                    }
            }
        }
    }
}