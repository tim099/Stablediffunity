using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UCL.Core;
using UCL.Core.UI;
using UnityEngine;



namespace SDU
{
    public static class SDU_CMDService
    {
        static public bool TriggeringCMD => s_TriggeringCMD;
        static bool s_TriggeringCMD = false;
        static int s_TriggerCMDAt = 0;
        static List<SDU_CMD> s_CMDs = null;
        static CancellationTokenSource s_CancellationTokenSource;
        public static async Task TriggerCMDs(SDU_ImgSetting iTarget, List<SDU_CMD> iCMDs, CancellationTokenSource iCancellationTokenSource)
        {
            if (s_TriggeringCMD) return;
            s_CancellationTokenSource = iCancellationTokenSource;
            s_TriggeringCMD = true;
            s_TriggerCMDAt = 0;
            s_CMDs = iCMDs;//.Clone();
            CancellationToken aCancellationToken = s_CancellationTokenSource.Token;
            for (int i = 0; i < s_CMDs.Count; i++)
            {
                s_TriggerCMDAt = i;
                if (s_CancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }
                var aCMD = s_CMDs[i];
                await aCMD.TriggerCMD(iTarget, aCancellationToken);
            }
            s_CancellationTokenSource = null;
            s_TriggeringCMD = false;
        }
        public static void OnGUI(UCL_ObjectDictionary iDataDic)
        {
            if (s_TriggeringCMD)
            {
                using(var aScope = new GUILayout.HorizontalScope("box"))
                {
                    GUILayout.Label($"Triggering CMD[{s_TriggerCMDAt+1}/{s_CMDs.Count}]",
                        UCL_GUIStyle.LabelStyle, GUILayout.Width(200));
                    if (!s_CancellationTokenSource.IsCancellationRequested)
                    {
                        if (GUILayout.Button("Cancel CMDs", UCL_GUIStyle.ButtonStyle))
                        {
                            s_CancellationTokenSource.Cancel();
                        }
                    }
                    else
                    {
                        GUILayout.Label($"Canceling CMDs", UCL_GUIStyle.LabelStyle);
                    }
                }

            }
        }
    }
}