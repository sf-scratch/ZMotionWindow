using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.Models
{
    internal class OutStatusPanel : BindableBase
    {
        private long _outStatus;

        public long OutStatus
        {
            get { return _outStatus; }
            set
            {
                _outStatus = value;
                RaisePropertyChanged(nameof(Tag));
            }
        }

        private string _outNum;
        public string OutNum
        {
            get { return _outNum; }
            set { _outNum = value; }
        }

        public string Tag
        {
            get { return (OutStatus != 0).ToString(); }
        }

        public OutStatusPanel(long outStatus, int outNum)
        {
            _outStatus = outStatus;
            _outNum = outNum.ToString();
        }
    }
}
