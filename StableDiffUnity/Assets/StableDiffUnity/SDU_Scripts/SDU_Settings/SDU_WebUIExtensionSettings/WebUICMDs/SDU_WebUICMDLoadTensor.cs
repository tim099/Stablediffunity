using System.Collections;
using System.Collections.Generic;
using System.IO;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.TextureLib;
using UCL.Core.UI;
using UnityEngine;
namespace SDU
{
    public class SDU_WebUICMDLoadTensor : SDU_WebUICMD
    {
        public string m_FolderPath;
        /// <summary>
        /// Load Tensor at which step,
        /// if m_LoadAtStep == -1 then output image immediately(skip the sample process)
        /// </summary>
        public int m_LoadAtStep = -1;

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_LoadTensorFileName = string.Empty;

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public string m_LoadJsonTensorName = string.Empty;

        public JsonData TensorJsonData { get; set; }
        public SDU_InputImage SDU_InputImage { get; set; }
        public override JsonData GetConfigJson()
        {
            CheckFolderPath();
            return base.GetConfigJson();
        }
        public override JsonData SerializeToJson()
        {
            return base.SerializeToJson();
        }
        private void CheckFolderPath()
        {
            if (string.IsNullOrEmpty(m_FolderPath))
            {
                m_FolderPath = System.IO.Path.Combine(RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting.OutputFolderPath, "tensors");
            }
        }
        public override object OnGUI(string iFieldName, UCL_ObjectDictionary iDataDic)
        {
            CheckFolderPath();
            UCL.Core.UI.UCL_GUILayout.DrawObjExSetting aDrawObjExSetting = new()
            {
                OnShowField = () =>
                {
                    string aPath = m_FolderPath;
                    {
                        IList<string> aFiles = new List<string>();
                        if (Directory.Exists(aPath))
                        {
                            aFiles = UCL.Core.FileLib.Lib.GetFilesName(aPath, "*.pt");
                        }
                        using (var aScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("LoadTensorFileName", UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                            m_LoadTensorFileName = UCL_GUILayout.PopupAuto(m_LoadTensorFileName, aFiles, iDataDic, "LoadTensorFileName");
                        }
                    }
                    GUILayout.Space(10);
                    
                    GUILayout.Label("====Testing Tensor====");
                    {
                        IList<string> aFiles = new List<string>();
                        if (Directory.Exists(aPath))
                        {
                            aFiles = UCL.Core.FileLib.Lib.GetFilesName(aPath, "*.json");
                        }
                        using (var aScope = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("LoadJsonTensorName", UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                            m_LoadJsonTensorName = UCL_GUILayout.PopupAuto(m_LoadJsonTensorName, aFiles, iDataDic, "LoadJsonTensorName");
                        }
                        if (!string.IsNullOrEmpty(m_LoadJsonTensorName))
                        {
                            string aJsonPath = Path.Combine(m_FolderPath, m_LoadJsonTensorName);
                            if (File.Exists(aJsonPath))
                            {
                                if (GUILayout.Button("Read Json"))
                                {
                                    string aJson = File.ReadAllText(aJsonPath);
                                    TensorJsonData = JsonData.ParseJson(aJson);
                                    if(SDU_InputImage == null)
                                    {
                                        SDU_InputImage = new SDU_InputImage();
                                    }
                                    JsonData aImageArr = TensorJsonData[0];

                                    int aHeight = aImageArr[0].Count;
                                    int aWidth = aImageArr[0][0].Count;

                                    SDU_InputImage.Clear();
                                    UCL_Texture2D aTexture = new UCL_Texture2D(aWidth, aHeight);
                                    float aMax = float.MinValue;
                                    float aMin = float.MaxValue;
                                    for (int x = 0; x < aWidth; x++)
                                    {
                                        for (int y = 0; y < aHeight; y++)
                                        {
                                            float[] aArr = new float[4];
                                            for (int z = 0; z < 4; z++)
                                            {
                                                aArr[z] = aImageArr[z][y][x];
                                                aMax = Mathf.Max(aMax, aArr[z]);
                                                aMin = Mathf.Min(aMin, aArr[z]);
                                            }
                                        }
                                    }
                                    float aAbsMax = Mathf.Max(Mathf.Abs(aMin), aMax);
                                    float aDiv = 0.5f / aAbsMax;
                                    for (int x = 0; x < aWidth; x++)
                                    {
                                        for (int y = 0; y < aHeight; y++)
                                        {
                                            float[] aArr = new float[4];
                                            for(int z = 0; z < 4; z++)
                                            {
                                                aArr[z] = aImageArr[z][y][x];
                                                aMax = Mathf.Max(aMax, aArr[z]);
                                                aMin = Mathf.Min(aMin, aArr[z]);
                                            }
                                            Vector4 aVec = new Vector4(aArr[0], aArr[1], aArr[2], aArr[3]);
                                            //aVec += 64f * Vector4.one;
                                            //aVec /= 128f;
                                            //aVec += aAbsMax * Vector4.one;
                                            aVec *= aDiv;//Range -0.5f ~ 0.5f
                                            aVec += 0.5f * Vector4.one;//Range
                                            //aVec.x = Mathf.Log(aVec.x, 2f) / 7.0f;
                                            //aVec.y = Mathf.Log(aVec.y, 2f) / 7.0f;
                                            //aVec.z = Mathf.Log(aVec.z, 2f) / 7.0f;

                                            //aVec *= (0.5f + aVec.w);
                                            //aVec.Scale(aVec);
                                            if (aVec.x < 0 || aVec.y < 0 || aVec.z < 0)
                                            {
                                                Debug.LogError("aVec:" + aVec.ToString());
                                            }
                                            aTexture.SetPixel(new Vector2Int(x, aHeight - y - 1), new Color(aVec.x, aVec.y, aVec.z, aVec.w));
                                        }
                                    }
                                    Debug.LogWarning($"aMax:{aMax},aMin:{aMin}");
                                    //UCL_Texture2D aTexture = new UCL_Texture2D(2 * aWidth, 2 * aHeight);
                                    //for (int x = 0; x < aWidth; x++)
                                    //{
                                    //    for (int y = 0; y < aHeight; y++)
                                    //    {
                                    //        float aVal = aImageArrX[x][y];
                                    //        aVal += 64f;
                                    //        aVal /= 128f;
                                    //        aTexture.SetPixel(new Vector2Int(x, y), new Color(aVal, aVal, aVal, 1));
                                    //    }
                                    //}
                                    //for (int x = 0; x < aWidth; x++)
                                    //{
                                    //    for (int y = 0; y < aHeight; y++)
                                    //    {
                                    //        float aVal = aImageArrY[x][y];
                                    //        aVal += 64f;
                                    //        aVal /= 128f;
                                    //        aTexture.SetPixel(new Vector2Int(x+aWidth, y), new Color(aVal, aVal, aVal, 1));
                                    //    }
                                    //}
                                    //for (int x = 0; x < aWidth; x++)
                                    //{
                                    //    for (int y = 0; y < aHeight; y++)
                                    //    {
                                    //        float aVal = aImageArrZ[x][y];
                                    //        aVal += 64f;
                                    //        aVal /= 128f;
                                    //        aTexture.SetPixel(new Vector2Int(x + aWidth, y + aHeight), new Color(aVal, aVal, aVal, 1));
                                    //    }
                                    //}
                                    //for (int x = 0; x < aWidth; x++)
                                    //{
                                    //    for (int y = 0; y < aHeight; y++)
                                    //    {
                                    //        float aVal = aImageArrW[x][y];
                                    //        aVal += 64f;
                                    //        aVal /= 128f;
                                    //        aTexture.SetPixel(new Vector2Int(x, y + aHeight), new Color(aVal, aVal, aVal, 1));
                                    //    }
                                    //}
                                    SDU_InputImage.Texture = aTexture.GetTexture();
                                }
                            }
                        }
                        if(TensorJsonData != null)
                        {
                            UCL_GUILayout.DrawObjectData(TensorJsonData, iDataDic.GetSubDic("TensorJsonData"), "TensorJsonData");
                        }
                        if(SDU_InputImage != null)
                        {
                            UCL_GUILayout.DrawObjectData(SDU_InputImage, iDataDic.GetSubDic("SDU_InputImage"), "SDU_InputImage");
                        }
                    }
                }
            };
            UCL.Core.UI.UCL_GUILayout.DrawField(this, iDataDic, iFieldName, iDrawObjExSetting: aDrawObjExSetting);

            
            return this;
        }
    }
}