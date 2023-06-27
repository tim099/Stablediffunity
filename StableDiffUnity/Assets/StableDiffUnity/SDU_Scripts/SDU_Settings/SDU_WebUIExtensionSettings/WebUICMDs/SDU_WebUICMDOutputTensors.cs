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
        }
        public OutputTensorType m_OutputTensorType = OutputTensorType.EveryNSteps;

        /// <summary>
        /// output tensor every N steps, N = m_OutputStepInterval
        /// </summary>
        [UCL.Core.PA.UCL_IntSlider(1, 20)]
        public int m_OutputStepInterval = 1;

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public List<int> m_OutputAtSteps = new List<int>();
        public override JsonData SerializeToJson()
        {
            switch (m_OutputTensorType)
            {
                case OutputTensorType.EveryNSteps:
                    {
                        m_OutputAtSteps.Clear();
                        int aTotalSteps = RunTimeData.Ins.CurImgSetting.m_Steps;
                        for (int i = 0; i < aTotalSteps; i++)
                        {
                            if (i % m_OutputStepInterval == 0)
                            {
                                m_OutputAtSteps.Add(i);
                            }
                        }
                        break;
                    }
            }
            return base.SerializeToJson();
        }
    }
}