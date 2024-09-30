using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.ZMotion.enums
{
    /// <summary>
    /// 参考 5.1.2-1 轴状态
    /// </summary>
    internal enum AxisStatus
    {
        [Description("随动误差超限告警")]
        Status1 = 2,
        [Description("与远程轴通讯出错")]
        Status2 = 4,
        [Description("远程驱动器报错")]
        Status3 = 8,
        [Description("正向硬限位")]
        Status4 = 16,
        [Description("反向硬限位")]
        Status5 = 32,
        [Description("找原点中")]
        Status6 = 64,
        [Description("HOLD 速度保持信号输入")]
        Status7 = 128,
        [Description("随动误差超限出错")]
        Status8 = 256,
        [Description("超过正向软限位")]
        Status9 = 512,
        [Description("超过负向软限位")]
        Status10 = 1024,
        [Description("CANCEL 执行中")]
        Status11 = 2048,
        [Description("脉冲频率超过 MAX_SPEED 限制需要修改降速或修改 MAX_SPEED")]
        Status12 = 4096,
        [Description("机械手指令坐标错误")]
        Status14 = 16384,
        [Description("电源异常")]
        Status18 = 262144,
        [Description("轴速度保护")]
        Status20 = 1048576,
        [Description("运动中触发特殊运动指令失败")]
        Status21 = 2097152,
        [Description("告警信号输入")]
        Status22 = 4194304,
        [Description("轴进入了暂停状态")]
        Status23 = 8388608,
    }
}
