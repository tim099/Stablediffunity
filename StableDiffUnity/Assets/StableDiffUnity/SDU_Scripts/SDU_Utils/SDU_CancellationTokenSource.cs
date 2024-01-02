using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace SDU
{
    public class SDU_CancellationTokenSource
    {
        private CancellationTokenSource m_CTS = null;
        public SDU_CancellationTokenSource()
        {

        }
        ~SDU_CancellationTokenSource()
        {
            Cancel();
        }
        public CancellationTokenSource Create(CancellationToken iToken)
        {
            Cancel();//Cancel prev CancellationRequested if exist
            m_CTS = CancellationTokenSource.CreateLinkedTokenSource(iToken);
            return m_CTS;
        }
        public CancellationTokenSource Create()
        {
            Cancel();//Cancel prev CancellationRequested if exist
            m_CTS = new CancellationTokenSource();
            return m_CTS;
        }
        public void TryCancel(CancellationTokenSource iCTS)
        {
            if (iCTS == null) return;
            if(iCTS == m_CTS)
            {
                Cancel();
            }
        }
        public void Cancel()
        {
            if (m_CTS == null) return;
            if (!m_CTS.IsCancellationRequested)
            {
                m_CTS.Cancel();
            }
            m_CTS.Dispose();
            m_CTS = null;
        }
    }
}