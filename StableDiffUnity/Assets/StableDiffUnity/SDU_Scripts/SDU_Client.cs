/*
AutoHeader Test
to change the auto header please go to RCG_AutoHeader.cs
*/
using Cysharp.Threading.Tasks;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UCL.Core.JsonLib;
using UnityEngine;
using UnityEngine.Networking;

namespace SDU
{
    public class WebRequestException : Exception
    {
        public WebRequestException(string str) : base(str) { }
    }
    public static class SDU_Client
    {
        public static string GetMethodVerb(this Method iMethod)
        {
            switch (iMethod)
            {
                case Method.Get:
                    {
                        return UnityWebRequest.kHttpVerbGET;
                    }
                case Method.Post:
                    {
                        return UnityWebRequest.kHttpVerbPOST;
                    }
            }
            return UnityWebRequest.kHttpVerbGET;
        }
        public enum Method
        {
            Get,
            Post,
        }
        public const string ContentType = "Content-Type";
        public const string ApplicationJson = "application/json";
        public class WebRequest : IDisposable
        {
            public string URL { get; protected set; }
            public Method Method { get; protected set; }

            public WebRequest(string iURL, Method iMethod)
            {
                URL = iURL;
                Method = iMethod;
            }
            private UnityWebRequest CreateUnityWebRequest()
            {
                var aRequest = new UnityWebRequest(URL, Method.GetMethodVerb());
                aRequest.SetRequestHeader(ContentType, ApplicationJson);
                return aRequest;
            }
            public void Dispose()
            {

            }
            public async ValueTask<JsonData> SendWebRequestAsync(string iJson = null)
            {
                string aResult = await SendWebRequestStringAsync(iJson);
                if (string.IsNullOrEmpty(aResult) || aResult == "null")
                {
                    return null;
                }
                else
                {
                    return UCL.Core.JsonLib.JsonData.ParseJson(aResult);
                }
            }
            public async ValueTask<string> SendWebRequestStringAsync(string iJson = null)
            {
                using (UnityWebRequest aWebRequest = CreateUnityWebRequest())
                {
                    if (iJson != null)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(iJson);
                        aWebRequest.uploadHandler = new UploadHandlerRaw(bytes);
                    }

                    aWebRequest.downloadHandler = new DownloadHandlerBuffer();

                    await aWebRequest.SendWebRequest();
                    if (aWebRequest.result == UnityWebRequest.Result.Success)
                    {
                        return aWebRequest.downloadHandler.text;
                    }
                    else
                    {
                        //Debug.LogError($"WebRequest.error:{WebRequest.error},URL:{WebRequest.url}");
                        throw new WebRequestException($"WebRequest.method:{aWebRequest.method},error:{aWebRequest.error},URL:{aWebRequest.url}");
                    }
                }
            }
            public async UniTask<string> SendAsyncUniTask(CancellationToken iCancellationToken, string iJson = null)
            {
                using (UnityWebRequest aWebRequest = CreateUnityWebRequest())
                {
                    if (iJson != null)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(iJson);
                        aWebRequest.uploadHandler = new UploadHandlerRaw(bytes);
                    }

                    aWebRequest.downloadHandler = new DownloadHandlerBuffer();
                    UnityWebRequestAsyncOperation aAsyncOperation = aWebRequest.SendWebRequest();
                    await UniTask.WaitUntil(() =>
                    {
                        return aAsyncOperation.isDone;
                    }, cancellationToken: iCancellationToken);
                    iCancellationToken.ThrowIfCancellationRequested();
                    if (aWebRequest.result == UnityWebRequest.Result.Success)
                    {
                        return aWebRequest.downloadHandler.text;
                    }
                    else
                    {
                        //Debug.LogError($"WebRequest.error:{WebRequest.error},URL:{WebRequest.url}");
                        throw new WebRequestException($"WebRequest.method:{aWebRequest.method},error:{aWebRequest.error},URL:{aWebRequest.url}");
                    }
                }
            }
        }
    }
}