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
        static public bool IsCancellationRequested => s_CancellationTokenSource == null || s_CancellationTokenSource.IsCancellationRequested;
        static public bool TriggeringCMD => s_TriggeringCMD;
        static bool s_RequireClearDic = false;
        static bool s_TriggeringCMD = false;
        static int s_TriggerCMDAt = 0;
        static List<SDU_CMD> s_CMDs = null;
        static CancellationTokenSource s_CancellationTokenSource;

        public static async Task TriggerCMDs(SDU_ImgSetting iTarget, List<SDU_CMD> iCMDs)
        {
            if (s_TriggeringCMD) return;
            if(s_CancellationTokenSource != null)
            {
                CancelCMDs();
            }

            s_CancellationTokenSource = new CancellationTokenSource();
            s_TriggeringCMD = true;
            s_TriggerCMDAt = 0;
            s_CMDs = iCMDs;//.Clone();
            CancellationToken aCancellationToken = s_CancellationTokenSource.Token;
            for (int i = 0; i < s_CMDs.Count; i++)
            {
                s_TriggerCMDAt = i;
                if (IsCancellationRequested)
                {
                    break;
                }
                var aCMD = s_CMDs[i];
                await aCMD.TriggerCMD(iTarget, aCancellationToken);
            }
            CancelCMDs();
            s_TriggeringCMD = false;
        }
        public static void CancelCMDs()
        {
            if(s_CancellationTokenSource == null) return;
            if (!s_CancellationTokenSource.IsCancellationRequested)
            {
                s_CancellationTokenSource.Cancel();
            }
            s_CancellationTokenSource.Dispose();
            s_CancellationTokenSource = null;
            s_RequireClearDic = true;
        }
        public static void OnGUI(UCL_ObjectDictionary iDataDic, bool iShowCMDs = true)
        {
            if (s_RequireClearDic)
            {
                s_RequireClearDic = false;
                iDataDic.Clear();
            }
            if (s_TriggeringCMD)
            {
                using (var aScope = new GUILayout.HorizontalScope("box"))
                {
                    GUILayout.Label($"Triggering CMD[{s_TriggerCMDAt + 1}/{s_CMDs.Count}]",
                        UCL_GUIStyle.LabelStyle, GUILayout.Width(200));
                    if (!IsCancellationRequested)
                    {
                        if (GUILayout.Button("Cancel CMDs", UCL_GUIStyle.ButtonStyle))
                        {
                            CancelCMDs();
                        }
                    }
                    else
                    {
                        GUILayout.Label($"Canceling CMDs", UCL_GUIStyle.LabelStyle);
                    }
                }
                if (iShowCMDs)
                {
                    var aSubDic = iDataDic.GetSubDic("s_CMDs");
                    if (aSubDic.GetData(UCL_GUILayout.IsShowFieldKey, false))
                    {
                        UCL_GUILayout.DrawList(s_CMDs, aSubDic, "Triggering CMDs",
                           iOverrideDrawElement: (iDrawElement) =>
                           {
                               if (iDrawElement != null)
                               {
                                   Vector2 aScrollPos = iDataDic.GetData("ScrollPos", Vector2.zero);
                                   using (var aScope = new GUILayout.ScrollViewScope(aScrollPos, GUILayout.MaxHeight(300)))
                                   {
                                       iDataDic.SetData("ScrollPos", aScope.scrollPosition);
                                       iDrawElement.Invoke();
                                   }
                               }
                           });
                    }
                    else
                    {
                        UCL_GUILayout.DrawList(s_CMDs, aSubDic, "Triggering CMDs", false);
                    }

                }
            }
        }
    }
}