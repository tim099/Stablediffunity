using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.UI;
using UnityEngine;
namespace SDU
{
    public class APISetting
    {
        public ControlNetAPI m_ControlNetAPI = new ControlNetAPI();
        public StablediffunityAPI m_StablediffunityAPI = new StablediffunityAPI();
        public StableDiffusionAPI m_StableDiffusionAPI = new StableDiffusionAPI();
        public OpenWebURL m_OpenWebURL = new OpenWebURL();
    }
    [System.Serializable]
    public class OpenWebURL : UCL.Core.UI.UCLI_FieldOnGUI
    {
        public StablediffunityAPI.GitCloneData m_GitCloneData = new StablediffunityAPI.GitCloneData();

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("Data"), iFieldName);
            if(GUILayout.Button("Open API Docs", UCL_GUIStyle.ButtonStyle))
            {
                System.Diagnostics.Process.Start(RunTimeData.SD_API.URL_Docs);
            }
            if (GUILayout.Button("Test StablediffunityAPI", UCL_GUIStyle.ButtonStyle))
            {
                TestAPI(RunTimeData.Stablediffunity_API.Client_GetVersion).Forget();
            }
            if (GUILayout.Button("Test StablediffunityAPI Git Clone", UCL_GUIStyle.ButtonStyle))
            {
                string aJson = m_GitCloneData.SerializeToJson().ToJson();
                Debug.LogError($"aJson:{aJson}");
                TestAPI(RunTimeData.Stablediffunity_API.Client_PostGitClone, aJson).Forget();
            }
            if (GUILayout.Button("Test ControlNetAPI", UCL_GUIStyle.ButtonStyle))
            {
                TestAPI(RunTimeData.ControlNet_API.Client_GetVersion).Forget();
            }
            //m_StablediffunityAPI
            return this;
        }
        public async UniTask TestAPI(SDU_WebUIClient.SDU_WebRequest iClient, string iJson = "")
        {
            using (iClient)
            {
                string aResult = await iClient.SendWebRequestStringAsync(iJson);
                Debug.LogError($"TestAPI URL:{iClient.WebRequest.url} ,Result:{aResult}");
            }
        }
    }
    [System.Serializable]
    public class StableDiffusionAPI
    {
        public static string ServerUrl => RunTimeData.ServerUrl;

        
        public string m_ApiCmdFlags = "/sdapi/v1/cmd-flags";
        
        public string m_ApiTxt2img = "/sdapi/v1/txt2img";
        public string m_ApiImg2img = "/sdapi/v1/img2img";
        public string m_ApiPngInfo = "/sdapi/v1/png-info";
        #region Get
        public string m_ApiSdModels = "/sdapi/v1/sd-models";
        public string m_Docs = "/docs";

        public string URL_SdModels => ServerUrl + m_ApiSdModels;
        public string URL_Docs => ServerUrl + m_Docs;
        #endregion

        #region Post
        public string m_ApiOptions = "/sdapi/v1/options";
        public string m_RefreshCheckpoints = "/sdapi/v1/refresh-checkpoints";

        public string URL_Options => ServerUrl + m_ApiOptions;


        public string URL_RefreshCheckpoints => ServerUrl + m_RefreshCheckpoints;
        #endregion


        public string URL_CmdFlags => ServerUrl + m_ApiCmdFlags;
        
        public string URL_Txt2img => ServerUrl + m_ApiTxt2img;
        public string URL_Img2img => ServerUrl + m_ApiImg2img;
        public string URL_PngInfo => ServerUrl + m_ApiPngInfo;

        #region Client
        public SDU_WebUIClient.SDU_WebRequest Client_Options =>
            new SDU_WebUIClient.SDU_WebRequest(URL_Options, SDU_WebRequest.Method.Post);

        public SDU_WebUIClient.SDU_WebRequest Client_RefreshCheckpoints => 
            new SDU_WebUIClient.SDU_WebRequest(URL_RefreshCheckpoints, SDU_WebRequest.Method.Post);


        public SDU_WebUIClient.SDU_WebRequest Client_Txt2img =>
            new SDU_WebUIClient.SDU_WebRequest(URL_Txt2img, SDU_WebRequest.Method.Post);
        public SDU_WebUIClient.SDU_WebRequest Client_Img2img =>
            new SDU_WebUIClient.SDU_WebRequest(URL_Img2img, SDU_WebRequest.Method.Post);
        public SDU_WebUIClient.SDU_WebRequest Client_Interrupt =>
            new SDU_WebUIClient.SDU_WebRequest(ServerUrl + "/sdapi/v1/interrupt", SDU_WebRequest.Method.Post);

        public SDU_WebUIClient.SDU_WebRequest Client_AppID =>
            new SDU_WebUIClient.SDU_WebRequest(ServerUrl + "/app_id", SDU_WebRequest.Method.Get);
        public SDU_WebUIClient.SDU_WebRequest Client_Progress =>
            new SDU_WebUIClient.SDU_WebRequest(ServerUrl + "/sdapi/v1/progress", SDU_WebRequest.Method.Get);
        public SDU_WebUIClient.SDU_WebRequest Client_Samplers =>
            new SDU_WebUIClient.SDU_WebRequest(ServerUrl + "/sdapi/v1/samplers", SDU_WebRequest.Method.Get);
        public SDU_WebUIClient.SDU_WebRequest Client_SdModels =>
            new SDU_WebUIClient.SDU_WebRequest(URL_SdModels, SDU_WebRequest.Method.Get);

        //public SDU_WebUIClient.SDU_WebRequest Client_Docs =>
        //    new SDU_WebUIClient.SDU_WebRequest(URL_Docs, SDU_WebRequest.Method.Get);
        #endregion
    }
    ///
    /// <summary>
    /// https://github.com/Mikubill/sd-webui-controlnet/wiki/API
    /// </summary>
    [System.Serializable]
    public class StablediffunityAPI
    {
        public static string ServerUrl => RunTimeData.ServerUrl;

        #region Client Get
        public SDU_WebUIClient.SDU_WebRequest Client_GetVersion => 
            new SDU_WebUIClient.SDU_WebRequest(ServerUrl+"/stablediffunity/version", SDU_WebRequest.Method.Get);
        #endregion

        #region Client Post
        public SDU_WebUIClient.SDU_WebRequest Client_PostGitClone => 
            new SDU_WebUIClient.SDU_WebRequest(ServerUrl + "/stablediffunity/git_clone", SDU_WebRequest.Method.Post);
        #endregion

        [System.Serializable]
        public class GitCloneData : UCL.Core.JsonLib.UnityJsonSerializable
        {
            public string m_url;
            public string m_target_dir;
            public string m_branch;
        }
    }
    /// <summary>
    /// https://github.com/Mikubill/sd-webui-controlnet/wiki/API
    /// </summary>
    [System.Serializable]
    public class ControlNetAPI
    {
        public static string ServerUrl => RunTimeData.ServerUrl;

        public const string ContentType = "application/json";

        #region Get
        public string m_ModelLists = "/controlnet/model_list";
        public string m_ModuleLists = "/controlnet/module_list";
        public string m_Version = "/controlnet/version";
        #endregion

        #region Post
        public string m_Detect = "/controlnet/detect";
        #endregion

        #region Get
        public string URL_ModelLists => ServerUrl + m_ModelLists;
        public string URL_ModuleLists => ServerUrl + m_ModuleLists;
        public string URL_Version => ServerUrl + m_Version;
        #endregion

        #region Post
        public string URL_Detect => ServerUrl + m_Detect;
        #endregion

        #region Client
        public SDU_WebUIClient.SDU_WebRequest Client_ModelLists => new SDU_WebUIClient.SDU_WebRequest(URL_ModelLists, SDU_WebRequest.Method.Get);
        public SDU_WebUIClient.SDU_WebRequest Client_GetVersion => new SDU_WebUIClient.SDU_WebRequest(ServerUrl + "/controlnet/version", SDU_WebRequest.Method.Get);
        #endregion
    }
}