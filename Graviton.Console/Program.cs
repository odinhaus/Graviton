//using Graviton.Common;
//using Graviton.Common.Component;
//using Graviton.Common.Neural;
//using Graviton.Common.Scheduling;
//using Graviton.Common.Temporal;
using Graviton.Common.Drawing;
using Graviton.Common.Indexing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Graviton
{
    class Program
    {
        //static InputNeuron i1;
        //static InputNeuron i2;
        //static InputNeuron i3;
        //static KlopfNeuron k1;
        //static KlopfNeuron k2;
        //static KlopfNeuron k3;
        //static KlopfNeuron kf;
        //static ulong max = 45000000;
        ////static ConcurrentQueue<WorkItem<ulong>> _queue = new ConcurrentQueue<WorkItem<ulong>>();
        //static WorkItem<ulong>[] _queue = new WorkItem<ulong>[max];
        //static long _idx = 0;

        //public class WorkItem<T>
        //{
        //    public Action<T> Action;
        //    public T Args;
        //}
        //static ManualResetEvent wait = new ManualResetEvent(false);
        ///*
        //static void Main(string[] args)
        //{
        //    //WaitCallback wc = new WaitCallback(DoItObj);
        //    //TaskFactory tf = new TaskFactory();
        //    //Thread t5 = new Thread(new ThreadStart(Run5ms));
        //    //t5.IsBackground = true;
        //    //t5.Start();
        //    //Console.Read();
        //    //Action<ulong> doit = new Action<ulong>(DoIt);
        //    //Console.WriteLine("Queued");
        //    //for (ulong i = 0; i < max; i++)
        //    //{
        //    //    //Scheduler.Instance.Enqueue(0, new Action<ulong>(DoIt), i);
        //    //    //Scheduler.Instance.EnqueueFast(wc, i);
        //    //    //Task t = tf.StartNew(new Action<object>(DoItObj), (object)i, TaskCreationOptions.AttachedToParent);
        //    //    //_queue.Enqueue(new WorkItem<ulong>(){Action=doit, Args = i}); 
        //    //    _queue[i] = new WorkItem<ulong>(){Action=doit, Args = i};
        //    //    Interlocked.Increment(ref _idx);
        //    //}
        //    //wait.WaitOne();
        //    //wait.Reset();
        //    //lastCount = counter = 0;
        //    //Console.WriteLine("Threadpool");
        //    //for (ulong i = 0; i < max; i++)
        //    //{
        //    //    Scheduler.Instance.Enqueue(5, new Action<ulong>(DoIt), i);
        //    //    //Scheduler.Instance.EnqueueFast(wc, i);
        //    //    //Task t = tf.StartNew(new Action<object>(DoItObj), (object)i, TaskCreationOptions.AttachedToParent);
        //    //    //_queue.Enqueue(new WorkItem<ulong>() { Action = doit, Args = i });
        //    //    //_queue[i] = new WorkItem<ulong>(){Action=doit, Args = i};
        //    //    //Interlocked.Increment(ref _idx);
        //    //}
        //    //wait.WaitOne(30000);
        //    //wait.Reset();
        //    //lastCount = counter = 0;
        //    //Console.WriteLine("Direct");
        //    //for (ulong i = 0; i < max; i++)
        //    //{
        //    //    DoIt(i);
        //    //}
        //    //Console.Read();
        //    KlopfNeuron.Initialize(9);

        //    i1 = new InputNeuron(0,1);
        //    i2 = new InputNeuron(-100,100);
        //    i3 = new InputNeuron(0,10);
        //    k1 = new KlopfNeuron(Activation.Excitatory, 1000);
        //    k2 = new KlopfNeuron(Activation.Excitatory, 1125);
        //    k3 = new KlopfNeuron(Activation.Inhibitory, 1125);
        //    kf = new KlopfNeuron(Activation.Excitatory, 1300);

        //    kf.Alpha = 0.02;
        //    kf.SignalActivation = SignalActivation.Above;
        //    kf.SignalThreshold = 0.2;
        //    kf.Signaled += kf_Signaled;
        //    k1.AddInput(i1, 4.0);
        //    k1.AddInput(i2, 0.5);
        //    //k2.AddInput(i1);
        //    //k2.AddInput(i2);
        //    //k2.AddInput(i3);
        //    //k3.AddInput(i2);
        //    //k3.AddInput(i3);
        //    KlopfNeuronConnection k1kf = new KlopfNeuronConnection(k1, kf) { Weight = 2.0 };
        //    //KlopfNeuronConnection k2kf = new KlopfNeuronConnection(k2, kf);
        //    //KlopfNeuronConnection k3kf = new KlopfNeuronConnection(k3, kf);
        //    //KlopfNeuronConnection k1k2 = new KlopfNeuronConnection(k1, k2);
        //    //k2.AddInput(k1k2);
        //    //k1.AddOutput(k1k2);

        //    //k1.AddOutput(k1kf);
        //    //k2.AddOutput(k2kf);
        //    //k3.AddOutput(k3kf);
        //    kf.AddInput(k1kf);
        //    //kf.AddInput(k2kf);
        //    //kf.AddInput(k3kf);

        //    DateTime start = DateTime.Now;
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    Random r = new Random(100);
        //    int count = 0;
        //    while (true)
        //    {
        //        DateTime now = start.AddMilliseconds(sw.ElapsedMilliseconds);
        //        if (sw.ElapsedMilliseconds > 30000 && sw.ElapsedMilliseconds < 60000)
        //        {
        //            i1.InputValue = 0;
        //            i2.InputValue = 0;
        //        }
        //        else
        //        {
        //            if (count < 40)
        //            {
        //                i1.InputValue = 1;// r.NextDouble();
        //                i2.InputValue = 0;
        //            }
        //            else
        //            {
        //                i1.InputValue = 0;
        //                i2.InputValue = -1;
        //            }
        //        }
        //        //i2.InputValue = 100;// *Math.Sin(((double)sw.ElapsedMilliseconds / 1000d) % (Math.PI * 2));
        //        //i3.InputValue = 10 *  Math.Sin(((double)sw.ElapsedMilliseconds / 3000d) % (Math.PI * 2));

        //        i1.Update(now);
        //        i2.Update(now);
        //        //i3.Update(now);
        //        //k1.Update(now);

        //        Console.WriteLine(string.Format("t: {8}\ti1: {0}\ti2: {1}\ti3: {6}\tk1: {2}\tk2: {3}\tk3: {4}\tkf: {5}\tk1kf: {7}",
        //            i1.Value.ToString("F5"),
        //            i2.Value.ToString("F5"),
        //            k1.Value.ToString("F5"),
        //            k2.Value.ToString("F5"),
        //            k3.Value.ToString("F5"),
        //            kf.Value.ToString("F5"),
        //            i3.Value.ToString("F5"),
        //            k1kf.Weight.ToString("F5"),
        //            now.Subtract(start).TotalSeconds.ToString("F2")));

        //        Thread.Sleep(100);
        //        if (count == 80)
        //            count = 0;
        //        else
        //            count++;
        //    }
        //}
        //*/
        //static void kf_Signaled(object sender, SignalEventArgs e)
        //{

        //}

        ////static void Run5ms()
        ////{
        ////    long lastIdx = 0;
        ////    while (true)
        ////    {
        ////        DateTime start = CurrentTime.Now;
        ////        WorkItem<ulong> wi;
        ////        //while (_queue.TryDequeue(out wi))
        ////        long idx = Interlocked.Read(ref _idx);
        ////        while (idx > lastIdx)
        ////        {
        ////            try
        ////            {
        ////                //wi = _queue.Dequeue();
        ////                wi = _queue[(int)lastIdx];
        ////                wi.Action(wi.Args);
        ////                _queue[lastIdx] = null;
        ////                lastIdx++;
        ////            }
        ////            catch { }
        ////        }
        ////        int sleep = (int)Math.Max(5, Math.Min(0, CurrentTime.Now.Subtract(start).TotalMilliseconds));
        ////        Thread.Sleep(sleep);
        ////    }
        ////}

        ////[STAThread]
        ////static void Main()
        ////{
        ////    InitV2();

        ////    while (true)
        ////    {
        ////        DateTime start = CurrentTime.Now;
        ////        WorkItemEn en = new WorkItemEn(new Action<ulong>(Update), Program.max);
        ////        foreach (WorkItem<ulong> wi in en)
        ////        {
        ////            wi.Action(wi.Args);
        ////        }

        ////        int sleep = (int)Math.Max(0, 5 - CurrentTime.Now.Subtract(start).TotalMilliseconds);
        ////        Console.WriteLine("Sleeping: " + sleep + " ms");
        ////        Thread.Sleep(sleep);
        ////    }
        ////}

        //static DateTime start;
        //static void DoItObj(object count)
        //{
        //    DoIt((ulong)count);
        //}
        //static ulong lastCount = 0;
        //static ulong counter = 0;
        //static void DoIt(ulong count)
        //{
        //    counter++;
        //    if (counter == 0) start = CurrentTime.Now;
        //    if (counter % (max / 5) == 0)
        //    {
        //        DateTime end = CurrentTime.Now;
        //        double delta = end.Subtract(start).TotalMilliseconds;
        //        start = end;
        //        Console.WriteLine("Rate: " + (double)(counter - lastCount) / (double)(delta / 1) + " calls/ms");
        //        lastCount = counter;
        //    }
        //    double value = 1d / (1d - Math.Atan(counter));
        //    if (counter == max) wait.Set();
        //}

        //static Graviton.Common.Neural.V2.Connection v2Connection;
        //static Graviton.Common.Neural.V2.Neuron[] v2Neurons;
        //static void InitV2()
        //{
        //    v2Neurons = new Graviton.Common.Neural.V2.Neuron[]
        //    {
        //        new Common.Neural.V2.Neuron(),
        //        new Common.Neural.V2.Neuron()
        //    };
        //    v2Connection = new Graviton.Common.Neural.V2.Connection16(v2Neurons[0], v2Neurons[1]);
        //    v2Neurons[0].Targets = new Common.Neural.V2.Connection[] { v2Connection };
        //    v2Neurons[1].Sources = new Common.Neural.V2.Connection[] { v2Connection };
        //}

        //static void Update(ulong maxCount)
        //{
        //    v2Neurons[0].Potential = 50;
        //    for (int n = 0; n < v2Neurons.Length; n++)
        //    {
        //        v2Neurons[n].Update();
        //    }
        //    v2Connection.Update();
        //}

        //public class WorkItemEn : IEnumerable<WorkItem<ulong>>
        //{
        //    ulong _counter = 0;
        //    public WorkItemEn(Action<ulong> action, ulong maxCount)
        //    {
        //        Action = action;
        //        MaxCount = maxCount;
        //    }

        //    public Action<ulong> Action { get; private set; }
        //    public ulong MaxCount { get; private set; }

        //    public IEnumerator<WorkItem<ulong>> GetEnumerator()
        //    {
        //        while (_counter < MaxCount)
        //        {
        //            yield return new WorkItem<ulong>() { Action = this.Action, Args = _counter };
        //            _counter++;
        //        }
        //    }

        //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //    {
        //        return GetEnumerator();
        //    }
        //}

        [STAThread]
        static void Main()
        {

            var count = 0;
            var players = 2000;
            var sw = new Stopwatch();
            Initialize(count, players);
            var iteration = 100;
            var step = 0;
            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.Zero);
            sw.Start();
            while(step < iteration)
            {
                gameTime = new GameTime(gameTime.ElapsedGameTime.Add(TimeSpan.FromMilliseconds(267)), TimeSpan.FromMilliseconds(267));
                Update(gameTime);
                step++;
            }
            sw.Stop();
            Console.WriteLine("Total Time: {0}, Rate: {1}", sw.Elapsed.TotalSeconds / (double)iteration, ((double)iteration / sw.Elapsed.TotalSeconds).ToString("g3"));
            Console.Read();
        }

        const float worldSize = 200000f;

        static List<IMovable3> _matter = new List<IMovable3>();
        static float[,] _distances;
        static float _distanceCutoff = 200;
        static float _distanceCutoffSquared = 200 * 200;
        static QuadTree<IMovable3> _index;

        private static void Initialize(int count, int players)
        {

            var random = new Random(1000);
            _index = new QuadTree<IMovable3>(6, new RectangleF() { X = -worldSize, Y = -worldSize, Height = worldSize * 2f, Width = worldSize * 2f});

            for (int i = 0; i < count + players; i++)
            {
                IMassive item;
                if (i < players)
                {
                    item = new Player(
                        new Vector3(RandomFloat(random, -worldSize, worldSize), 0f, RandomFloat(random, -worldSize, worldSize)),
                        new Vector3(RandomFloat(random, -worldSize, worldSize), 0f, RandomFloat(random, -worldSize, worldSize)),
                        RandomFloat(random, 0, 10),
                        RandomFloat(random, 0, 10));
                }
                else
                {
                    item = new Matter(
                        new Vector3(RandomFloat(random, -worldSize, worldSize), 0f, RandomFloat(random, -worldSize, worldSize)),
                        new Vector3(RandomFloat(random, -worldSize, worldSize), 0f, RandomFloat(random, -worldSize, worldSize)),
                        RandomFloat(random, 0, 10),
                        RandomFloat(random, 0, 10));
                }
                var quad = _index.FindFirst(item.Bounds);
                item.Quad = quad;
                quad.Items.Add(item);
                _matter.Add(item);
            }
        }


        private static float RandomFloat(Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        private static void Update(GameTime gameTime)
        {
            var action = new Action<IMovable3, int, int>((item1, i, j0) =>
            {
                var rect = new RectangleF()
                {
                    X = item1.Bounds.X - _distanceCutoff,
                    Y = item1.Bounds.Y - _distanceCutoff,
                    Width = _distanceCutoff * 2f + item1.Bounds.Width,
                    Height = _distanceCutoff * 2f + item1.Bounds.Width
                };

                IEnumerable<IMovable3> en = _matter.Count > 275 ? _index.FindAll(rect).SelectMany(quad => quad.Items) : _matter;

                foreach (var item2 in en)
                {
                    if (item1 is IConsumer && item2 is IMassive)
                    {
                        var distanceSquared = ComputeDistanceSquared(item1, item2);
                        if (distanceSquared < _distanceCutoffSquared)
                        {
                            if (item2 is IConsumer)
                            {
                                ApplyGravity((IMassive)item1, (IMassive)item2, gameTime);
                            }
                            CheckCollisions((IMassive)item1, (IMassive)item2, gameTime, item2 is IConsumer, distanceSquared);
                        }
                    }
                }
            });

            var workers = new Worker[]
            {
                new Worker(action),
                new Worker(action),
                new Worker(action),
                new Worker(action),
                new Worker(action),
                new Worker(action),
                new Worker(action),
                new Worker(action)
            };

            var waitHandles = new WaitHandle[]
            {
                workers[0].ReadyHandle.WaitHandle,
                workers[1].ReadyHandle.WaitHandle,
                workers[2].ReadyHandle.WaitHandle,
                workers[3].ReadyHandle.WaitHandle,
                workers[4].ReadyHandle.WaitHandle,
                workers[5].ReadyHandle.WaitHandle,
                workers[6].ReadyHandle.WaitHandle,
                workers[7].ReadyHandle.WaitHandle
            };

            for(int i = 0; i < _matter.Count; i++)
            {
                var item1 = _matter[i];
                if (item1 is IConsumer)
                {
                    var worker = workers[WaitHandle.WaitAny(waitHandles)];
                    worker.Item = item1;
                    worker.J = i;
                    worker.ReadyHandle.Reset();
                    worker.WaitHandle.Set();
                }
                //BounceSphereInWorld(item1, gameTime);
            }
        }

        public class Worker : IDisposable
        {
            static int _id = 0;
            public Thread Thread;
            public ManualResetEventSlim WaitHandle;
            public ManualResetEventSlim ReadyHandle;
            public int J;
            public IMovable3 Item;
            private Action<IMovable3, int, int> Work;
            private bool _running;
            private int Id;

            public Worker(Action<IMovable3, int, int> work)
            {
                this.Id = _id++;
                this.Work = work;
                this.WaitHandle = new ManualResetEventSlim(false);
                this.ReadyHandle = new ManualResetEventSlim(true);
                this.Thread = new Thread(new ThreadStart(DoWork));
                this.Thread.Name = "Worker";
                this.Thread.IsBackground = true;
                this.Thread.Start();
            }

            private void DoWork()
            {
                this._running = true;
                while(_running)
                {
                    ReadyHandle.Set();
                    WaitHandle.Wait();
                    WaitHandle.Reset();
                    if (_running)
                    {
                       // Console.Write(this.Id);
                        Work(Item, J, J);
                    }
                }
            }

            public void Dispose()
            {
                _running = false;
                WaitHandle.Set();
            }
        }

        private static float ComputeDistance(IMovable3 item1, IMovable3 item2)
        {
            var dx = item1.Position.X - item2.Position.X;
            var dy = item1.Position.Z - item2.Position.Z;
            var dsquared = dx * dx + dy * dy;
            return (float)Math.Sqrt(dsquared);
        }

        private static float ComputeDistanceSquared(IMovable3 item1, IMovable3 item2)
        {
            var dx = item1.Position.X - item2.Position.X;
            var dy = item1.Position.Z - item2.Position.Z;
            return dx * dx + dy * dy;
        }


        private static void BounceSphereInWorld(IMovable3 s, GameTime gameTime)
        {
            if (s.Velocity == Vector3.Zero) return;
            if (s.Velocity.Length() <= 0.001)
            {
                s.Velocity = Vector3.Zero;
                return;
            }

            s.Velocity = s.Velocity * 0.99996f;

            // vy = ay * dt + v0;
            Vector3 velocity = s.Velocity;
            Vector3 position = s.Position;
            if (velocity.Y > 0)
                velocity.Y = velocity.Y - (float)(2f * 9.8f * gameTime.ElapsedGameTime.TotalSeconds);
            // First test along the X axis, flipping the velocity if a collision occurs.
            if (s.Position.X < -worldSize)
            {
                position.X = -worldSize;
                if (s.Velocity.X < 0f)
                    velocity.X *= -1f;
            }
            else if (s.Position.X > worldSize)
            {
                position.X = worldSize;
                if (s.Velocity.X > 0f)
                    velocity.X *= -1f;
            }

            // And lastly the Z axis
            if (s.Position.Z < -worldSize)
            {
                position.Z = -worldSize;
                if (s.Velocity.Z < 0f)
                    velocity.Z *= -1f;
            }
            else if (s.Position.Z > worldSize)
            {
                position.Z = worldSize;
                if (s.Velocity.Z > 0f)
                    velocity.Z *= -1f;
            }
            s.Position = position;
            s.Velocity = velocity;

        }

        static private bool CheckCollisions(IMassive mass1, IMassive mass2, GameTime gameTime, bool fastCheck, double distanceSquared)
        {
            if (fastCheck)
            {
                return distanceSquared <= mass1.Radius * mass1.Radius;
            }
            else
            {
                if (mass1.Radius >= mass2.Radius && mass1 is IConsumer)
                {
                    if (Contains(mass1.Position, mass1.Radius, mass2.Position, mass2.Radius, 0.8f))
                    {
                        // do more work here
                        mass1.Mass += mass2.Mass;
                        return true;
                    }
                }
                else
                {
                    // get angle between two circle centers and determine overlap to move the mass
                    var dx = mass2.Position.X - mass1.Position.X;
                    var dy = mass2.Position.Z - mass1.Position.Z;
                    var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    var minDistance = mass2.Radius + mass1.Radius;
                    var delta = minDistance - distance;
                    mass2.Position = new Vector3(mass2.Position.X + delta * dx / distance, mass2.Position.Y, mass2.Position.Z + delta * dy / distance);
                    mass2.Velocity = mass1.Velocity;
                }
            }
            return false;
        }

        static private bool Contains(Vector3 position1, float radius1, Vector3 position2, float radius2, float percent)
        {
            if (position2.X < position1.X + radius1 && position2.X > position1.X - radius1 && position2.Z < position1.Z + radius1 && position2.Z > position1.Z - radius1)
            {
                var dx = position1.X - position2.X;
                var dy = position1.Z - position2.Z;
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                return distance / radius1 <= percent;
            }
            return false;
        }

        static private float G = -1f;
        static private void ApplyGravity(IMassive mass1, IMassive mass2, GameTime gameTime)
        {
            var dx = mass1.Position.X - mass2.Position.X;
            var dy = mass1.Position.Z - mass2.Position.Z;
            var dsquared = dx * dx + dy * dy;
            var distance = (float)Math.Sqrt(dsquared);
            if (distance <= mass1.Radius) return;
            var f = 10f * G * mass2.Mass / (dsquared);
            var fx = f * dx / distance;
            var fy = f * dy / distance;
            if (fx < 0.001f && fy < 0.001f) return;
            mass1.Velocity = new Vector3(mass1.Velocity.X + fx * (float)gameTime.ElapsedGameTime.TotalSeconds, 0f, mass1.Velocity.Z + fy * (float)gameTime.ElapsedGameTime.TotalSeconds);
            if (mass1.Velocity.Length() > 60 * 3)
            {
                var v = mass1.Velocity;
                v.Normalize();
                mass1.Velocity = v * 60 * 3;
            }
            
        }

    }

    public interface IMovable3 : IPositionable3
    {
        Vector3 Velocity { get; set; }
    }
    public interface IPositionable3
    {
        Vector3 Position { get; set; }
        float Radius { get; }
        RectangleF Bounds { get; }
        BoundingSphere BoundingSphere { get; }
        void Update(GameTime gameTime);
        QuadTree<IMovable3>.Quad Quad { get; set; }
    }

    public interface IMassive : IMovable3
    {
        float Mass { get; set; }
    }

    public interface IConsumer
    {

    }

    public class Matter : IMassive
    {
        public Matter(Vector3 position, Vector3 velocity, float radius, float mass)
        {
            Position = position;
            Velocity = velocity;
            Radius = radius;
            Mass = mass;
            Bounds = new RectangleF() { X = position.X - Radius, Y = position.Z - Radius, Width = 2f * radius, Height = 2f * radius };
        }
        public BoundingSphere BoundingSphere
        {
            get;
            private set;
        }

        public RectangleF Bounds
        {
            get;
            private set;
        }

        public float Mass
        {
            get;

            set;
        }

        public Vector3 Position
        {
            get;

            set;
        }

        public QuadTree<IMovable3>.Quad Quad
        {
            get;
            set;
        }

        public float Radius
        {
            get;
            set;
        }

        public Vector3 Velocity
        {
            get;

            set;
        }

        public void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float dx = Velocity.X * dt, dy = Velocity.Y * dt, dz = Velocity.Z * dt;
            Position = new Vector3( dx, dy, dz);
            Bounds.X += dx;
            Bounds.Y += dz;
        }
    }

    public class Player : Matter, IConsumer
    {
        public Player(Vector3 position, Vector3 velocity, float radius, float mass)
            : base(position, velocity, radius, mass) { }
    }

}
