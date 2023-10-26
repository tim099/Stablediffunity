using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.UI;
using UnityEngine;

namespace SDU
{
    public class SDU_SceneControl : MonoBehaviour
    {
        public static SDU_SceneControl Ins => s_Ins;
        private static SDU_SceneControl s_Ins = null;

        public Camera m_Camera;
        public Animation m_RotateAnimation;
        public Animator m_RotateAnimator;
        public Transform m_UnityChanTransform;
        private void Awake()
        {
            s_Ins = this;
        }
        private void OnDestroy()
        {
            s_Ins = null;
        }

        public virtual void ContentOnGUI(UCL_ObjectDictionary iDataDic)
        {
            GUILayout.Space(20);

            using (var aScope = new GUILayout.HorizontalScope()) {


                m_RotateAnimation.enabled = UCL_GUILayout.CheckBox(m_RotateAnimation.enabled);
                GUILayout.Label("RotateAnimation", UCL_GUIStyle.LabelStyle);
                const float PauseSpeed = 0f;//Mathf.Epsilon
                if (Mathf.Approximately(Time.timeScale, PauseSpeed))
                {
                    if (GUILayout.Button("Resume", UCL_GUIStyle.ButtonStyle, GUILayout.Width(120)))
                    {
                        Time.timeScale = 1;
                        //m_RotateAnimator.speed = 1;
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop", UCL_GUIStyle.ButtonStyle, GUILayout.Width(120)))
                    {
                        Time.timeScale = PauseSpeed;
                        //m_RotateAnimator.speed = 0;
                    }
                }
                if (!m_RotateAnimation.enabled)
                {
                    using (var aScope2 = new GUILayout.HorizontalScope(GUILayout.Width(300)))
                    {
                        float aAngle = m_UnityChanTransform.eulerAngles.y;
                        float aNewAngle = UCL_GUILayout.Slider("Angle(Y)", aAngle, 0, 359.9f, iDataDic.GetSubDic("Angle"));
                        if (!Mathf.Approximately(aNewAngle, aAngle))
                        {
                            m_UnityChanTransform.localRotation = Quaternion.Euler(0, aNewAngle, 0);
                        }
                    }

                }
                {
                    var aPos = m_Camera.transform.localPosition;
                    using (var aScope2 = new GUILayout.HorizontalScope(GUILayout.Width(300)))
                    {
                        float aY = m_Camera.transform.localPosition.y;
                        float aNewVal = UCL_GUILayout.Slider("Camera(Y)", aY, -1f, 2f, iDataDic.GetSubDic("Camera_Y"));
                        if (!Mathf.Approximately(aNewVal, aY))
                        {
                            m_Camera.transform.localPosition = new Vector3(aPos.x, aNewVal, aPos.z);
                        }
                    }
                    using (var aScope2 = new GUILayout.HorizontalScope(GUILayout.Width(300)))
                    {
                        float aZ = m_Camera.transform.localPosition.z;
                        float aNewZ = UCL_GUILayout.Slider("Camera(Z)", aZ, -8.5f, -17f, iDataDic.GetSubDic("Camera_Z"));
                        if (!Mathf.Approximately(aNewZ, aZ))
                        {
                            m_Camera.transform.localPosition = new Vector3(aPos.x, aPos.y, aNewZ);
                        }
                    }
                }


                GUILayout.FlexibleSpace();
            }
            
            if (!UnityChan.IdleChanger.s_IdleChangers.IsNullOrEmpty())
            {
                
                GUILayout.Box($"Change UnityChan Motion");
                for (int i = 0; i < UnityChan.IdleChanger.s_IdleChangers.Count; i++)
                {
                    var aIdleChanger = UnityChan.IdleChanger.s_IdleChangers[i];

                    aIdleChanger.CustomOnGUI(i);
                }
            }
        }
    }
}
