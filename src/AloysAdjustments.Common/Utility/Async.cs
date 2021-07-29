using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AloysAdjustments.Utility
{
    public static class Async
    {
        public static async Task Run(Action action, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Run(action, cancellationToken);
            }
            catch(Exception ex)
            {
                throw new AsyncException(ex);
            }
        }

        public static async Task<T> Run<T>(Func<T> action, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(action, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new AsyncException(ex);
            }
        }

        public static async void Forget(this Task action)
        {
            try
            {
                await action;
            }
            catch (Exception ex)
            {
                throw new AsyncException(ex);
            }
        }

        public static void RunSync(this Func<Task> task)
        {
            Task.Factory.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }
        
        public static TResult RunSync<TResult>(this Func<Task<TResult>> task)
        {
            return Task.Factory.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }
    }
}
