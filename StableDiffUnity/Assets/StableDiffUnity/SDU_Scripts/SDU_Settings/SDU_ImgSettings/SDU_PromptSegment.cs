using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UCL.Core;
using UCL.Core.JsonLib;
using UCL.Core.UI;
using UnityEngine;

namespace SDU
{
    public class SDU_PromptSegment : UnityJsonSerializable, UCL.Core.UCLI_NameOnGUI//, UCL.Core.UCLI_ShortName
    {
        public static string TypeName
        {
            get
            {
                if (string.IsNullOrEmpty(s_TypeName))
                {
                    s_TypeName = typeof(SDU_PromptSegment).Name;
                }
                return s_TypeName;
            }
        }
        public static string s_TypeName = null;
        public enum Mode
        {
            Prompt,
            Groups,
            Loras,
        }
        [UCL.Core.ATTR.UCL_HideOnGUI]
        public bool m_IsEnable = true;

        [UCL.Core.ATTR.UCL_HideOnGUI]
        public Mode m_Mode;

        [UCL.Core.PA.Conditional("m_Mode",false, Mode.Prompt)]
        public string m_Prompt;
        [UCL.Core.PA.Conditional("m_Mode", false, Mode.Groups)]
        public bool m_ShowBrackets = true;
        [UCL.Core.PA.Conditional("m_Mode", false, Mode.Groups)]
        public List<SDU_PromptSegment> m_SubGroups = new List<SDU_PromptSegment>();

        [UCL.Core.PA.Conditional("m_Mode", false, Mode.Loras)]
        public List<SDU_LoraSetting> m_Loras = new List<SDU_LoraSetting>();

        public bool IsEnable => m_IsEnable;
        public bool IsEmpty
        {
            get
            {
                switch (m_Mode)
                {
                    case Mode.Prompt: return string.IsNullOrEmpty(m_Prompt);
                    case Mode.Groups:
                        {
                            if (m_SubGroups.IsNullOrEmpty())
                            {
                                return true;
                            }
                            foreach(var subGroup in m_SubGroups)
                            {
                                if (!subGroup.IsEmpty) return false;
                            }
                            return true;
                        }
                    case Mode.Loras:
                        {
                            if (m_SubGroups.IsNullOrEmpty())
                            {
                                return true;
                            }
                            foreach (var lora in m_Loras)
                            {
                                if (!lora.IsEnable) return false;
                            }
                            return true;
                        }
                }

                return true;
            }
        }
        public string GetShortName()
        {
            switch (m_Mode)
            {
                case Mode.Prompt:
                    {
                        return "Prompt";
                    }
                case Mode.Groups:
                case Mode.Loras:
                    {
                        return Prompt.IsNullOrEmpty() ? Prompt : Prompt.CutToMaxLength(90);
                    }

            }
            return m_Mode.ToString();
        }

        public string Prompt {
            get
            {
                switch (m_Mode)
                {
                    case Mode.Prompt:
                        {
                            return m_Prompt;
                        }
                    case Mode.Groups:
                        {
                            if (!m_SubGroups.IsNullOrEmpty())
                            {
                                System.Text.StringBuilder aSB = new System.Text.StringBuilder();
                                bool aIsFirst = true;
                                foreach (var aSubGroup in m_SubGroups)
                                {
                                    if (aSubGroup.IsEnable)
                                    {
                                        if (aIsFirst) aIsFirst = false;
                                        else aSB.Append(',');
                                        aSB.Append(aSubGroup.Prompt);
                                    }
                                }
                                if (m_ShowBrackets)
                                {
                                    return $"({aSB.ToString()})";
                                }
                                return aSB.ToString();
                            }
                            return string.Empty;
                        }
                    case Mode.Loras:
                        {
                            if (!m_Loras.IsNullOrEmpty())
                            {
                                System.Text.StringBuilder aSB = new System.Text.StringBuilder();
                                bool aIsFirst = true;
                                foreach (var aLora in m_Loras)
                                {
                                    if (aLora.IsEnable)
                                    {
                                        if (aIsFirst) aIsFirst = false;
                                        else aSB.Append(',');
                                        aSB.Append(aLora.Prompt);
                                    }
                                }
                                return aSB.ToString();
                            }
                            return string.Empty;
                        }
                }

                return string.Empty;
            }
        }
        public void NameOnGUI(UCL_ObjectDictionary iDataDic, string iDisplayName)
        {
            using (var aScope2 = new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                if (iDisplayName != TypeName)//Root
                {
                    GUILayout.Label(iDisplayName, UCL_GUIStyle.LabelStyle);
                    m_IsEnable = true;
                    m_Mode = Mode.Groups;
                }
                else
                {
                    m_IsEnable = UCL_GUILayout.CheckBox(m_IsEnable);
                    m_Mode = UCL_GUILayout.PopupAuto(m_Mode, iDataDic, "Mode", 6, GUILayout.MinWidth(100));
                }


                switch (m_Mode)
                {
                    case Mode.Prompt:
                        {
                            m_Prompt = GUILayout.TextField(m_Prompt, GUILayout.MinWidth(400));
                            break;
                        }
                    case Mode.Loras:
                        {
                            if (GUILayout.Button("Refresh", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                            {
                                RunTimeData.Ins.m_WebUISetting.RefreshLora().Forget();
                            }
                            if (GUILayout.Button("Open Folder", UCL.Core.UI.UCL_GUIStyle.ButtonStyle, GUILayout.ExpandWidth(false)))
                            {
                                RunTimeData.InstallSetting.OpenFolder(FolderEnum.Lora);
                            }
                            GUILayout.Label(GetShortName(), UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));

                            break;
                        }
                    default:
                        {
                            GUILayout.Label(GetShortName(), UCL_GUIStyle.LabelStyle, GUILayout.ExpandWidth(false));
                            break;
                        }
                }
            }
        }
    }
}
