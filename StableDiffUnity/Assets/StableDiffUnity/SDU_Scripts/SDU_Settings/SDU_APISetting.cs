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
        public StableDiffunityAPI m_StablediffunityAPI = new StableDiffunityAPI();
        public StableDiffusionAPI m_StableDiffusionAPI = new StableDiffusionAPI();
        public OpenWebURL m_OpenWebURL = new OpenWebURL();
    }
    [System.Serializable]
    public class OpenWebURL : UCL.Core.UI.UCLI_FieldOnGUI
    {
        //public StableDiffunityAPI.GitCloneData m_GitCloneData = new StableDiffunityAPI.GitCloneData();

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
            if (GUILayout.Button("Test StablediffunityAPI VAE", UCL_GUIStyle.ButtonStyle))
            {
                RunTimeData.WebUISetting.RefreshVAEs().Forget();
                //TestAPI(RunTimeData.Stablediffunity_API.Client_GetVAEs).Forget();
            }
            //if (GUILayout.Button("Test StablediffunityAPI Git Clone", UCL_GUIStyle.ButtonStyle))
            //{
            //    string aJson = m_GitCloneData.SerializeToJson().ToJson();
            //    Debug.LogError($"aJson:{aJson}");
            //    TestAPI(RunTimeData.Stablediffunity_API.Client_PostGitClone, aJson).Forget();
            //}
            if (GUILayout.Button("Test ControlNetAPI", UCL_GUIStyle.ButtonStyle))
            {
                TestAPI(RunTimeData.ControlNet_API.Client_GetVersion).Forget();
            }
            //m_StablediffunityAPI
            return this;
        }
        public async UniTask TestAPI(SDU_Client.WebRequest iClient, string iJson = "")
        {
            using (iClient)
            {
                string aResult = await iClient.SendWebRequestStringAsync(iJson);
                Debug.LogError($"TestAPI URL:{iClient.URL} ,Result:{aResult}");
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
        #region Get
        public string m_Docs = "/docs";
        public string URL_Docs => ServerUrl + m_Docs;
        #endregion



        public string URL_CmdFlags => ServerUrl + m_ApiCmdFlags;
        
        public string URL_Txt2img => ServerUrl + m_ApiTxt2img;
        public string URL_Img2img => ServerUrl + m_ApiImg2img;
        public string URL_PngInfo => ServerUrl + "/sdapi/v1/png-info";

        #region Client
        public SDU_Client.WebRequest Client_Options =>
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/options", SDU_Client.Method.Post);

        public SDU_Client.WebRequest Client_RefreshCheckpoints => 
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/refresh-checkpoints", SDU_Client.Method.Post);
        public SDU_Client.WebRequest Client_RefreshLoras =>
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/refresh-loras", SDU_Client.Method.Post);

        public SDU_Client.WebRequest Client_Txt2img =>
            new SDU_Client.WebRequest(URL_Txt2img, SDU_Client.Method.Post);
        public SDU_Client.WebRequest Client_Img2img =>
            new SDU_Client.WebRequest(URL_Img2img, SDU_Client.Method.Post);
        public SDU_Client.WebRequest Client_Interrupt =>
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/interrupt", SDU_Client.Method.Post);
        /// <summary>
        /// Not in WebUI master branch yet, currently in api-quit-restart branch
        /// </summary>
        public SDU_Client.WebRequest Client_RestartWebUI =>
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/restart-webui", SDU_Client.Method.Post);



        public SDU_Client.WebRequest Client_AppID =>
            new SDU_Client.WebRequest(ServerUrl + "/app_id", SDU_Client.Method.Get);
        public SDU_Client.WebRequest Client_Progress =>
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/progress", SDU_Client.Method.Get);
        public SDU_Client.WebRequest Client_Samplers =>
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/samplers", SDU_Client.Method.Get);

        public SDU_Client.WebRequest Client_SdModels =>
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/sd-models", SDU_Client.Method.Get);
        public SDU_Client.WebRequest Client_Loras =>
            new SDU_Client.WebRequest(ServerUrl + "/sdapi/v1/loras", SDU_Client.Method.Get);
        #endregion
    }
    ///
    /// <summary>
    /// https://github.com/Mikubill/sd-webui-controlnet/wiki/API
    /// </summary>
    [System.Serializable]
    public class StableDiffunityAPI
    {
        public static string ServerUrl => RunTimeData.ServerUrl;

        #region Client Get
        public SDU_Client.WebRequest Client_GetVersion => 
            new SDU_Client.WebRequest(ServerUrl+"/stablediffunity/version", SDU_Client.Method.Get);
        public SDU_Client.WebRequest Client_GetVAEs =>
            new SDU_Client.WebRequest(ServerUrl + "/stablediffunity/sd-vae", SDU_Client.Method.Get);
        #endregion

        #region Client Post
        public SDU_Client.WebRequest Client_SetVAE =>
            new SDU_Client.WebRequest(ServerUrl + "/stablediffunity/set-sd-vae", SDU_Client.Method.Post);

        public SDU_Client.WebRequest Client_PostGitClone => 
            new SDU_Client.WebRequest(ServerUrl + "/stablediffunity/git_clone", SDU_Client.Method.Post);
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
        public SDU_Client.WebRequest Client_ModelLists => new SDU_Client.WebRequest(URL_ModelLists, SDU_Client.Method.Get);
        public SDU_Client.WebRequest Client_GetVersion => new SDU_Client.WebRequest(ServerUrl + "/controlnet/version", SDU_Client.Method.Get);
        #endregion
    }
}