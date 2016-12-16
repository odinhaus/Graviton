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
        static int _size = Marshal.SizeOf<_PlayerRequest>();
        static Stopwatch _sw = new Stopwatch();
        static Scheduler _scheduler = new Scheduler(120);
        static WorkerPool<UpdateArg<Player>> _responsePool;
        static WorkerPool<UpdateArg<PlayerRequest>> _inputPool;
        static UserInputs()
        {
            _start = Diagnostics.Timer.CurrentTime();
            _sw.Start();
            _responsePool = new WorkerPool<UpdateArg<Player>>(8, SendUpdates);
            _inputPool = new WorkerPool<UpdateArg<PlayerRequest>>(8, UpdateUserInput);
            GameTime = new GameTime(_sw.Elapsed, TimeSpan.FromMilliseconds(0), _scheduler.Epoch);
            _scheduler.Start(ProcessRequests);
        }

        public static bool Authenticate(SocketState state, out ulong requester)
        {
            byte[] raw = state.Buffer;
            var request = new AuthenticateRequest();
            request.Deserialize(raw);
            requester = 0;
            if (request.IsValid)
            {
                return Game.Authenticate(request, state, out requester);
            }
            return false;
        }

        public static void UserDisconnected(ulong requester)
        {
            Game.UserDisconnected(requester);
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
                var request = new PlayerRequest();
                request.Deserialize(raw, offset);
                if (request.IsValid)
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
        static double _dt;
        static ulong _count = 0;

        private static void ProcessRequests()
        {
            var now = Diagnostics.Timer.CurrentTime();
            var dt = now - _start;
            _start = now;
            _dt = (_dt * (double)_count + dt) / ((double)_count + 1d);
            _count++;
            if (_count % 240 == 0)
            {
                Console.WriteLine("Current: " + dt + ", Avg: " + _dt);
            }
            PlayerRequest[] requests;
            lock(_requests)
            {
                requests = _requests.ToArray();
                _requests.Clear();
            }

            GameTime = new GameTime(_sw.Elapsed, TimeSpan.FromMilliseconds(dt), _scheduler.Epoch);

            for (int i = 0; i < requests.Length; i++)
            {
                var arg = new UpdateArg<PlayerRequest>()
                {
                    Requests = requests,
                    Index = i,
                    GameTime = GameTime
                };
                _inputPool.Execute(arg);
            }

            if (requests.Length > 0)
                _inputPool.WaitForCompletion();

            Game.Update(GameTime);
            var players = Game.Players;

            for (int i = 0; i < players.Length; i++)
            {
                var arg = new UpdateArg<Player>()
                {
                    Requests = players,
                    Index = i,
                    GameTime = GameTime
                };
                _responsePool.Execute(arg);
            }

            if (players.Length > 0)
                _responsePool.WaitForCompletion();
        }

        public static GameTime GameTime { get; private set; }

        private static void UpdateUserInput(UpdateArg<PlayerRequest> arg)
        {
            var requester = arg.Requests[arg.Index];
            Game.ProcessUserInput(arg.GameTime, requester);
        }

        private static void SendUpdates(UpdateArg<Player> arg)
        {
            var requester = arg.Requests[arg.Index];
            foreach (var item in Game.GetUserUpdates(arg.GameTime, requester))
            {
                arg.Requests[arg.Index].SocketState.Socket.Send(item.Type.GetBytes());
                arg.Requests[arg.Index].SocketState.Socket.Send(item.Serialize());
            }
        }

        public class UpdateArg<T>
        {
            public T[] Requests;
            public int Index;
            public GameTime GameTime;
        }
    }
}
