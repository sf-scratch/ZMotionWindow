using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.ZMotion.exceptions
{
    internal class ZMotionStopException : Exception
    {
        public override string Message => "控制强制停止!";
    }
}
