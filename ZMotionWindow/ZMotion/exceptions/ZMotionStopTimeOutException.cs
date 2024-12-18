using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.ZMotion.exceptions
{
    internal class ZMotionStopTimeOutException : Exception
    {
        public override string Message => "任务未在指定时间内全部结束";
    }
}
