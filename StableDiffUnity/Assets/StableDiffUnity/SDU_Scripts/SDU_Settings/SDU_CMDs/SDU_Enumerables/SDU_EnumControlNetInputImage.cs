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
        public string m_InputImagesFolder = string.Empty;
        override public string GetShortName()
        {
            string aName = base.GetShortName();
            return aName;
        }
        public override object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            if (string.IsNullOrEmpty(m_InputImagesFolder))
            {
                m_InputImagesFolder = SDU_ImageGenerator.DefaultImageOutputFolder();
            }
            //SDU_ImageGenerator.DefaultImageOutputFolder();
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, false);
            return this;
        }
        override public SDU_Enumerator GetEnumerator()
        {
            List<SDU_CMD> aCMDs = new List<SDU_CMD>();
            if (Directory.Exists(m_InputImagesFolder))
            {
                var aFiles = UCL.Core.FileLib.Lib.GetFilesName(m_InputImagesFolder,"*.png");
                if (!aFiles.IsNullOrEmpty())
                {
                    SDU_CMDControlNet aCMDControlNet = new SDU_CMDControlNet();
                    aCMDs.Add(aCMDControlNet);
                    foreach (var aFile in aFiles)
                    {
                        //string aFilePath = Path.Combine(m_InputImagesFolder, aFile);
                        SDU_ControlNetCMDSetInputImage aCMD = new SDU_ControlNetCMDSetInputImage();
                        aCMD.m_InputImage.m_LoadImageSetting.m_FileName = aFile;
                        aCMD.m_InputImage.m_LoadImageSetting.m_FolderPath = m_InputImagesFolder;
                        //aCMD.m_InputImage.LoadImage();
                        aCMDControlNet.m_ControlNetCMDs.Add(aCMD);
                    }
                }
            }
            else
            {
                Debug.LogError($"SDU_EnumControlNetInputImages InputImagesFolder Not Exist," +
                    $"InputImagesFolder:{m_InputImagesFolder}");
            }
            return new SDU_Enumerator(aCMDs);
        }
    }
}

