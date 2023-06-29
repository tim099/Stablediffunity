using System.Collections;
using System.Collections.Generic;
using UCL.Core.JsonLib;
using UCL.Core.TextureLib;
using UnityEngine;

namespace SDU
{
    public static class SDU_TensorUtil
    {
        public static UCL_Texture2D TensorToTexture(JsonData iTensor)
        {
            int aHeight = iTensor[0].Count;
            int aWidth = iTensor[0][0].Count;
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
                        aArr[z] = iTensor[z][y][x];
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
                    for (int z = 0; z < 4; z++)
                    {
                        aArr[z] = iTensor[z][y][x];
                    }
                    Vector4 aVec = new Vector4(aArr[0], aArr[1], aArr[2], aArr[3]);

                    aVec *= aDiv;//Range -0.5f ~ 0.5f
                    aVec += 0.5f * Vector4.one;//Range

                    if (aVec.x < 0 || aVec.y < 0 || aVec.z < 0)
                    {
                        Debug.LogError("aVec:" + aVec.ToString());
                    }
                    aTexture.SetPixel(new Vector2Int(x, aHeight - y - 1), new Color(aVec.x, aVec.y, aVec.z, aVec.w));
                }
            }
            Debug.LogWarning($"TensorToTexture Max:{aMax},Min:{aMin}");
            return aTexture;
        }
    }
}