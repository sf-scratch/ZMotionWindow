﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ZMotionWindow.ZMotion.enums;
using ZMotionWindow.ZMotion.exceptions;
using static cszmcaux.Zmcaux;

namespace ZMotionWindow.ZMotion
{
    public class ZmotionStatus : IDisposable
    {
        public event Action<long> InUpdatedEvent;
        public event Action<long> OutUpdatedEvent;
        private IntPtr _handle;
        private Timer _timer;
        private readonly HashSet<TaskCompletionSource<InStatusResult>> WaitingTasks;

        private long _inStatus;

		public long InStatus
        {
			get { return _inStatus; }
			set 
            { 
                if (_inStatus != value)
                {
                    InUpdatedEvent?.Invoke(value);
                }
                _inStatus = value;
            }
		}

        private long _outStatus;

        public long OutStatus
        {
            get { return _outStatus; }
            set
            {
                if (_outStatus != value)
                {
                    OutUpdatedEvent?.Invoke(value);
                }
                _outStatus = value;
            }
        }

        public ZmotionStatus(IntPtr handle)
        {
            _handle = handle;
            _timer = new Timer(10);
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
            WaitingTasks = new HashSet<TaskCompletionSource<InStatusResult>>();
        }

        /// <summary>
        /// 等待输入信号改变
        /// </summary>
        /// <param name="inNum">In口</param>
        /// <param name="waitStatus">In状态 -1：改变为任意状态 0：In低电平 1：In高电平</param>
        /// <returns></returns>
        public async Task<InStatusResult> WaitInUpdateAsync(int inNum, int waitStatus = -1)
        {
            TaskCompletionSource<InStatusResult> tck = new TaskCompletionSource<InStatusResult>();
            InStatusResult result = await WaitInUpdateAsync(tck, inNum, waitStatus);
            return result;
        }

        /// <summary>
        /// 等待多个输入信号中任意一个变化
        /// </summary>
        /// <param name="waitStatus"></param>
        /// <param name="inNums"></param>
        /// <returns></returns>
        public async Task<InStatusResult> WaitInUpdateWhenAnyAsync(int waitStatus, params int[] inNums)
        {
            var dictionary = new Dictionary<Task<InStatusResult>, TaskCompletionSource<InStatusResult>>();
            foreach (int inNum in inNums)
            {
                TaskCompletionSource<InStatusResult> source = new TaskCompletionSource<InStatusResult>();
                Task<InStatusResult> task = WaitInUpdateAsync(source, inNum, waitStatus);
                dictionary.Add(task, source);
            }
            Task<InStatusResult> result = await Task.WhenAny(dictionary.Keys);
            foreach (Task<InStatusResult> task in dictionary.Keys)
            {
                if (task != result && !dictionary[task].Task.IsCompleted)//关闭未结束的任务
                {
                    lock (dictionary[task])
                    {
                        if (!dictionary[task].Task.IsCompleted)
                        {
                            dictionary[task].SetResult(null);
                        }
                    }
                }
            }
            return await result;
        }

        public void StopAllWaiting()
        {
            if (WaitingTasks.Count <= 0) return;
            foreach (var item in WaitingTasks)
            {
                item.TrySetCanceled();
            }
        }

        private async Task<InStatusResult> WaitInUpdateAsync(TaskCompletionSource<InStatusResult> tcs, int inNum, int waitStatus)
        {
            long originInStatus = InStatus & (1L << inNum);
            if ((waitStatus == 1 && originInStatus != 0) || (waitStatus == 0 && originInStatus == 0))
            {
                return new InStatusResult(inNum, originInStatus);
            }
            System.Threading.SemaphoreSlim semaphoreSlim = new System.Threading.SemaphoreSlim(1);
            async void In_Updated(long inMulti)
            {
                try
                {
                    long curInStatus = inMulti & (1L << inNum);
                    if ((waitStatus == 1 && curInStatus != 0) || (waitStatus == 0 && curInStatus == 0) || (waitStatus == -1 && curInStatus != originInStatus))
                    {
                        await semaphoreSlim.WaitAsync();
                        InUpdatedEvent -= In_Updated;
                        if (tcs.Task.IsCompleted) return;
                        lock (tcs)
                        {
                            if (tcs.Task.IsCompleted) return;
                            tcs.SetResult(new InStatusResult(inNum, curInStatus));
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }

            };
            InUpdatedEvent += In_Updated;
            InStatusResult res;
            try
            {
                WaitingTasks.Add(tcs);
                res = await tcs.Task;
            }
            catch(TaskCanceledException ex)
            {
                throw new ZMotionStopException();
            }
            finally
            {
                InUpdatedEvent -= In_Updated;
                WaitingTasks.Remove(tcs);
            }
            return res;
        }

        //public async Task WaitExpectedResultAsync<T>(Func<T> func, T expectedResult)
        //{
        //    TaskCompletionSource<int> source = new TaskCompletionSource<int>();
        //    System.Threading.SemaphoreSlim semaphoreSlim = new System.Threading.SemaphoreSlim(1);
        //    async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        //    {
        //        try
        //        {
        //            T result = func.Invoke();
        //            if (result.Equals(expectedResult) && !source.Task.IsCompleted)
        //            {
        //                await semaphoreSlim.WaitAsync();
        //                if (result.Equals(expectedResult) && !source.Task.IsCompleted)
        //                {
        //                    source.SetResult(0);
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            semaphoreSlim.Release();
        //        }
        //    }
        //    _timer.Elapsed += Timer_Elapsed;
        //    try
        //    {
        //        await source.Task;
        //    }
        //    finally
        //    {
        //        _timer.Elapsed -= Timer_Elapsed;
        //    }
        //}

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            InStatus = CustomZMotion.GetInMulti0_63(_handle);
            OutStatus = CustomZMotion.GetOutMulti0_63(_handle);
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
