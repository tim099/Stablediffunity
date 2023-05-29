using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace SDU
{
    public class SDU_CMDForeach : SDU_CMD
    {
        public List<SDU_Enumerable> m_Enumerables = new List<SDU_Enumerable>();
        public List<SDU_CMD> m_CMDs = new List<SDU_CMD>();
        override public string GetShortName()
        {
            if (m_CMDs.IsNullOrEmpty()) return base.GetShortName();
            return $"Foreach({m_Enumerables.ConcatString((iCMD) => iCMD.GetShortName())})".CutToMaxLength(30) + $"[{m_CMDs.Count}]";
        }
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            if (m_CMDs.IsNullOrEmpty()) return;
            var aEnumerables = m_Enumerables.Clone();
            var aCMDs = m_CMDs.Clone();
            foreach (var aEnumerable in aEnumerables)
            {
                if (iCancellationToken.IsCancellationRequested) break;
                foreach (SDU_CMD aEnumCMD in aEnumerable)
                {
                    if (iCancellationToken.IsCancellationRequested) break;
                    try
                    {
                        await aEnumCMD.TriggerCMD(iTex2ImgSetting, iCancellationToken);
                        foreach (var aCMD in aCMDs)
                        {
                            if (iCancellationToken.IsCancellationRequested) break;
                            try
                            {
                                await aCMD.TriggerCMD(iTex2ImgSetting, iCancellationToken);
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                    }
                    catch(System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}