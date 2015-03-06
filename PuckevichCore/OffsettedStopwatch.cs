using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuckevichCore
{
    internal class StopWatchWithOffset
    {
        private readonly Stopwatch __Sw;
        TimeSpan __Offset;

        public StopWatchWithOffset(TimeSpan offsetElapsed)
        {
            __Offset = offsetElapsed;
            __Sw = new Stopwatch();
        }

        public StopWatchWithOffset()
        {
            __Offset = TimeSpan.Zero;
            __Sw = new Stopwatch();
        }

        public void Restart()
        {
            __Sw.Restart();
            __Offset = TimeSpan.Zero;
        }

        public void Start()
        {
            __Sw.Start();
        }

        public void Stop()
        {
            __Sw.Stop();
        }

        public void Reset()
        {
            __Sw.Reset();
            __Offset = TimeSpan.Zero;
        }

        public TimeSpan Elapsed
        {
            get
            {
                return __Sw.Elapsed + __Offset;
            }
            set
            {
                __Offset = value;
            }
        }
    }
}
