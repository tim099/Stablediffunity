using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UCL.Core.JsonLib;
using UCL.Core.EditorLib.Page;
using System.IO;
using UCL.Core.UI;
using UCL.Core;
using System;

namespace SDU
{
    public class SDU_CompressImageSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public static float s_CompressProgress = -1;
        public static bool Compresssing { get; private set; } = false;

        public string m_InputFolder = string.Empty;
        public string m_OutputFolder = string.Empty;
        public float m_DownScaleRate = 0.5f;
        

        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            var aDic = iDataDic.GetSubDic("SDU_CompressImageSetting");
            UCL.Core.UI.UCL_GUILayout.DrawField(this, aDic, iFieldName, false);
            if (aDic.GetData(UCL_GUILayout.IsShowFieldKey, false))
            {
                if (Compresssing)
                {
                    GUILayout.Label($"Compresssing Progress: {(100.0f*s_CompressProgress).ToString("0.0")} %");
                }
                else
                {
                    if (Directory.Exists(m_InputFolder))
                    {
                        if (GUILayout.Button("Compress Images"))
                        {
                            CompressImageAsync(m_InputFolder, m_OutputFolder, m_DownScaleRate).Forget();
                        }
                    }
                }


            }
            return this;
        }
        public static async UniTaskVoid CompressImageAsync(string iInputFolder, string iOutputFolder, float iDownScaleRate)
        {
            if (Compresssing)
            {
                return;
            }
            Compresssing = true;
            s_CompressProgress = 0f;
            if (!Directory.Exists(iInputFolder))
            {
                Debug.LogError($"CompressImageAsync() !Directory.Exists(m_InputFolder) m_InputFolder:{iInputFolder}");
                return;
            }
            if (!Directory.Exists(iOutputFolder))
            {
                UCL.Core.FileLib.Lib.CreateDirectory(iOutputFolder);
            }

            var aFiles = UCL.Core.FileLib.Lib.GetFilesName(iInputFolder, "*.png");
            List<UniTask> aTasks = new List<UniTask>();
            foreach (var aFileName in aFiles)
            {
                try
                {
                    var aPath = Path.Combine(iInputFolder, aFileName);
                    if (!File.Exists(aPath))
                    {
                        Debug.LogError($"LoadImage() File.Exists(aPath) aPath:{aPath}");
                        continue;
                    }
                    var aBytes = File.ReadAllBytes(aPath);
                    var aTexture = UCL.Core.TextureLib.Lib.CreateTexture(aBytes);
                    int aWidth = Mathf.RoundToInt(aTexture.width * iDownScaleRate);
                    int aHeight = Mathf.RoundToInt(aTexture.height * iDownScaleRate);
                    if (aWidth < 1) aWidth = 1;
                    if (aHeight < 1) aHeight = 1;

                    var aResizeTexture = aTexture.CreateResizeTexture(aWidth, aHeight);

                    var aOutputBytes = aResizeTexture.EncodeToPNG();
                    var aOutputPath = Path.Combine(iOutputFolder, aFileName);
                    aTasks.Add(File.WriteAllBytesAsync(aOutputPath, aOutputBytes).AsUniTask());
                    GameObject.DestroyImmediate(aTexture);
                    GameObject.DestroyImmediate(aResizeTexture);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            await UniTask.WhenAll(aTasks);

            s_CompressProgress = -1;
            Compresssing = false;
        }
    }
    public class SDU_CompressImagePage : UCL_EditorPage
    {
        static public SDU_CompressImagePage Create() => UCL_EditorPage.Create<SDU_CompressImagePage>();
        protected override bool ShowCloseButton => false;
        public override string WindowName => $"SDU Compress Image {SDU_EditorMenuPage.SDU_Version}";
        UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();
        SDU_CompressImageSetting CompressImageSetting => RunTimeData.Ins.m_HideOnGUIData.m_CompressImageSetting;
        public override void Init(UCL_GUIPageController iGUIPageController)
        {
            base.Init(iGUIPageController);
            SDU_FileInstall.CheckAndInstall(RunTimeData.InstallSetting);
        }
        protected override void ContentOnGUI()
        {
            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                UCL.Core.UI.UCL_GUILayout.DrawObjectData(CompressImageSetting, m_Dic.GetSubDic("CompressImageSetting"));
            }

        }
    }
}