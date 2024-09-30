using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static cszmcaux.Zmcaux;

namespace ZMotionWindow.ZMotion
{
    internal class IOStatus
    {
        public event Action<int> InUpdatedEvent;
        private IntPtr _handle;
        private Timer _timer;

        private int inStatus;

		public int InStatus
        {
			get { return inStatus; }
			set 
            { 
                if (inStatus != value)
                {
                    InUpdatedEvent?.Invoke(value);
                }
                inStatus = value;
            }
		}

        public IOStatus(IntPtr handle)
        {
            _handle = handle;
            _timer = new Timer(10);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int inMulti;
            ZAux_Direct_GetInMulti(_handle, 0, 31, out inMulti); //获取多路In
            InStatus = inMulti;//更新IO
        }

        public async Task<int> WaitInUpdateAsync(int inNum)
        {
            int originInStatus = InStatus & (1 << inNum);
            var tcs = new TaskCompletionSource<int>();
            void In_Updated(int inMulti)
            {
                lock (tcs)
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        if ((inMulti & (1 << inNum)) != originInStatus)
                        {
                            if (!tcs.Task.IsCompleted)
                            {
                                tcs.SetResult(inMulti);
                            }
                        }
                    }
                }
            };
            InUpdatedEvent += In_Updated;
            try
            {
                inStatus = await tcs.Task;
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                InUpdatedEvent -= In_Updated;
            }
            return inStatus;
        }
    }
}
