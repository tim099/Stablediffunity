/*
AutoHeader Test
to change the auto header please go to RCG_AutoHeader.cs
*/
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SDU
{
    public static class SDU_Extension
    {
        public static System.Runtime.CompilerServices.TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
        public static void Forget(this ValueTask task, bool logWarning = false)
        {
            task.AsTask().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    if (t.Exception != null)
                    {
                        foreach (var e in t.Exception.Flatten().InnerExceptions)
                        {
                            Debug.LogException(e);
                        }
                    }
                    else
                    {
                        if (logWarning) { Debug.LogWarning($"Task[{t.Id}]: was faulted."); }
                    }

                }
                else if (t.IsCanceled)
                {
                    if (logWarning) { Debug.LogWarning($"Task[{t.Id}]: was canceled."); }
                }
            });
        }

        public static void Forget<T>(this ValueTask<T> task, bool logWarning = false, bool log = false)
        {
            task.AsTask().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    if (t.Exception != null)
                    {
                        foreach (var e in t.Exception.Flatten().InnerExceptions)
                        {
                            Debug.LogError(e);
                        }
                    }
                    else
                    {
                        if (logWarning) { Debug.LogWarning($"Task[{t.Id}]: was faulted."); }
                    }

                }
                else if (t.IsCanceled)
                {
                    if (logWarning) { Debug.LogWarning($"Task[{t.Id}]: was canceled."); }
                }
                else if (t.IsCompleted)
                {
                    if (log) { Debug.Log($"Task[{t.Id}]: completed with {t.Result}"); }
                }
            });
        }
    }
}