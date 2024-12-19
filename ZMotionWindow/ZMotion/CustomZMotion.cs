using DryIoc;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZMotionWindow.ZMotion.exceptions;
using static cszmcaux.Zmcaux;
using static ImTools.ImMap;

namespace ZMotionWindow.ZMotion
{
    public class CustomZMotion
    {
        private static readonly int InTriggerStatus = 0;//输入触发电平
        private static readonly int InCloseStatus = 1;//输入关闭电平
        private static readonly int LongInch = 100000;
        private static readonly HashSet<Task> RuningTasks = new HashSet<Task>();
        private static CancellationTokenSource CancelAllTasks = new CancellationTokenSource();

        public async static Task ReturnZeroAsync(IntPtr handle, int axis, ZmotionStatus zmotionStatus, int fwdIn, int revIn, int datumIn)
        {
            int res = 0;
            SetMotionParam(handle, axis, 100, 150, 150, 400, 100, 0);//设置找原点运动参数
            res = SingleMove(handle, axis, -1);//开始反向运动
            InStatusResult result = await zmotionStatus.WaitInUpdateWhenAnyAsync(InTriggerStatus, revIn, datumIn);//等待In口变化
            if (result.InNum == datumIn)
            {
                await SingleCancelAsync(handle, axis);//停止
                SingleMove(handle, axis, 1);//开始正向运动
                await zmotionStatus.WaitInUpdateAsync(datumIn, InCloseStatus);//走出原点
                await SingleCancelAsync(handle, axis);//停止
            }
            else
            {
                await SingleCancelAsync(handle, axis);//停止
                SingleMove(handle, axis, 1);//开始正向运动
                await zmotionStatus.WaitInUpdateAsync(datumIn, InTriggerStatus);//触碰原点
                await zmotionStatus.WaitInUpdateAsync(datumIn, InCloseStatus);//走出原点
                await SingleCancelAsync(handle, axis);//停止
            }
            #region 锁存回零
            SetMotionParam(handle, axis, 0, 20, 200, 400, 100, 0);//设置回零运动参数
            SingleMove(handle, axis, -1);//开始反向运动
            await zmotionStatus.WaitInUpdateAsync(datumIn, InTriggerStatus);//触碰原点
            int mode = 3;//等待 R0 下降沿的绝对位置
            ZAux_Direct_Regist(handle, axis, mode);//设置单次锁存，锁存模式：IN0 下降沿触发锁存
            int piValue = 0;
            Task task = Task.Run(async () =>
            {
                CancellationTokenSource source = CancelAllTasks;
                while (true)
                {
                    if (source.IsCancellationRequested)
                    {
                        throw new ZMotionStopException();
                    }
                    await Task.Delay(500, source.Token);
                    ZAux_Direct_GetMark(handle, axis, ref piValue);
                    if (piValue == -1)//触发锁存
                    {
                        ZAux_Direct_Single_Cancel(handle, axis, 3);
                        await Task.Delay(2000, source.Token);//等编码器返回值稳定
                        float curMpos = 0;
                        ZAux_Direct_GetMpos(handle, axis, ref curMpos);
                        float regPos = 0;
                        ZAux_Direct_GetRegPos(handle, axis, ref regPos);
                        SetMotionParam(handle, axis, 100, 150, 150, 400, 100, 0);
                        ZAux_Direct_Single_Move(handle, axis, regPos - curMpos);
                        await Task.Delay(2000, source.Token);//等编码器返回值稳定
                        break;
                    }
                }
            });
            try
            {
                RuningTasks.Add(task);
                await task;
            }
            finally
            {
                RuningTasks.Remove(task);
            }
            #endregion
            #region 普通回零
            //SetMotionParam(handle, axis, 0, 20, 200, 400, 100, 0);//设置回零运动参数
            //SingleMove(handle, axis, -1);//开始反向运动
            //await zmotionStatus.WaitInUpdateAsync(datumIn, InTriggerStatus);//等待触碰原点
            //await zmotionStatus.WaitInUpdateAsync(datumIn, InCloseStatus);//走出原点
            //await SingleCancelAsync(handle, axis);
            //SingleMove(handle, axis, 1);//开始正向运动
            //await zmotionStatus.WaitInUpdateAsync(datumIn, InTriggerStatus);//等待触碰原点
            //await SingleCancelAsync(handle, axis);
            //await Task.Delay(1000);
            #endregion
            ZAux_Direct_SetDpos(handle, axis, 0);//位置置零
            ZAux_Direct_SetMpos(handle, axis, 0);//编码器位置置零
        }

        public static async Task<int> StopAsync(IntPtr handle, int axis, ZmotionStatus zmotionStatus)
        {
            int res = ZAux_Direct_Single_Cancel(handle, axis, 3);
            zmotionStatus.StopAllWaiting();
            CancelAllTasks.Cancel();
            await WaitAllTasksStoped(RuningTasks, 2000);
            return res;
        }

