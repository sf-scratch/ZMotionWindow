using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static cszmcaux.Zmcaux;
using static ImTools.ImMap;

namespace ZMotionWindow.ZMotion
{
    public class CustomZMotion
    {
        private static readonly int InTriggerStatus = 1;//输入触发电平
        private static readonly int InCloseStatus = 0;//输入关闭电平

        public async static Task ReturnZero(IntPtr handle, int axis, ZmotionStatus zmotionStatus, int fwdIn, int revIn, int datumIn)
        {
            SetMotionParam(handle, axis, 100, 150, 150, 400, 100, 0);//设置找原点运动参数
            ZAux_Direct_Single_Vmove(handle, axis, -1);//开始反向运动
            InStatusResult result = await zmotionStatus.WaitInUpdateWhenAnyAsync(InTriggerStatus, revIn, datumIn);//等待In口变化
            if (result.InNum == datumIn)
            {
                await zmotionStatus.WaitInUpdateAsync(datumIn, InCloseStatus);//继续反向运动，直到走出原点
                ZAux_Direct_Single_Cancel(handle, axis, 3);//停止
                ZAux_Direct_Single_Vmove(handle, axis, 1);//开始正向运动
            }
            else
            {
                ZAux_Direct_Single_Cancel(handle, axis, 3);//停止
                await Task.Delay(1000);//触碰正限位，等待一秒再发命令
                ZAux_Direct_Single_Vmove(handle, axis, 1);//开始正向运动
            }
            await zmotionStatus.WaitInUpdateAsync(datumIn, InTriggerStatus);//触碰原点
            await zmotionStatus.WaitInUpdateAsync(datumIn, InCloseStatus);//走出原点
            ZAux_Direct_Single_Cancel(handle, axis, 3);//停止
            #region 锁存回零
            SetMotionParam(handle, axis, 0, 20, 200, 400, 100, 0);//设置回零运动参数
            ZAux_Direct_Single_Vmove(handle, axis, -1);//开始反向运动
            await zmotionStatus.WaitInUpdateAsync(datumIn, InTriggerStatus);//触碰原点
            int mode = 3;//等待 R0 下降沿的绝对位置
            ZAux_Direct_Regist(handle, axis, mode);//设置单次锁存，锁存模式：IN0 下降沿触发锁存
            int piValue = 0;
            while (true)
            {
                await Task.Delay(500);
                ZAux_Direct_GetMark(handle, axis, ref piValue);
                if (piValue == -1)//触发锁存
                {
                    ZAux_Direct_Single_Cancel(handle, axis, 3);
                    await Task.Delay(2000);//等编码器返回值稳定
                    float curMpos = 0;
                    ZAux_Direct_GetMpos(handle, axis, ref curMpos);
                    float regPos = 0;
                    ZAux_Direct_GetRegPos(handle, axis, ref regPos);
                    SetMotionParam(handle, axis, 100, 150, 150, 400, 100, 0);
                    ZAux_Direct_Single_Move(handle, axis, regPos - curMpos);
                    await Task.Delay(2000);//等编码器返回值稳定
                    break;
                }
            }
            #endregion
            #region 普通回零
            //SetMotionParam(handle, axis, 0, 20, 200, 400, 100, 0);//设置回零运动参数
            //ZAux_Direct_Single_Vmove(handle, axis, -1);//开始反向运动
            //await zmotionStatus.WaitInUpdateAsync(datumIn, InTriggerStatus);//等待触碰原点
            //await zmotionStatus.WaitInUpdateAsync(datumIn, InCloseStatus);//走出原点
            //ZAux_Direct_Single_Cancel(handle, axis, 3);
            //ZAux_Direct_Single_Vmove(handle, axis, 1);//开始正向运动
            //await zmotionStatus.WaitInUpdateAsync(datumIn, InTriggerStatus);//等待触碰原点
            //ZAux_Direct_Single_Cancel(handle, axis, 3);
            //await Task.Delay(1000);
            #endregion
            ZAux_Direct_SetDpos(handle, axis, 0);//位置置零
            ZAux_Direct_SetMpos(handle, axis, 0);//编码器位置置零
        }

        public static long GetInMulti0_63(IntPtr handle)
        {
            ZAux_Direct_GetInMulti(handle, 0, 31, out int inMulti0_31); //获取多路In
            ZAux_Direct_GetInMulti(handle, 32, 63, out int inMulti32_63); //获取多路In
            long status = (long)inMulti32_63 << 32;
            status = (long)inMulti0_31 | status;
            return status;
        }

        public static long GetOutMulti0_63(IntPtr handle)
        {
            ZAux_Direct_GetOutMulti(handle, 0, 31, out uint outMulti0_31); //获取多路Out
            ZAux_Direct_GetOutMulti(handle, 32, 63, out uint outMulti32_63); //获取多路Out
            long status = (long)outMulti32_63 << 32;
            status = (long)outMulti0_31 | status;
            return status;
        }

        private static int SetMotionParam(IntPtr handle, int axis, int lSpeed, int speed, int accel, int decel, int units, int sramp)
        {
            int res = 0;
            res |= ZAux_Direct_SetLspeed(handle, axis, lSpeed); //设置轴起始速度
            res |= ZAux_Direct_SetSpeed(handle, axis, speed); //设置轴速度
            res |= ZAux_Direct_SetAccel(handle, axis, accel);//设置轴加速度
            res |= ZAux_Direct_SetDecel(handle, axis, decel);//设置轴减速度
            res |= ZAux_Direct_SetUnits(handle, axis, units); //设置轴脉冲当量
            res |= ZAux_Direct_SetSramp(handle, axis, sramp);//设置轴的S曲线时间
            return res;
        }
    }
}
