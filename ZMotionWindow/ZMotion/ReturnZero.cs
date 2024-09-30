using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static cszmcaux.Zmcaux;

namespace ZMotionWindow.ZMotion
{
    internal class ReturnZero
    {
        private IntPtr _handle;
        private int _axis;//轴号
        private IOStatus _ioStatus;
        private int _fwdIn;//正向软限位IO口
        private int _revIn;//负向软限位IO口
        private int _datumIn;//设置原点位映射的IO口

        public ReturnZero(IntPtr handle, int axis, IOStatus ioStatus, int fwdIn, int revIn, int datumIn)
        {
            _handle = handle;
            _axis = axis;
            _ioStatus = ioStatus;
            _fwdIn = fwdIn;
            _revIn = revIn;
            _datumIn = datumIn;
        }

        public async Task StartAsync()
        {
            SetMotionParam(0, 200, 200, 400, 100, 0);
            ZAux_Direct_Single_Vmove(_handle, _axis, 1);//开始正向运动
            Task<int> taskInPositiveLimit = _ioStatus.WaitIn4UpdateAsync();
            Task<int> taskInOrigin = _ioStatus.WaitIn6UpdateAsync();
            Task<int> taskAny = await Task.WhenAny(taskInPositiveLimit, taskInOrigin);//等待In口变化
            if (taskAny == taskInOrigin)
            {
                await _ioStatus.WaitIn6UpdateAsync();//等待走出原点
            }
            ZAux_Direct_Single_Cancel(_handle, _axis, 3);//停止
            SetMotionParam(0, 50, 200, 400, 100, 0);//设置返回运动参数
            ZAux_Direct_Single_Vmove(_handle, _axis, -1);
            await _ioStatus.WaitIn6UpdateAsync();//等待触碰原点
            ZAux_Direct_Single_Cancel(_handle, _axis, 3);//停止
            ZAux_Direct_SetDpos(_handle, _axis, 0);//位置置零
        }
        private int SetMotionParam(int lSpeed, int speed, int accel, int decel, int units, int sramp)
        {
            int res = 0;
            res |= ZAux_Direct_SetLspeed(_handle, _axis, lSpeed); //设置轴起始速度
            res |= ZAux_Direct_SetSpeed(_handle, _axis, speed); //设置轴速度
            res |= ZAux_Direct_SetAccel(_handle, _axis, accel);//设置轴加速度
            res |= ZAux_Direct_SetDecel(_handle, _axis, decel);//设置轴减速度
            res |= ZAux_Direct_SetUnits(_handle, _axis, units); //设置轴脉冲当量
            res |= ZAux_Direct_SetSramp(_handle, _axis, sramp);//设置轴的S曲线时间
            return res;
        }
    }
}
