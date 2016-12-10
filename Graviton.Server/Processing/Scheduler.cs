using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public class Scheduler
    {
        private static readonly Boolean IsSingleCpuMachine = (Environment.ProcessorCount == 1);

        private bool _running = true;
        private double _interval;
        public Scheduler(ushort framesPerSecond)
        {
            this.FPS = framesPerSecond;
            _interval = 1000d / (double)framesPerSecond;
        }

        public ushort FPS { get; private set; }
        public ulong Epoch { get; private set; }

        public void Start(Action work)
        {
            var sleep = _interval;
            double now = 0d, then = 0d;
            while(_running)
            {
                Sleep(sleep);
                now = Diagnostics.Timer.CurrentTime();
                work();
                then = Diagnostics.Timer.CurrentTime();
                sleep = _interval - (then - now);
                sleep.Clamp(0d, _interval);
                Epoch++;
            }
        }

        public void Stop()
        {
            _running = false;
        }

        private void Sleep(double milliseconds)
        {
            if (milliseconds == 0) return;

            var time = Diagnostics.Timer.CurrentTime();
            var slept = 0d;
            var totalTime = (double)milliseconds;

            while (slept < totalTime)
            {
                if (milliseconds > 3)
                {
                    milliseconds = milliseconds / 2d;
                    Thread.Sleep((int)milliseconds);
                }
                else
                {
                    StallThread(1);
                }
                var newTime = Diagnostics.Timer.CurrentTime();
                slept += newTime - time;
                time = newTime;
            }
        }

        private static void StallThread(int threshold)
        {
            if (threshold < 10)
            {
                // On a single-CPU system, spinning does no good
                if (IsSingleCpuMachine) SwitchToThread();

                // Multi-CPU system might be hyper-threaded, let other thread run
                else Thread.SpinWait(1);
            }
            else //if (spinCount < 100000)
            {
                // On a single-CPU system, spinning does no good
                if (IsSingleCpuMachine) SwitchToThread();

                // Multi-CPU system might be hyper-threaded, let other thread run
                else Thread.SpinWait(5);
            }
        }

        [DllImport("kernel32", ExactSpelling = true)]
        private static extern void SwitchToThread();
    }
}
