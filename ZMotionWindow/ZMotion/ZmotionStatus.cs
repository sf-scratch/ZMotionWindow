﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ZMotionWindow.ZMotion.enums;
using static cszmcaux.Zmcaux;

namespace ZMotionWindow.ZMotion
{
    public class ZmotionStatus
    {
        public event Action<int> InUpdatedEvent;
        private IntPtr _handle;
        private Timer _timer;

        private int _inStatus;

		public int InStatus
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

        public ZmotionStatus(IntPtr handle)
        {
            _handle = handle;
            _timer = new Timer(10);
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }

        /// <summary>
        /// 等待输入信号改变
        /// </summary>
        /// <param name="inNum">In口</param>
        /// <param name="waitStatus">In状态 -1：改变为任意状态 0：In低电平 1：In高电平</param>
        /// <returns></returns>
        public async Task<InStatusResult> WaitInUpdateAsync(int inNum, int waitStatus = -1)
        {
            return await WaitInUpdateAsync(new TaskCompletionSource<int>(), inNum, waitStatus);
        }

        /// <summary>
        /// 等待多个输入信号中任意一个变化
        /// </summary>
        /// <param name="waitStatus"></param>
        /// <param name="inNums"></param>
        /// <returns></returns>
        public async Task<InStatusResult> WaitInUpdateWhenAnyAsync(int waitStatus, params int[] inNums)
        {
            var dictionary = new Dictionary<Task<InStatusResult>, TaskCompletionSource<int>>();
            foreach (int inNum in inNums)
            {
                TaskCompletionSource<int> source = new TaskCompletionSource<int>();
                Task<InStatusResult> task = WaitInUpdateAsync(source, inNum, waitStatus);
                dictionary.Add(task, source);
            }
            Task<InStatusResult> result = await Task.WhenAny(dictionary.Keys);
            foreach (Task<InStatusResult> task in dictionary.Keys)
            {
                if (task != result && !dictionary[task].Task.IsCompleted)//关闭未结束的任务
                {
                    dictionary[task].SetResult(-1);
                }
            }
            return result.Result;
        }

        private async Task<InStatusResult> WaitInUpdateAsync(TaskCompletionSource<int> tcs, int inNum, int waitStatus)
        {
            int originInStatus = InStatus & (1 << inNum);
            if ((waitStatus == 1 && originInStatus != 0) || (waitStatus == 0 && originInStatus == 0))
            {
                return new InStatusResult(inNum, originInStatus);
            }
            System.Threading.SemaphoreSlim semaphoreSlim = new System.Threading.SemaphoreSlim(1);
            async void In_Updated(int inMulti)
            {
                try
                {
                    int curInStatus = inMulti & (1 << inNum);
                    if ((waitStatus == 1 && curInStatus != 0) || (waitStatus == 0 && curInStatus == 0) || (waitStatus == -1 && curInStatus != originInStatus))
                    {
                        await semaphoreSlim.WaitAsync();
                        if ((waitStatus == 1 && curInStatus != 0) || (waitStatus == 0 && curInStatus == 0) || (waitStatus == -1 && curInStatus != originInStatus))
                        {
                            InUpdatedEvent -= In_Updated;
                            if (!tcs.Task.IsCompleted)
                            {
                                tcs.SetResult(curInStatus);
                            }
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }

            };
            InUpdatedEvent += In_Updated;
            try
            {
                _inStatus = await tcs.Task;
            }
            finally
            {
                InUpdatedEvent -= In_Updated;
            }
            return new InStatusResult(inNum, _inStatus);
        }

        public async Task WaitExpectedResultAsync<T>(Func<T> func, T expectedResult)
        {
            TaskCompletionSource<int> source = new TaskCompletionSource<int>();
            System.Threading.SemaphoreSlim semaphoreSlim = new System.Threading.SemaphoreSlim(1);
            async void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                try
                {
                    T result = func.Invoke();
                    if (result.Equals(expectedResult) && !source.Task.IsCompleted)
                    {
                        await semaphoreSlim.WaitAsync();
                        if (result.Equals(expectedResult) && !source.Task.IsCompleted)
                        {
                            source.SetResult(0);
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            _timer.Elapsed += Timer_Elapsed;
            try
            {
                await source.Task;
            }
            finally
            {
                _timer.Elapsed -= Timer_Elapsed;
            }
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int inMulti;
            ZAux_Direct_GetInMulti(_handle, 0, 31, out inMulti); //获取多路In
            InStatus = inMulti;//更新IO
        }
    }
}
