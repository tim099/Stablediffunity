using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UCL.Core;
using UnityEngine;


namespace SDU
{
    public class SDU_EnumControlNetInputImages : SDU_Enumerable
    {
        public class InputImagesSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
        {
            public int m_TargetControlNetID = 0;
            public string m_InputImagesFolder = string.Empty;
            public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
            {
                if (string.IsNullOrEmpty(m_InputImagesFolder))
                {
                    m_InputImagesFolder = SDU_ImageGenerator.DefaultImageOutputFolder();
                }
                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
                return this;
            }
        }

        //public string m_InputImagesFolder = string.Empty;
        public List<InputImagesSetting> m_InputImagesSettings = new List<InputImagesSetting>();
        override public string GetShortName()
        {
            string aName = base.GetShortName();
            return aName;
        }
        public override object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            //SDU_ImageGenerator.DefaultImageOutputFolder();
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
            return this;
        }
        override public SDU_Enumerator GetEnumerator()
        {
            List<SDU_CMD> aCMDs = new List<SDU_CMD>();
            List<string[]> aFileNames = new List<string[]>();
            int aMaxFileCount = 0;
            foreach (var aSetting in m_InputImagesSettings)
            {
                string aInputImagesFolder = aSetting.m_InputImagesFolder;
                string[] aFileNameArr = null;
                if (Directory.Exists(aInputImagesFolder))
                {
                    aFileNameArr = UCL.Core.FileLib.Lib.GetFilesName(aInputImagesFolder, "*.png");
                    aMaxFileCount = System.Math.Max(aFileNameArr.Length, aMaxFileCount);
                }
                else
                {
                    Debug.LogError($"SDU_EnumControlNetInputImages InputImagesFolder Not Exist," +
                        $"InputImagesFolder:{aInputImagesFolder}");
                }
                aFileNames.Add(aFileNameArr);
            }
            Debug.LogWarning($"SDU_EnumControlNetInputImages aMaxFileCount:{aMaxFileCount}");
            for (int aFileID = 0; aFileID < aMaxFileCount; aFileID++)
            {
                SDU_CMDGroup aGroup = new SDU_CMDGroup();
                aGroup.UnpackToCMDList = false;
                aCMDs.Add(aGroup);
                for (int aSettingID = 0; aSettingID < m_InputImagesSettings.Count; aSettingID++)
                {
                    var aSetting = m_InputImagesSettings[aSettingID];
                    string aInputImagesFolder = aSetting.m_InputImagesFolder;
                    var aFiles = aFileNames[aSettingID];
                    if (aFiles.IsNullOrEmpty())
                    {
                        Debug.LogError($"EnumControlNetInputImages aFiles.IsNullOrEmpty() aInputImagesFolder:{aInputImagesFolder}");
                        continue;
                    }
                    if(aFileID >= aFiles.Length)
                    {
                        Debug.LogError($"EnumControlNetInputImages aFileID:{aFileID},aFiles.Length:{aFiles.Length},aInputImagesFolder:{aInputImagesFolder}");
                        continue;
                    }
                    SDU_CMDControlNet aCMDControlNet = new SDU_CMDControlNet();
                    aCMDControlNet.m_TargetControlNetID = aSetting.m_TargetControlNetID;
                    aGroup.m_CMDs.Add(aCMDControlNet);
                    var aFile = aFiles[aFileID];

                    SDU_ControlNetCMDSetInputImage aCMD = new SDU_ControlNetCMDSetInputImage();
                    aCMD.m_InputImage.m_LoadImageSetting.m_FileName = aFile;
                    aCMD.m_InputImage.m_LoadImageSetting.m_FolderPath = aInputImagesFolder;
                    aCMDControlNet.m_ControlNetCMDs.Add(aCMD);
                }
            }
            //for (int i = 0; i < m_InputImagesSettings.Count; i++)
            //var aSetting = m_InputImagesSettings[i];

            //foreach (var aSetting in m_InputImagesSettings)
            //{
            //    string aInputImagesFolder = aSetting.m_InputImagesFolder;
            //    if (Directory.Exists(aInputImagesFolder))
            //    {
            //        var aFiles = UCL.Core.FileLib.Lib.GetFilesName(aInputImagesFolder, "*.png");
            //        if (!aFiles.IsNullOrEmpty())
            //        {
            //            SDU_CMDControlNet aCMDControlNet = new SDU_CMDControlNet();
            //            aCMDControlNet.m_TargetControlNetID = aSetting.m_TargetControlNetID;
            //            aCMDs.Add(aCMDControlNet);
            //            foreach (var aFile in aFiles)
            //            {
            //                //string aFilePath = Path.Combine(m_InputImagesFolder, aFile);
            //                SDU_ControlNetCMDSetInputImage aCMD = new SDU_ControlNetCMDSetInputImage();
            //                aCMD.m_InputImage.m_LoadImageSetting.m_FileName = aFile;
            //                aCMD.m_InputImage.m_LoadImageSetting.m_FolderPath = aInputImagesFolder;
            //                //aCMD.m_InputImage.LoadImage();
            //                aCMDControlNet.m_ControlNetCMDs.Add(aCMD);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Debug.LogError($"SDU_EnumControlNetInputImages InputImagesFolder Not Exist," +
            //            $"InputImagesFolder:{aInputImagesFolder}");
            //    }
            //}

            return new SDU_Enumerator(aCMDs);
        }
    }
}

