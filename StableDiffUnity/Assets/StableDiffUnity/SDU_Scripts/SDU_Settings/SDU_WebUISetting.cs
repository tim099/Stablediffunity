using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDU
{
    [System.Serializable]
    public class WebUISetting
    {
        [System.Serializable]
        public class ControlNetData
        {
            public List<string> m_ModelList = new List<string>();
        }

        public ControlNetData m_ControlNetData = new ControlNetData();

        public List<string> m_ModelNames = new List<string>();
        public List<string> m_LoraNames = new List<string>();
        public List<string> m_Samplers = new List<string>
            {
                "Euler a",
                "Euler",
                "LMS",
                "Heun",
                "DPM2",
                "DPM2 a",
                "DPM++ 2S a",
                "DPM++ 2M",
                "DPM++ SDE",
                "DPM fast",
                "DPM adaptive",
                "LMS Karras",
                "DPM2 Karras",
                "DPM2 a Karras",
                "DPM++ 2S a Karras",
                "DPM++ 2M Karras",
                "DPM++ SDE Karras",
                "DDIM",
                "PLMS"
            };

        public List<SDU_WebUIClient.Get.SdApi.V1.SdModels.Responses> m_Models = new();
        public SDU_WebUIClient.Get.SdApi.V1.CmdFlags.Responses m_CmdFlags = new();
    }
}