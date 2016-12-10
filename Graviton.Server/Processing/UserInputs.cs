using Graviton.Server.Net;
using Graviton.Server.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public unsafe static class UserInputs
    {
        static int _size = Marshal.SizeOf<PlayerRequest>();
        static Stopwatch _sw = new Stopwatch();
        static Scheduler _scheduler = new Scheduler(120);
        static WorkerPool<UpdateArg> _responsePool;
        static WorkerPool<UpdateArg> _inputPool;
        static UserInputs()
        {
            _start = Diagnostics.Timer.CurrentTime();
            _sw.Start();
            _responsePool = new WorkerPool<UpdateArg>(8, SendUpdates);
            _inputPool = new WorkerPool<UpdateArg>(8, UpdateUserInput);
            _scheduler.Start(ProcessRequests);
        }

        public static bool Authenticate(byte[] raw, out ulong requester)
        {
            var request = AuthenticateRequest.Deserialize(raw);
            requester = 0;
            if (request._IsValid)
            {
                return Game.Authenticate(request, out requester);
            }
            return false;
        }

        public static bool Process(SocketState state, out int offset)
        {
            //if (_requests.Count == 0)
            //{
            //    _sw.Start();
            //}
            var read = false;
            var length = state.Offset;
            var raw = state.Buffer;
            offset = 0;
            while (offset + _size <= length)
            {
                var request = PlayerRequest.Deserialize(raw, offset);
                if (request._IsValid)
                {
                    request.SocketState = state;
                    offset += _size;
                    Enqueue(request);
                    read = true;
                }
                else break;
            }
            offset = length - offset;
            //if (_requests.Count == 1000000)
            //{
            //    _sw.Stop();
            //    Console.WriteLine(_sw.Elapsed.TotalSeconds);
            //}
            return read;

        }

        private static void Enqueue(PlayerRequest request)
        {
            lock (_requests)
            {
                _requests.Add(request);
            }
        }

        static List<PlayerRequest> _requests = new List<PlayerRequest>();

        static double _start;
        //static double _dt;
        //static ulong _count = 0;

        private static void ProcessRequests()
        {
            var now = Diagnostics.Timer.CurrentTime();
            var dt = now - _start;
            _start = now;
            //_dt = (_dt * (double)_count + dt) / ((double)_count + 1d);
            //_count++;
            //if (_count % 240 == 0)
            //{
            //    Console.WriteLine(dt + ", " + _dt);
            //}
            PlayerRequest[] requests;
            lock(_requests)
            {
                requests = _requests.ToArray();
                _requests.Clear();
            }
            var gameTime = new GameTime(_sw.Elapsed, TimeSpan.FromMilliseconds(dt), _scheduler.Epoch);

            for (int i = 0; i < requests.Length; i++)
            {
                var arg = new UpdateArg()
                {
                    Requests = requests,
                    Index = i,
                    GameTime = gameTime
                };
                _inputPool.Execute(arg);
            }
            _inputPool.WaitForCompletion();

            Game.Update(gameTime);

            for (int i = 0; i < requests.Length; i++)
            {
                var arg = new UpdateArg()
                {
                    Requests = requests,
                    Index = i,
                    GameTime = gameTime
                };
                _responsePool.Execute(arg);
            }
            _responsePool.WaitForCompletion();
        }

        private static void UpdateUserInput(UpdateArg arg)
        {
            var requester = arg.Requests[arg.Index];
            Game.ProcessUserInput(arg.GameTime, requester);
        }

        private static void SendUpdates(UpdateArg arg)
        {
            var requester = arg.Requests[arg.Index];
            foreach (var item in Game.GetUserUpdates(arg.GameTime, requester._Viewport_X, requester._Viewport_Y, requester._Viewport_W, requester._Viewport_H))
            {
                arg.Requests[arg.Index].SocketState.Socket.Send(ItemTypes.GetType(item).GetBytes());
                arg.Requests[arg.Index].SocketState.Socket.Send(item.Serialize());
            }
        }

        public class UpdateArg
        {
            public PlayerRequest[] Requests;
            public int Index;
            public GameTime GameTime;
        }
    }
}
