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
    public interface IWebUIStatus
    {
        public interface IArgs
        {
            bool ServerStarted { get; }
            string ServerAppId { get; }
            bool ServerReady { get; }
        }

        public bool ServerStarted { get; }
        public string ServerAppId { get; }
        public bool ServerReady { get; }

        public event EventHandler<IArgs> Changed;
    }
    public class SDU_Args : EventArgs, IWebUIStatus.IArgs
    {
        public bool ServerStarted { get; set; } = false;
        public string ServerAppId { get; set; } = string.Empty;
        public bool ServerReady { get; set; } = false;
    }
    public class SDU_WebUIStatus
    {
        public static SDU_WebUIStatus Ins
        {
            get
            {
                if (s_Ins == null) s_Ins = new SDU_WebUIStatus(IPAddress.Any, 50007);
                return s_Ins;
            }
        }
        public static SDU_WebUIStatus s_Ins = null;
        public static bool s_ServerReady = false;



        public SDU_WebUIStatus(IPAddress adress, int port)
        {
            s_Ins = this;
            s_EndPoint = new IPEndPoint(adress, port);
        }
        ~SDU_WebUIStatus()
        {
            if (s_Ins == this) s_Ins = null;
        }
        /// <summary>
        /// Get.AppId
        /// </summary>
        bool m_CheckEnabled = false;
        private SDU_Args m_Args = new();
        public event EventHandler<IWebUIStatus.IArgs> Changed;


        public bool ServerStarted => m_Args.ServerStarted;
        public string ServerAppId => m_Args.ServerAppId;
        public bool ServerReady => m_Args.ServerReady;

        

        

        private void InvokeWebUIStatusChanged()
        {
            Debug.LogWarning($"InvokeWebUIStatusChanged ServerStarted:{m_Args.ServerStarted},ServerAppId:{m_Args.ServerAppId}" +
                $",ServerReady:{m_Args.ServerReady}");
            if (Changed != null) 
            {
                Changed.Invoke(this, m_Args); 
            }
        }

        public void SetServerStarted(bool started)
        {
            m_Args.ServerStarted = started;
            InvokeWebUIStatusChanged();
        }

        public void SetServerAppId(string id)
        {
            m_Args.ServerAppId = id;
            InvokeWebUIStatusChanged();
        }

        public void SetServerReady(bool ready)
        {
            m_Args.ServerReady = ready;
            InvokeWebUIStatusChanged();
        }

        public void SetServerArgs(bool started, string id, bool ready)
        {
            m_Args.ServerStarted = started;
            m_Args.ServerAppId = id;
            m_Args.ServerReady = ready;
            InvokeWebUIStatusChanged();
        }

        public void ResetServerArgs()
        {
            m_Args.ServerStarted = false;
            m_Args.ServerAppId = string.Empty;
            m_Args.ServerReady = false;
            InvokeWebUIStatusChanged();
        }



        public async ValueTask<bool> ValidateConnection()
        {
            try
            {
                var aGetAppID = new SDU_WebUIClient.Get.AppId();
                var aResult = await aGetAppID.SendRequestAsync();

                if (Regex.IsMatch(aResult.app_id, "^[0-9]+$"))
                {
                    SetServerArgs(ServerStarted, aResult.app_id, true);
                    UnityEngine.Debug.LogWarning($"Server AppId : {ServerAppId}");
                }
                else
                {
                    SetServerArgs(ServerStarted, string.Empty, true);
                }

                UnityEngine.Debug.LogWarning($"Check done.");
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"Checking ... {e.Message}");
                return false;
            }
        }

        public async ValueTask ValidateConnectionContinuously(System.Action iEndAct = null)
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
                    if (await ValidateConnection())
                    {
                        Close();
                    }
                    else
                    {
                        await Task.Delay(3000);
                    }
                }
            }
            finally
            {
                s_ServerReady = true;
                iEndAct?.Invoke();
                Close();
            }
        }

        // UDP Client
        private static IPEndPoint s_EndPoint;
        private UdpClient _client;
        private bool _receiveEnabled = false;

        public async ValueTask ConnectUdp()
        {
            if (_receiveEnabled)
            {
                UnityEngine.Debug.Log($"UdpClient already receiving.");
                return;
            }

            try
            {
                _client = new(s_EndPoint);
                _receiveEnabled = true;

                while (_receiveEnabled)
                {
                    var result = await _client.ReceiveAsync();

                    var str = Encoding.UTF8.GetString(result.Buffer);

                    if (Regex.IsMatch(str, "^[0-9]+$"))
                    {
                        SetServerAppId(str);
                        UnityEngine.Debug.Log($"Server PID : {ServerAppId}");
                    }
                    else
                    {
                        switch (str)
                        {
                            case "start":
                                SetServerStarted(true);
                                UnityEngine.Debug.Log("Server starting.");
                                break;

                            case "ready":
                                Close();
                                SetServerReady(true);
                                UnityEngine.Debug.Log("Server ready.");
                                break;
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                UnityEngine.Debug.LogWarning("UdpClient closed.");
            }
            finally
            {
                Close();
            }
        }

        public void Close()
        {
            m_CheckEnabled = false;
            _receiveEnabled = false;

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

    }
}