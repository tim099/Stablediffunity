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
        public static async UniTask DownloadFileAsync(string iURL, string iFilePath, System.Action<float> iProgressCallback = null)
        {
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
            if(iProgressCallback == null)
            {
                await aUnityWebRequest.SendWebRequest();
            }
            else
            {
                var aTask = aUnityWebRequest.SendWebRequest();
                await UniTask.WaitUntil(() => 
                {
                    iProgressCallback.Invoke(aUnityWebRequest.downloadProgress);
                    if (aUnityWebRequest.result == UnityWebRequest.Result.InProgress)
                    {
                        return false;
                    }
                    return true;
                }, PlayerLoopTiming.FixedUpdate);
                iProgressCallback.Invoke(1f);
            }
            
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
        public static IEnumerator DownloadFileEnumerator(string iURL, string iFilePath, System.Action<float> iProgressCallback = null)
        {
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
            if (iProgressCallback == null)
            {
                yield return aUnityWebRequest.SendWebRequest();
            }
            else
            {
                aUnityWebRequest.SendWebRequest();
                iProgressCallback.Invoke(0f);
                while (aUnityWebRequest.result == UnityWebRequest.Result.InProgress)
                {
                    Debug.LogWarning($"aUnityWebRequest.downloadProgress:{aUnityWebRequest.downloadProgress}");
                    iProgressCallback.Invoke(aUnityWebRequest.downloadProgress);
                    yield return null;
                }

                iProgressCallback.Invoke(1f);
            }

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