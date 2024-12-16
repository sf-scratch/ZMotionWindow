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

        private int _outNum;
        public int OutNum
        {
            get { return _outNum; }
            set { _outNum = value; }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public string Tag
        {
            get { return (OutStatus != 0).ToString(); }
        }

        public OutStatusPanel(long outStatus, int outNum)
        {
            _outStatus = outStatus;
            _outNum = outNum;
            _content = string.Format("{0}", outNum);
        }
    }
}
