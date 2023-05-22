using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SDU
{
    public class APISetting
    {
        public ControlNetAPI m_ControlNetAPI = new ControlNetAPI();
        public StableDiffusionAPI m_StableDiffusionAPI = new StableDiffusionAPI();
    }
    [System.Serializable]
    public class StableDiffusionAPI
    {
        public static string ServerUrl => SDU_StableDiffusionPage.ServerUrl;

        public string m_ApiSdModels = "/sdapi/v1/sd-models";
        public string m_ApiCmdFlags = "/sdapi/v1/cmd-flags";
        public string m_ApiOptions = "/sdapi/v1/options";
        public string m_ApiTxt2img = "/sdapi/v1/txt2img";
        public string m_ApiPngInfo = "/sdapi/v1/png-info";
        public string m_ControlNetTxt2img = "/controlnet/txt2img";


        public string URL_SdModels => ServerUrl + m_ApiSdModels;
        public string URL_CmdFlags => ServerUrl + m_ApiCmdFlags;
        public string URL_Options => ServerUrl + m_ApiOptions;
        public string URL_Txt2img => ServerUrl + m_ApiTxt2img;
        public string URL_PngInfo => ServerUrl + m_ApiPngInfo;
        public string URL_ControlNetTxt2img => ServerUrl + m_ControlNetTxt2img;
    }

    /// <summary>
    /// https://github.com/Mikubill/sd-webui-controlnet/wiki/API
    /// </summary>
    [System.Serializable]
    public class ControlNetAPI
    {
        public static string ServerUrl => SDU_StableDiffusionPage.ServerUrl;

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
        #endregion
    }
}