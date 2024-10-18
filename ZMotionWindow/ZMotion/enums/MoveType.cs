using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.ZMotion.enums
{
    public enum MoveType
    {
        /***
0 IDLE(没有运动)
1 MOVE（单轴直线或者插补直线运动）
2 MOVEABS（绝对值单轴直线或者绝对插补直线运动）
3 MHELICAL（圆心螺旋运动）
4 MOVECIRC（圆弧插补）
5 MOVEMODIFY（修改运动位置）
6 MOVESP（SP 速度的单轴直线或者 SP 速度的插补直线运动）
7 MOVEABSSP（SP 速度的绝对值单轴直线/SP 速度的绝对插补直线运动）
8 MOVECIRCSP（SP 速度的圆弧插补）
9 MHELICALSP（SP 速度的圆心螺旋运动）
10 FORWARD，VMOVE(1)（正向持续运动）
11 REVERSE，VMOVE(-1)（负向持续运动）
12 DATUMING (回零运动中)
13 CAM (凸轮表运动)
14 FWD_JOG (映射正向 JOG 运动)
15 REV_JOG (映射负向 JOG 运动)
20 CAMBOX (跟随凸轮表运动)
21 CONNECT (同步运动)
22 MOVELINK (自动凸轮运动)
23 CONNPATH (同步运动 2，矢量类型的)
25 MOVESLINK（自动凸轮运动 2）
26 MOVESPIRAL(渐开线圆弧)
27 MECLIPSE，MECLIPSEABS，MECLIPSESP，MECLIPSEABSSP（椭圆运动）
28 MOVE_OP/MOVE_OP2 ， MOVE_TABLE ， MOVE_PWM ， MOVE_TASKMOVE_PARA，MOVE_ASYNMOVE，MOVE_AOUT（缓冲 IO/缓冲寄存器操作等）
29 MOVE_DELAY，MOVE_WAIT，MOVE_SYNMOVE（缓冲延时）
31 MSPHERICAL，MSPHERICALSP（空间圆弧）
32 MOVE_PT（单位时间内运动距离）
33 CONNFRAME（机械手逆解运动）
34 CONNREFRAME（机械手正解运动）
        */
        [Description("没有运动")]
        IDLE = 0,
        [Description("单轴直线或者插补直线运动")]
        MOVE = 1,
        [Description("绝对值单轴直线或者绝对插补直线运动")]
        MOVEABS = 2,
        [Description("圆心螺旋运动")]
        MHELICAL = 3,
        [Description("圆弧插补")]
        MOVECIRC = 4,
        [Description("修改运动位置")]
        MOVEMODIFY = 5,
        [Description("SP 速度的单轴直线或者 SP 速度的插补直线运动")]
        MOVESP = 6,
        [Description("SP 速度的绝对值单轴直线/SP 速度的绝对插补直线运动")]
        MOVEABSSP = 7,
        [Description("SP 速度的圆弧插补")]
        MOVECIRCSP = 8,
        [Description("SP 速度的圆心螺旋运动")]
        MHELICALSP = 9,
        [Description("1)（正向持续运动")]
        FORWARD = 10,
        [Description("-1)（负向持续运动")]
        REVERSE = 11,
        [Description("回零运动中")]
        DATUMING = 12,
        [Description("凸轮表运动")]
        CAM = 13,
        [Description("映射正向 JOG 运动")]
        FWD_JOG = 14,
        [Description("映射负向 JOG 运动")]
        REV_JOG = 15,
        [Description("跟随凸轮表运动")]
        CAMBOX = 20,
        [Description("同步运动")]
        CONNECT = 21,
        [Description("自动凸轮运动")]
        MOVELINK = 22,
        [Description("同步运动 2，矢量类型的")]
        CONNPATH = 23,
        [Description("自动凸轮运动 2")]
        MOVESLINK = 25,
        [Description("渐开线圆弧")]
        MOVESPIRAL = 26,
        [Description("椭圆运动")]
        MECLIPSE = 27,
        [Description("缓冲 IO/缓冲寄存器操作等")]
        MOVE_OP = 28,
        [Description("缓冲延时")]
        MOVE_DELAY = 29,
        [Description("空间圆弧")]
        MSPHERICAL = 31,
        [Description("单位时间内运动距离")]
        MOVE_PT = 32,
        [Description("机械手逆解运动")]
        CONNFRAME = 33,
        [Description("机械手正解运动")]
        CONNREFRAME = 34,

    }
}
