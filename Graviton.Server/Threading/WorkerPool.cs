using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Graviton.Server.Threading
{
    public class WorkerPool<T>
    {
        Worker<T>[] _workers;
        WaitHandle[] _waitHandles;
        public WorkerPool(byte workerCount, Action<T> work)
        {
            _workers = new Worker<T>[workerCount];
            for(int i = 0; i < workerCount; i++)
            {
                _workers[i] = new Worker<T>(work);
                _waitHandles[i] = _workers[i].WaitHandle.WaitHandle;
            }
        }

        public void Execute(T arg)
        {
            var worker = _workers[WaitHandle.WaitAny(_waitHandles)];
            worker.Arg = arg;
            worker.ReadyHandle.Reset();
            worker.WaitHandle.Set();
        }

        public void WaitForCompletion()
        {
            WaitHandle.WaitAll(_waitHandles);
        }
    }

    public class Worker<T> : IDisposable
    {
        static int _id = 0;
        public Thread Thread;
        public ManualResetEventSlim WaitHandle;
        public ManualResetEventSlim ReadyHandle;
        private Action<T> Work;
        private bool _running;
        private int Id;

        public Worker(Action<T> work)
        {
            this.Id = _id++;
            this.Work = work;
            this.WaitHandle = new ManualResetEventSlim(false);
            this.ReadyHandle = new ManualResetEventSlim(true);
            this.Thread = new Thread(new ThreadStart(DoWork));
            this.Thread.Name = "Worker " + _id;
            this.Thread.IsBackground = true;
            this.Thread.Start();
        }

        private void DoWork()
        {
            this._running = true;
            while (_running)
            {
                ReadyHandle.Set();
                WaitHandle.Wait();
                WaitHandle.Reset();
                if (_running)
                {
                    // Console.Write(this.Id);
                    Work(Arg);
                }
            }
        }

        public T Arg;

        public void Dispose()
        {
            _running = false;
            WaitHandle.Set();
        }
    }
}
