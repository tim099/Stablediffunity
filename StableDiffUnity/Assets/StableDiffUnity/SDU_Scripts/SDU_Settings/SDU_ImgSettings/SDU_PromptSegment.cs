using System.Collections;
using System.Collections.Generic;
using UCL.Core.JsonLib;
using UnityEngine;

namespace SDU
{
    public class SDU_PromptSegment : UnityJsonSerializable, UCL.Core.UCLI_ShortName
    {
        public string m_Prompt;
        public List<SDU_PromptSegment> m_SubGroups = new List<SDU_PromptSegment>();

        public string GetShortName() => Prompt.IsNullOrEmpty()? Prompt : Prompt.CutToMaxLength(30);
        public bool IsEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(m_Prompt) || m_SubGroups.IsNullOrEmpty())
                {
                    return false;
                }
                return true;
            }
        }
        public string Prompt {
            get
            {
                if (!m_SubGroups.IsNullOrEmpty())
                {
                    System.Text.StringBuilder aSB = new System.Text.StringBuilder();
                    bool aIsFirst = true;
                    foreach (SDU_PromptSegment aPromptSegment in m_SubGroups)
                    {
                        if (aIsFirst) aIsFirst = false;
                        else aSB.Append(',');
                        aSB.Append(aPromptSegment.Prompt);
                    }
                    if(!m_Prompt.IsNullOrEmpty())
                    {
                        aSB.Append(',');
                        aSB.Append(m_Prompt);
                    }
                    return $"({aSB.ToString()})";
                }
                return m_Prompt;
            }
        }
    }
}
