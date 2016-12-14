using Graviton.Server.Drawing;
using Graviton.Server.Indexing;
using Graviton.Server.Net;
using Graviton.Server.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server
{
    public static class Game
    {
        static float _maxSpeed = 50f;
        static float _worldSize = 13000f;
        static ulong _requester = 0;
        static RectangleF _worldBounds = new RectangleF() { X = -_worldSize, Y = -_worldSize, Height = _worldSize * 2f, Width = _worldSize * 2f };
        static QuadTree<IMovable> _quadTree = new QuadTree<IMovable>(6, _worldBounds);
        static Dictionary<ulong, Player> _players = new Dictionary<ulong, Player>();
        static List<IMovable> _updatables = new List<IMovable>();
        static int _matterCount = 1000;


        static Game()
        {
            Initialize();
        }

        private static void Initialize()
        {
            var r = new Random();
            for (int i = 0; i < _matterCount; i++)
            {
                var matter = new Matter(UserInputs.GameTime, _worldBounds);
                matter.MatterType = MatterType.Gold;
                matter.X = RandomFloat(r, -_worldSize, _worldSize);
                matter.Y = RandomFloat(r, -_worldSize, _worldSize);
                matter.Vx = 0;
                matter.Vy = 0;
                matter.Angle = RandomFloat(r, 0, (float)Math.PI * 2f);
                matter.FirstUpdate = UserInputs.GameTime.Epoch;
                matter.LastUpdate = UserInputs.GameTime.Epoch;
                matter.Mass = (uint)r.Next(5000, 25000);
                var width = 2f * (16f * (float)Math.Tanh(matter.Mass / 50000f) + 0.02f);
                matter.Bounds = new RectangleF()
                {
                    X = matter.X - width / 2,
                    Y = matter.Y - width / 2,
                    Width = width,
                    Height = width
                };
                matter.Quad = _quadTree.FindFirst(matter.Bounds);
                matter.Quad.Items.Add(matter);
                matter.IsNew = true;
                _updatables.Add(matter);
            }
        }

        public static bool Authenticate(AuthenticateRequest request, SocketState state, out ulong requester)
        {
            _requester++;
            requester = _requester;
            var player = CreateNewPlayer(UserInputs.GameTime);
            player.SocketState = state;
            player.Requester = requester;
            player.Quad = _quadTree.FindFirst(player.Bounds);
            player.Quad.Items.Add(player);
            player.IsNew = true;
            _players.Add(requester, player);
            _updatables.Add(player);
            return true;
        }

        public static Player[] Players { get { lock (_players) { return _players.Values.ToArray(); } } }

        public static void ProcessUserInput(GameTime gameTime, PlayerRequest request)
        {
            Player player;

            if (request.IsFirstRequest && !_players.ContainsKey(request.Requester))
            {
                player = CreateNewPlayer(gameTime);
            }
            else
            {
                player = _players[request.Requester];
            }

            player.LocalEpoch = request.LocalEpoch;
            ProcessKeyboardInput(gameTime, player, request.KeyStateMask);
            MovePlayer(gameTime, player, request);
            if (!float.IsNaN(request.ViewPort_H) &&
                !float.IsNaN(request.ViewPort_X) &&
                !float.IsNaN(request.ViewPort_Y) &&
                !float.IsNaN(request.ViewPort_W))
            { 
                player.ViewPort = new RectangleF()
                {
                    X = player.X - request.ViewPort_W / 2f,
                    Y = player.Y - request.ViewPort_H / 2f,
                    Width = request.ViewPort_W,
                    Height = request.ViewPort_H
                };
            }
        }

        public static void UserDisconnected(ulong requester)
        {
            lock (_players)
            {
                var player = _players[requester];
                player.Quad.Items.Remove(player);
                _updatables.Remove(player);
                _players.Remove(requester);
            }
        }

        private static void MovePlayer(GameTime gameTime, Player player, PlayerRequest request)
        {
            player.Vx = request.Vector_X * _maxSpeed;
            player.Vy = request.Vector_Y * _maxSpeed;
        }

        private static void ProcessKeyboardInput(GameTime gameTime, Player player, ulong keyStateMask)
        {
            
        }

        private static Player CreateNewPlayer(GameTime gameTime)
        {
            return new Player(gameTime, _worldBounds)
            {
                X = 0,
                Y = 0,
                Vx = 0,
                Vy = 0,
                Bounds = new RectangleF() { X = -0.5f, Y = -0.5f, Width = 1f, Height = 1f },
                Mass = 10000,
                IsValid = true,
                FirstUpdate = gameTime.Epoch,
                LastUpdate = gameTime.Epoch,
                IsNew = true
            };
        }

        public static void Update(GameTime gameTime)
        {
            for(int i = 0; i < _updatables.Count; i++)
            {
                var updatable = _updatables[i];
                if ( updatable.IsNew || ( updatable.Vx != 0f && updatable.Vy != 0f))
                {
                    updatable.Update(gameTime);


                    if (updatable.Quad != null && !updatable.Quad.Bounds.Intersects(updatable.Bounds))
                    {
                        updatable.Quad.Items.Remove(updatable);
                        updatable.Quad = null;
                    }

                    if (updatable.Quad == null)
                    {
                        var quad = _quadTree.FindFirst(updatable.Bounds);
                        quad.Items.Add(updatable);
                        updatable.Quad = quad;
                    }
                }
            }
        }

        public static IEnumerable<ICanSerialize> GetUserUpdates(GameTime gameTime, Player requester)
        {
            yield return new GameStateResponse()
            {
                Epoch = gameTime.Epoch,
                T = gameTime.TotalGameTime.TotalSeconds,
                dT = gameTime.EpochGameTime.TotalSeconds,
                TimespanTicks = gameTime.TotalGameTime.Ticks,
                WorldSize = _worldSize,
                IsValid = true
            };

            IMovable[] items;
            lock(_quadTree)
            {
                if (requester.IsNew)
                {
                    items = _updatables.ToArray();
                }
                else
                {
                    items = _quadTree.FindAll(new RectangleF() { X = requester.ViewPort.X, Y = requester.ViewPort.Y, Width = requester.ViewPort.Width, Height = requester.ViewPort.Height })
                                     .SelectMany(q => q.Items)
                                     .Where(i => i.LastUpdate == gameTime.Epoch)
                                     .ToArray();
                }
            }

            foreach(var item in items)
            {
                if (item is Player)
                {
                    var p = (Player)item;
                    yield return new PlayerStateResponse()
                    {
                        Requestor = p.Requester,
                        X = p.X,
                        Y = p.Y,
                        Mass = p.Mass,
                        Vx = p.Vx,
                        Vy = p.Vy,
                        LastUpdate = p.LastUpdate,
                        IsValid = true,
                        LocalEpoch = p.LocalEpoch,
                        MaxSpeed = p.MaxSpeed
                    };
                }
                else if (item is Matter)
                {
                    var m = (Matter)item;
                    yield return new MatterStateResponse()
                    {
                        Id = m.Id,
                        Angle = m.Angle,
                        FirstUpdate = m.FirstUpdate,
                        LastUpdate = m.LastUpdate,
                        Mass = m.Mass,
                        MatterType = m.MatterType,
                        Vx = m.Vx,
                        Vy = m.Vy,
                        X = m.X,
                        Y = m.Y,
                        IsValid = true
                    };
                }
                item.IsNew = false;
            }
        }

        private static float RandomFloat(Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }
    }
}
