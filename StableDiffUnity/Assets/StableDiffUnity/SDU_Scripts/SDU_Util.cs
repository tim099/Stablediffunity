using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SDU
{
    public static class SDU_Util
    {
        public static Vector2Int GetTextureSize(int iMaxSize, int iWidth, int iHeight)
        {
            if (iWidth <= 0)
            {
                Debug.LogError("GetSize iWidth <= 0");
                return new Vector2Int(iMaxSize, iMaxSize);
            }
            int aWidth = iMaxSize;
            int aHeight = iMaxSize;
            float aRatio = iHeight / (float)iWidth;
            if (aRatio > 1)
            {
                aWidth = Mathf.RoundToInt(iMaxSize / aRatio);
            }
            else if (aRatio < 1)
            {
                aHeight = Mathf.RoundToInt(iMaxSize * aRatio);
            }
            return new Vector2Int(aWidth, aHeight);
        }
        public static Vector2Int GetTextureSize(int iMaxSize, Texture iTexture)
        {
            return GetTextureSize(iMaxSize, iTexture.width, iTexture.height);
        }
    }
}