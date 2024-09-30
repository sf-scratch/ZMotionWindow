using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ZMotionWindow.ZMotion
{
    internal class IOStatus
    {
        public event Action<int> In2UpdatedEvent;
        public event Action<int> In4UpdatedEvent;
        public event Action<int> In6UpdatedEvent;

        private int in2;

		public int In2
		{
			get { return in2; }
			set 
            { 
                if (in2 != value)
                {
                    In2UpdatedEvent?.Invoke(value);
                }
                in2 = value;
            }
		}

        private int in4;

        public int In4
        {
            get { return in4; }
            set
            {
                if (in4 != value)
                {
                    In4UpdatedEvent?.Invoke(value);
                }
                in4 = value;
            }
        }

        private int in6;

        public int In6
        {
            get { return in6; }
            set
            {
                if (in6 != value)
                {
                    In6UpdatedEvent?.Invoke(value);
                }
                in6 = value;
            }
        }

        /// <summary>
        /// 更新IO
        /// </summary>
        /// <param name="inMulti">多路In（0 - 31）</param>
        public void UpdateIO(int inMulti)
        {
            In2 = inMulti & (1 << 2);
            In4 = inMulti & (1 << 4);
            In6 = inMulti & (1 << 6);
        }

        public async Task<int> WaitIn2UpdateAsync()
        {
            return await InUpdatedAsync(In2UpdatedEvent);
        }

        public async Task<int> WaitIn4UpdateAsync()
        {
            int inStatus = 0;
            var tcs = new TaskCompletionSource<int>();
            void In_Updated(int status)
            {
                tcs.SetResult(status);
            };
            In4UpdatedEvent += In_Updated;
            try
            {
                inStatus = await tcs.Task;
            }
            finally
            {
                In4UpdatedEvent -= In_Updated;
            }
            return inStatus;
        }

        public async Task<int> WaitIn6UpdateAsync()
        {
            int inStatus = 0;
            var tcs = new TaskCompletionSource<int>();
            void In_Updated(int status)
            {
                tcs.SetResult(status);
            };
            In6UpdatedEvent += In_Updated;
            try
            {
                inStatus = await tcs.Task;
            }
            finally
            {
                In6UpdatedEvent -= In_Updated;
            }
            return inStatus;
        }

        private async Task<int> InUpdatedAsync(Action<int> action)
        {
            int inStatus = 0;
            var tcs = new TaskCompletionSource<int>();
            void In_Updated(int status)
            {
                tcs.SetResult(status);
            };
            action += In_Updated;
            try
            {
                inStatus = await tcs.Task;
            }
            finally
            {
                action -= In_Updated;
            }
            return inStatus;
        }
    }
}