        private static async Task WaitAllTasksStoped(HashSet<Task> tasks, int timeOut)
        {
            if (tasks.Count == 0)
            {
                CancelAllTasks = new CancellationTokenSource();
                return;
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            Task timeoutTask = Task.Delay(timeOut, cts.Token);
            Task completedTask;
            try
            {
                completedTask = await Task.WhenAny(Task.WhenAll(tasks), timeoutTask);
            }
            finally
            {
                CancelAllTasks = new CancellationTokenSource();
            }
            if (completedTask == timeoutTask)
            {
                throw new ZMotionStopTimeOutException();
            }
            cts.Cancel();
        }

        public static long GetInMulti0_63(IntPtr handle)
        {
            long inMulti0_63 = 0;
            uint[] multi = GetInMulti(handle, 0, 63);
            if (multi != null && multi.Length == 2)
            {
                inMulti0_63 = (long)multi[0] | (long)multi[1] << 32;
            }
            return inMulti0_63;
        }

        public static uint[] GetInMulti(IntPtr handle, int start, int end)
        {
            int inCount = end - start + 1;
            int intSize = sizeof(int) * 8;
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append("?");//拼接命令
            int commandCount = inCount % intSize == 0 ? inCount / intSize : (inCount / intSize) + 1;//一个IN命令32个IN状态，即一个int
            for (int i = 0; i < commandCount - 1; i++)
            {
                commandBuilder.Append(string.Format("IN({0},{1})", start, start + intSize - 1));
            }
            commandBuilder.Append(string.Format("IN({0},{1})", end - intSize + 1 < start ? start : end - intSize + 1, end));
            StringBuilder response = new StringBuilder();
            ZAux_DirectCommand(handle, commandBuilder.ToString(), response, 2048);
            int[] multi = new int[commandCount];
            ZAux_TransStringtoInt(response.ToString(), commandCount, multi);
            return multi.Select(p => (uint)p).ToArray();
        }

        public static long GetOutMulti0_63(IntPtr handle)
        {
            long outMulti0_63 = 0;
            uint[] multi = GetOutMulti(handle, 0, 63);
            if (multi != null && multi.Length == 2)
            {
                outMulti0_63 = (long)multi[0] | (long)multi[1] << 32;
            }
            return outMulti0_63;
            //ZAux_Direct_GetOutMulti(handle, 0, 31, out uint outMulti0_31); //获取多路Out
            //ZAux_Direct_GetOutMulti(handle, 32, 63, out uint outMulti32_63); //获取多路Out
            //long status = (long)outMulti32_63 << 32;
            //status = (long)outMulti0_31 | status;
            //return status;
        }

        public static uint[] GetOutMulti(IntPtr handle, int start, int end)
        {
            int inCount = end - start + 1;
            int intSize = sizeof(int) * 8;
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append("?");
            int commandCount = inCount % intSize == 0 ? inCount / intSize : (inCount / intSize) + 1;//一个OUT命令32个OUT状态，即一个int
            for (int i = 0; i < commandCount - 1; i++)
            {
                commandBuilder.Append(string.Format("OUT({0},{1})", start, start + intSize - 1));
            }
            commandBuilder.Append(string.Format("OUT({0},{1})", end - intSize + 1 < start ? start : end - intSize + 1, end));
            StringBuilder response = new StringBuilder();
            ZAux_DirectCommand(handle, commandBuilder.ToString(), response, 2048);
            int[] multi = new int[commandCount];
            ZAux_TransStringtoInt(response.ToString(), commandCount, multi);
            return multi.Select(p => (uint)p).ToArray();
        }

        public static int SetMotionParam(IntPtr handle, int axis, float lSpeed, float speed, float accel, float decel, float units, float sramp)
        {
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.AppendLine(string.Format("LSPEED({0})={1}", axis, lSpeed));//设置轴起始速度
            commandBuilder.AppendLine(string.Format("SPEED({0})={1}", axis, speed));//设置轴速度
            commandBuilder.AppendLine(string.Format("ACCEL({0})={1}", axis, accel));//设置轴加速度
            commandBuilder.AppendLine(string.Format("DECEL({0})={1}", axis, decel));//设置轴减速度
            commandBuilder.AppendLine(string.Format("UNITS({0})={1}", axis, units));//设置轴脉冲当量
            commandBuilder.AppendLine(string.Format("SRAMP({0})={1}", axis, sramp));//设置轴的S曲线时间
            StringBuilder response = new StringBuilder();
            return ZAux_DirectCommand(handle, commandBuilder.ToString(), response, 2048);

            //int res = 0;
            //res |= ZAux_Direct_SetLspeed(handle, axis, lSpeed); //设置轴起始速度
            //res |= ZAux_Direct_SetSpeed(handle, axis, speed); //设置轴速度
            //res |= ZAux_Direct_SetAccel(handle, axis, accel);//设置轴加速度
            //res |= ZAux_Direct_SetDecel(handle, axis, decel);//设置轴减速度
            //res |= ZAux_Direct_SetUnits(handle, axis, units); //设置轴脉冲当量
            //res |= ZAux_Direct_SetSramp(handle, axis, sramp);//设置轴的S曲线时间
            //return res;
        }

        private static async Task SingleCancelAsync(IntPtr handle, int axis)
        {
            int res = ZAux_Direct_Single_Cancel(handle, axis, 3);//停止
            if (res == 0)
            {
                int idle = 0;
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(100);
                        res = ZAux_Direct_GetIfIdle(handle, axis, ref idle);
                        if (res != 0 || idle == -1) break;//-1 表示未运动
                    }
                });
            }
        }

        private static int SingleMove(IntPtr handle, int axis, int dir)
        {
            if (dir != 1 && dir != -1)
            {
                return -1;
            }
            return ZAux_Direct_Single_Move(handle, axis, LongInch * dir);
        }
    }
}
