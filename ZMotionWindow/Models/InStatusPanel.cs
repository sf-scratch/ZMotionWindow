using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.Models
{
    internal class InStatusPanel : BindableBase
    {
        private long _inStatus;

        public long InStatus
        {
            get { return _inStatus; }
            set 
            { 
                _inStatus = value; 
                RaisePropertyChanged(nameof(Tag));
            }
        }

        private int _inNum;
        public int InNum
        {
            get { return _inNum; }
            set { _inNum = value; }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public string Tag
        {
            get {  return (InStatus != 0).ToString(); }
        }

        public InStatusPanel(long inStatus, int inNum)
        {
            _inStatus = inStatus;
            _inNum = inNum;
            _content = string.Format("{0}", inNum);
        }
    }
}
