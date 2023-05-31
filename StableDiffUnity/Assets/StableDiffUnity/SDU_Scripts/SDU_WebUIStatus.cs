/*
AutoHeader Test
to change the auto header please go to RCG_AutoHeader.cs
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
namespace SDU
{
    public class SDU_WebUIStatus
    {
        public static SDU_WebUIStatus Ins
        {
            get
            {
                if (s_Ins == null) s_Ins = new SDU_WebUIStatus();
                return s_Ins;
            }
        }
        public static SDU_WebUIStatus s_Ins = null;
        public static bool ServerReady {
            get => s_ServerReady;
            set
            {
                //Debug.LogError($"Set ServerReady:{s_ServerReady}");
                s_ServerReady = value;
            }
        }
        public static bool s_ServerReady = false;

        public SDU_WebUIStatus()
        {
            s_Ins = this;
        }
        ~SDU_WebUIStatus()
        {
            if (s_Ins == this) s_Ins = null;
        }
        /// <summary>
        /// Get.AppId
        /// </summary>
        bool m_CheckEnabled = false;
        string m_ServerAppId;
        public string ServerAppId => m_ServerAppId;

        public async ValueTask<bool> ValidateConnection()
        {
            try
            {
                using(var aClient = RunTimeData.SD_API.Client_AppID)
                {
                    var aResult = await aClient.SendWebRequestAsync();
                    if (aResult.Contains("app_id"))
                    {
                        m_ServerAppId = aResult["app_id"].GetString();
                    }
                    else
                    {
                        m_ServerAppId = string.Empty;
                    }
                    //Debug.LogWarning($"ValidateConnection Result:{aResult}");
                }
                UnityEngine.Debug.LogWarning($"Check done. ServerAppId:{m_ServerAppId}");
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"Checking ... {e.Message}");
                //UnityEngine.Debug.LogException(e);
                return false;
            }
        }
        public async ValueTask<bool> CheckServerReady(System.Action<bool> iEndAct = null)
        {
            //ServerReady = false;
            try
            {
                ServerReady = await ValidateConnection();
            }
            finally
            {
                iEndAct?.Invoke(ServerReady);
                //Close();
            }
            return ServerReady;
        }
        public async ValueTask ValidateConnectionContinuously(System.Action<bool> iEndAct = null)
        {
            if (m_CheckEnabled)
            {
                UnityEngine.Debug.LogWarning($"Check already active.");
                return;
            }

            m_CheckEnabled = true;

            try
            {
                while (m_CheckEnabled)
                {
                    ServerReady = await ValidateConnection();
                    if (ServerReady)
                    {
                        //Debug.LogError($"s_ServerReady:{ServerReady}");
                        Close();
                        break;
                    }
                    await Task.Delay(1000);
                }
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                iEndAct?.Invoke(ServerReady);
                Close();
            }
        }


        public void Close()
        {
            //Debug.LogError("SDU_WebUIStatus Close()");
            m_CheckEnabled = false;
        }

    }
}