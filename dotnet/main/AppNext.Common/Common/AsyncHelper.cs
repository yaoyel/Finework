using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppBoot.Common
{
    public static class AsyncHelper
    {
        private static readonly TaskFactory m_TaskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            Task<Task<TResult>> task = m_TaskFactory.StartNew(func);
            TaskAwaiter<TResult> awaiter = task.Unwrap().GetAwaiter();
            return awaiter.GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            Task<Task> task = m_TaskFactory.StartNew(func);
            TaskAwaiter awaiter = task.Unwrap().GetAwaiter();
            awaiter.GetResult();
        }
    }
}
