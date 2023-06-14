using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;
namespace SDU
{
    [System.Serializable]
    public class HideOnGUIData
    {
        public SDU_DownloadFileSetting m_DownloadFileSetting = new SDU_DownloadFileSetting();
        public SDU_CompressImageSetting m_CompressImageSetting = new SDU_CompressImageSetting();
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
        const string ConfigFilePathKey = "StableDiffusionPage.ConfigFilePath";
        public static string ConfigFilePath
        {
            get
            {
                if (!PlayerPrefs.HasKey(ConfigFilePathKey))
                {
                    PlayerPrefs.SetString(ConfigFilePathKey, DefaultConfigFilePath);
                }
                return PlayerPrefs.GetString(ConfigFilePathKey);
            }
            set
            {
                PlayerPrefs.SetString(ConfigFilePathKey, value);
            }
        }
        public static string DefaultConfigFilePath => Path.Combine(InstallSetting.DefaultInstallRoot, "Configs", "StableDiffusion.json");

        static RunTimeData s_RunTimeData = null;
        static public void ReloadRunTimeData()
        {
            s_RunTimeData = LoadRunTimeData();
        }
        static public RunTimeData LoadRunTimeData()
        {
            var aPath = ConfigFilePath;
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
            UCL.Core.FileLib.Lib.WriteAllText(ConfigFilePath, aJsonStr);
            //PlayerPrefs.SetString(RunTimeDataKey, aJsonStr);
        }
        static public void ConfigOnGUI(UCL_ObjectDictionary iDataDic)
        {
            using (var aScope = new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Save", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    RunTimeData.SaveRunTimeData();
                }
                if (File.Exists(RunTimeData.ConfigFilePath))
                {
                    if (GUILayout.Button("Load", UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                    {
                        RunTimeData.ReloadRunTimeData();
                    }
                }

                var aConfigFilePath = RunTimeData.ConfigFilePath;
                var aNewConfigFilePath = UCL_GUILayout.TextField("ConfigFilePath", aConfigFilePath);
                if (aNewConfigFilePath != aConfigFilePath)
                {
                    RunTimeData.ConfigFilePath = aNewConfigFilePath;
                }
            }

        }
        
        static public StableDiffusionAPI SD_API => Ins.m_APISetting.m_StableDiffusionAPI;
        static public StableDiffunityAPI Stablediffunity_API => Ins.m_APISetting.m_StablediffunityAPI;
        static public ControlNetAPI ControlNet_API => Ins.m_APISetting.m_ControlNetAPI;
        static public InstallSetting InstallSetting => Ins.m_InstallSetting;
        static public WebUISetting WebUISetting => Ins.m_WebUISetting;
        public static string ServerUrl => Ins.m_WebURL;
        #endregion

        public enum GenerateMode
        {
            Txt2Img,
            Img2Img,
        }

        public InstallSetting m_InstallSetting = new InstallSetting();
        public BootSetting m_BootSetting = new BootSetting();
        public ResolutionSetting m_ResolutionSetting = new ResolutionSetting();
        public APISetting m_APISetting = new APISetting();
        public WebUISetting m_WebUISetting = new WebUISetting();

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public GenerateMode m_GenerateMode = GenerateMode.Txt2Img;

        public SDU_ImgSetting CurImgSetting
        {
            get
            {
                switch (m_GenerateMode)
                {
                    case GenerateMode.Img2Img: return m_Img2ImgSetting;
                }
                return m_Txt2ImgSettings;
            }
        }

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public Txt2ImgSetting m_Txt2ImgSettings = new Txt2ImgSetting();

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public SDU_Img2ImgSetting m_Img2ImgSetting = new SDU_Img2ImgSetting();

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public HideOnGUIData m_HideOnGUIData = new HideOnGUIData();

        public bool m_RedirectStandardOutput = false;
        public bool m_AutoOpenWeb = true;
        public string m_WebURL = "http://127.0.0.1:7860";
        //[UCL.Core.ATTR.UCL_HideOnGUI] public int m_OutPutFileID = 0;

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("RunTimeData"), iFieldName, false);
            using (var aScope = new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Generate Mode", UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                m_GenerateMode = UCL_GUILayout.PopupAuto(m_GenerateMode, iDataDic.GetSubDic("GenerateMode"), "GenerateMode");
            }
                
            switch (m_GenerateMode)
            {
                case GenerateMode.Txt2Img:
                    {
                        UCL_GUILayout.DrawObjectData(m_Txt2ImgSettings, iDataDic.GetSubDic("Txt2Img"), "Txt2Img", false);
                        break;
                    }
                case GenerateMode.Img2Img:
                    {
                        UCL_GUILayout.DrawObjectData(m_Img2ImgSetting, iDataDic.GetSubDic("Img2Img"), "Img2Img", false);
                        break;
                    }
            }
            return this;
        }
    }
}