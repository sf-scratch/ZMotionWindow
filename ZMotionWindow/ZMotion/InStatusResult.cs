using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.ZMotion
{
    public class InStatusResult
    {
        private int _inNum;
        /// <summary>
        /// In号
        /// </summary>
        public int InNum
        {
            get { return _inNum; }
            set { _inNum = value; }
        }

        private int _status;
        /// <summary>
        /// 状态
        /// </summary>
        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public InStatusResult(int inNum, int status) 
        {
            _inNum = inNum;
            _status = status;
        }
    }
}
