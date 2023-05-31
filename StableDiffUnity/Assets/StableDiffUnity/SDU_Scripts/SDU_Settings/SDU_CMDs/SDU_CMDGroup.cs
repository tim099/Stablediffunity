using System.Collections.Generic;
using System.Threading.Tasks;
namespace SDU
{
    public class SDU_CMDGroup : SDU_CMD
    {
        public List<SDU_CMD> m_CMDs = new List<SDU_CMD>();

        override public string GetShortName()
        {
            if (m_CMDs.IsNullOrEmpty()) return base.GetShortName();
            return $"[{m_CMDs.ConcatString((iCMD) => iCMD.GetShortName())}]".CutToMaxLengthRichText(25);
        }
        public override List<SDU_CMD> GetCMDList()
        {
            var aList = new List<SDU_CMD>();
            foreach(var aCmd in m_CMDs)
            {
                aList.Append(aCmd.GetCMDList());
            }
            return aList;
        }
        override public async Task TriggerCMD(Tex2ImgSetting iTex2ImgSetting, System.Threading.CancellationToken iCancellationToken)
        {
            await Task.Delay(1);
            //if (m_CMDs.IsNullOrEmpty()) return;
            //iTex2ImgSetting.RequireClearDic = true;
            //var aCMDs = m_CMDs.Clone();
            //foreach (var aCMD in aCMDs)
            //{
            //    if (iCancellationToken.IsCancellationRequested) break;
            //    await aCMD.TriggerCMD(iTex2ImgSetting, iCancellationToken);
            //}
        }
    }
}