using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZMotionWindow.Utils
{
    public class CancelableThreadTask
    {
        private readonly Action _action;
        private Thread _thread;
        private int _isRunning = 0;// 0：未运行  1：运行中

        public CancelableThreadTask(Action action)
        {
            if (action == null) 
                throw new ArgumentNullException("action");
            _action = action;
        }

        public Task RunAsync(CancellationToken token)
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
                throw new InvalidOperationException("Task is already running!");

            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            _thread = new Thread(() =>
            {
                try
                {
                    _action();
                    tcs.SetResult(0);
                }
                catch (Exception ex)
                {
                    if (ex is ThreadInterruptedException)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.SetException(ex);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _isRunning, 0);
                }
            });

            token.Register(() =>
            {
                if (Interlocked.CompareExchange(ref _isRunning, 0, 1) == 1)
                {
                    //终止线程
                    //_thread.Interrupt();//无法打断没有阻塞的线程
                    _thread.Abort();//销毁线程效果很好，但只适用于net framework
                    _thread.Join();
                }
            });

            _thread.Start();

            return tcs.Task;
        }
    }
}
