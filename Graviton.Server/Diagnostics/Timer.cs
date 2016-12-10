using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Graviton.Server.Diagnostics
{
    public class Timer
    {
        [DllImport("Kernel32.dll")]
        public static extern int QueryPerformanceFrequency(ref Int64 lpFrequency);

        [DllImport("Kernel32.dll")]
        public static extern int QueryPerformanceCounter(ref Int64 lpPerformanceCount);

        static Int64 _freq;
        static double _freqInv;

        static Timer()
        {
            QueryPerformanceFrequency(ref _freq);
            _freqInv = (1d / (double)_freq) * 1000;
        }

        /// <summary>
        /// returns a double value representing a relative time value in fractional milliseconds
        /// </summary>
        /// <returns></returns>
        public static double CurrentTime()
        {
            long time = 0;
            QueryPerformanceCounter(ref time);
            return time * _freqInv;
        }

        /// <summary>
        /// Creates a call timer record that can be used to loosely time the 
        /// execution of a process 
        /// </summary>
        /// <param name="key"></param>
        public static void StartCallTimer(string key)
        {
            LocalDataStoreSlot lds = Thread.GetNamedDataSlot("metrics");
            Dictionary<string, Int64> timers = Thread.GetData(lds) as Dictionary<string, Int64>;

            if (timers == null)
            {
                timers = new Dictionary<string, long>();
                Thread.SetData(lds, timers);
            }

            if (!timers.ContainsKey(key))
            {
                Int64 time = 0;
                QueryPerformanceCounter(ref time);
                timers.Add(key, time);
#if(TIMER)
                System.Diagnostics.Logger.Log(Thread.CurrentThread.ManagedThreadId.ToString() + " - " + key + " started...");
#endif
            }
        }

        /// <summary>
        /// Stops an existing call timer, and returns the amount of time the
        /// process took (in milliseconds).
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double StopCallTimer(string key)
        {
            LocalDataStoreSlot lds = Thread.GetNamedDataSlot("metrics");
            Dictionary<string, Int64> timers = Thread.GetData(lds) as Dictionary<string, Int64>;

            if (timers == null || !timers.ContainsKey(key)) return -1;

            Int64 time = 0;
            QueryPerformanceCounter(ref time);

            double delta = (double)(time - timers[key]) * _freqInv;
            timers.Remove(key);
#if(TIMER)
            System.Diagnostics.Logger.Log(Thread.CurrentThread.ManagedThreadId.ToString() + " - " + key + " took " + delta.ToString() + " ms.");
#endif
            return delta;
        }

        /// <summary>
        /// The time interval that the timer takes measurements in milliseconds/tick
        /// </summary>
        public static double TimerFrequency { get { return _freqInv; } }
    }
}
