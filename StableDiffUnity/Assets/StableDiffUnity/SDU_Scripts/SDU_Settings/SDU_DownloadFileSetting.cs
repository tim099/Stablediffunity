using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UnityEngine;
using System.IO;


namespace SDU
{
    public class SDU_DownloadFileSetting : UCL.Core.JsonLib.UnityJsonSerializable, UCL.Core.UI.UCLI_FieldOnGUI
    {
        public enum FileType
        {
            CheckPoint,
            Lora,
        }
        public string m_URL;

        
        public string m_FileName = "NewFile";
        public string m_FileExtension = "safetensors";
        public FolderEnum m_Folder = FolderEnum.Lora;

        public string FolderPath => RunTimeData.InstallSetting.GetFolderPath(m_Folder);
        public string FilePath => Path.Combine( FolderPath, $"{m_FileName}.{m_FileExtension}");
        private float m_Progress = -1;
        public object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            using (var aScope = new GUILayout.VerticalScope("box"))
            {
                UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic.GetSubDic("DownloadFileSetting"), iFieldName, false);
                //if (m_Progress < 0)
                {
                    if (GUILayout.Button("Download"))
                    {
                        m_Progress = 0;
                        SDU_FileDownloader.DownloadFileAsync(m_URL, FilePath, (iProgress) => m_Progress = iProgress).Forget();
                    }
                }
                //else
                {
                    GUILayout.Label($"Progress:{m_Progress}");
                }
            }


            return this;
        }
    }
}