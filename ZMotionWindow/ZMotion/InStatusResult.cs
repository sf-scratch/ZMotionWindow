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

        private long _status;
        /// <summary>
        /// In状态
        /// </summary>
        public long Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public TckIntention Intention { get; set; }

        public InStatusResult(int inNum, long status, TckIntention intention = TckIntention.None) 
        {
            _inNum = inNum;
            _status = status;
            Intention = intention;
        }

        public InStatusResult(TckIntention intention)
        {
            _inNum = 0;
            _status = 0;
            Intention = intention;
        }
    }

    public enum TckIntention
    {
        None,
        Stop,
        StopThrowErr
    }
}
