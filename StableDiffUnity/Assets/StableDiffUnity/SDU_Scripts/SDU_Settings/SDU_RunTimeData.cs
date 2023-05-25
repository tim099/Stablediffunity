using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
using UnityEngine;
namespace SDU
{
    [System.Serializable]
    public class Tex2ImgResults
    {
        public Dictionary<string, string> m_Infos = new Dictionary<string, string>();
    }
    [System.Serializable]
    public class RunTimeData : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        #region static
        static public RunTimeData Ins
        {
            get
            {
                if (s_RunTimeData == null)
                {
                    ReloadRunTimeData();
                }
                return s_RunTimeData;
            }
        }
        static RunTimeData s_RunTimeData = null;
        static public void ReloadRunTimeData()
        {
            s_RunTimeData = LoadRunTimeData();
        }
        static public RunTimeData LoadRunTimeData()
        {
            var aPath = SDU_StableDiffusionPage.ConfigFilePath;
            if (File.Exists(aPath))
            {
                try
                {
                    string aJsonStr = File.ReadAllText(aPath);//PlayerPrefs.GetString(RunTimeDataKey);
                    JsonData aJson = JsonData.ParseJson(aJsonStr);
                    var aRunTimeData = JsonConvert.LoadDataFromJsonUnityVer<RunTimeData>(aJson);
                    if (aRunTimeData != null) return aRunTimeData;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return new RunTimeData();
        }
        static public void SaveRunTimeData()
        {
            string aJsonStr = UCL.Core.JsonLib.JsonConvert.SaveDataToJsonUnityVer(Ins).ToJsonBeautify();
            UCL.Core.FileLib.Lib.WriteAllText(SDU_StableDiffusionPage.ConfigFilePath, aJsonStr);
            //PlayerPrefs.SetString(RunTimeDataKey, aJsonStr);
        }
        static public StableDiffusionAPI SD_API => Ins.m_APISetting.m_StableDiffusionAPI;
        static public ControlNetAPI ControlNet_API => Ins.m_APISetting.m_ControlNetAPI;
        static public InstallSetting InstallSetting => Ins.m_InstallSetting;
        #endregion

        public InstallSetting m_InstallSetting = new InstallSetting();
        public ResolutionSetting m_ResolutionSetting = new ResolutionSetting();
        public APISetting m_APISetting = new APISetting();
        public WebUISetting m_WebUISetting = new WebUISetting();


        [UCL.Core.ATTR.UCL_HideOnGUI]
        public Tex2ImgSetting m_Tex2ImgSettings = new Tex2ImgSetting();

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public Tex2ImgResults m_Tex2ImgResults = new Tex2ImgResults();

        public bool m_RedirectStandardOutput = false;
        public bool m_AutoOpenWeb = true;
        public string m_WebURL = "http://127.0.0.1:7860";
        [UCL.Core.ATTR.UCL_HideOnGUI] public int m_OutPutFileID = 0;

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("RunTimeData"), iFieldName, false);
            UCL.Core.UI.UCL_GUILayout.DrawObjectData(m_Tex2ImgSettings, iDataDic.GetSubDic("Tex2Img"), "Tex2Img", false);
            return this;
        }
    }
}