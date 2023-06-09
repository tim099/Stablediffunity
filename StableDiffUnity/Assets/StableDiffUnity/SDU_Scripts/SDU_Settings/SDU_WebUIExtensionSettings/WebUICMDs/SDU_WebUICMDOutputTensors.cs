using System.Collections;
using System.Collections.Generic;
using UCL.Core.JsonLib;
using UnityEngine;


namespace SDU
{
    public class SDU_WebUICMDOutputTensors : SDU_WebUICMD
    {
        public enum OutputTensorType
        {
            /// <summary>
            /// output tensor every N steps
            /// </summary>
            EveryNSteps,
            /// <summary>
            /// output tensor at specify steps
            /// </summary>
            OutputAtSteps,
        }
        public OutputTensorType m_OutputTensorType = OutputTensorType.EveryNSteps;
        public string m_FolderPath;
        /// <summary>
        /// output tensor every N steps, N = m_OutputStepInterval
        /// </summary>
        [UCL.Core.PA.Conditional("m_OutputTensorType", false, OutputTensorType.EveryNSteps)]
        [UCL.Core.PA.UCL_IntSlider(1, 20)]
        public int m_OutputStepInterval = 1;

        [UCL.Core.PA.Conditional("m_OutputTensorType", false, OutputTensorType.OutputAtSteps)]
        public List<int> m_OutputAtSteps = new List<int>();

        public bool m_OutputJsonTensor = false;
        //[UCL.Core.ATTR.UCL_HideOnGUI]


        public override JsonData GetConfigJson()
        {
            if (string.IsNullOrEmpty(m_FolderPath))
            {
                m_FolderPath = System.IO.Path.Combine(RunTimeData.Ins.CurImgSetting.m_ImageOutputSetting.OutputFolderPath, "tensors");
            }
            switch (m_OutputTensorType)
            {
                case OutputTensorType.EveryNSteps:
                    {
                        m_OutputAtSteps.Clear();
                        int aTotalSteps = RunTimeData.Ins.CurImgSetting.m_Steps;
                        for (int i = 0; i <= aTotalSteps; i++)
                        {
                            if (i % m_OutputStepInterval == 0)
                            {
                                m_OutputAtSteps.Add(i);
                            }
                        }
                        break;
                    }
            }
            return base.GetConfigJson();
        }
        protected override JsonData GetConfigJsonData()
        {
            return JsonConvert.SaveFieldsToJsonUnityVer(this);
        }
        public override JsonData SerializeToJson()
        {
            switch (m_OutputTensorType)
            {
                case OutputTensorType.EveryNSteps:
                    {
                        m_OutputAtSteps.Clear();
                        break;
                    }
            }
            
            return base.SerializeToJson();
        }
    }
}