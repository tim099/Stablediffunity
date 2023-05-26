using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine.Networking;

namespace SDU
{
    public static class SDU_FileDownloader
    {
        public class DownloadHandle
        {
            public string ProgressStr => $"{(100f*Progress).ToString("0.00")}%";
            public float Progress { get; set;}
            public string ID { get; set; }
            public bool CancelDownload { get; set; } = false;
        }
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
            aHandle.Progress = 0;
            s_DownloadingFiles[aID] = aHandle;
            string aDir = Path.GetDirectoryName(iFilePath);
            if (!Directory.Exists(aDir))
            {
                Directory.CreateDirectory(aDir);
            }

            var aUnityWebRequest = new UnityWebRequest(iURL);
            aUnityWebRequest.method = UnityWebRequest.kHttpVerbGET;
            var aDownloadHandlerFile = new DownloadHandlerFile(iFilePath);
            aDownloadHandlerFile.removeFileOnAbort = true;
            aUnityWebRequest.downloadHandler = aDownloadHandlerFile;

            var aTask = aUnityWebRequest.SendWebRequest();
            await UniTask.WaitUntil(() =>
            {
                if (aHandle.CancelDownload)
                {
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