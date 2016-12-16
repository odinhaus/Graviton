using Graviton.Server.Drawing;
using Graviton.Server.Indexing;
using Graviton.Server.Net;
using Graviton.Server.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server
{
    public static class Game
    {
        public const float MaxSpeed = 50f;
        public const float WorldSize = 500000;
        static ulong _requester = 0;
        static RectangleF _worldBounds = new RectangleF() { X = -WorldSize, Y = -WorldSize, Height = WorldSize * 2f, Width = WorldSize * 2f };
        static QuadTree<IMovable> _quadTree = new QuadTree<IMovable>(6, _worldBounds);
        static Dictionary<ulong, Player> _players = new Dictionary<ulong, Player>();
        static List<IMovable> _updatables = new List<IMovable>();
        static List<IMovable> _moving = new List<IMovable>();
        static int _matterCount = 500000;
        const float G = 5e+04f;
        const float DRAG = 32.8f;
        const float MinV = 0.4f;


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
                matter.X = RandomFloat(r, -WorldSize + 20f, WorldSize - 20f);
                matter.Y = RandomFloat(r, -WorldSize + 20f, WorldSize - 20f);
                matter.Vx = 0;
                matter.Vy = 0;
                matter.Angle = RandomFloat(r, 0, (float)Math.PI * 2f);
                matter.FirstUpdate = UserInputs.GameTime.Epoch;
                matter.LastUpdate = UserInputs.GameTime.Epoch;
                matter.Mass = (uint)r.Next(2000, 12500);
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
                _moving.Add(matter);
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
            _moving.Add(player);
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
            player.Vx = request.Vector_X * MaxSpeed;
            player.Vy = request.Vector_Y * MaxSpeed;
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
            UpdateGravity(gameTime);

            for(int i = _moving.Count - 1; i >= 0 ; i--)
            {
                var updatable = _moving[i];
                if ( updatable.IsNew 
                    || ( updatable.Vx != 0f && updatable.Vy != 0f)
                    || (updatable is Matter && ((Matter)updatable).Fx != 0f && ((Matter)updatable).Fy != 0f))
                {
                    UpdateDrag(gameTime, updatable);

                    updatable.Update(gameTime);

                    if (updatable.Vx < MinV && updatable.Vx > -MinV)
                        updatable.Vx = 0;
                    if (updatable.Vy < MinV && updatable.Vy > -MinV)
                        updatable.Vy = 0;

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
                else
                {
                    if (!(updatable is Player))
                    {
                        _moving.RemoveAt(i);
                    }
                }
                updatable.IsNew = false;
            }
        }

        private static void UpdateDrag(GameTime gameTime, IMovable item)
        {
            if (item is Matter)
            {
                var matter = (Matter)item;
                if (matter.Vx != 0f)
                    matter.Fx += matter.Vx * matter.Vx * DRAG * (float)Math.Sign(matter.Vx) * -1f;
                if (matter.Vy != 0f)
                    matter.Fy += matter.Vy * matter.Vy * DRAG * (float)Math.Sign(matter.Vy) * -1f;
            }
        }

        private static void UpdateGravity(GameTime gameTime)
        {
            Player[] players;
            lock (_players)
            {
                players = _players.Values.ToArray();
            }

            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                var gThresh = (float)Math.Sqrt(G * player.Mass); // maximum distance to still produce an acceleration of 1 unit/s^2
                var quads = _quadTree.FindAll(new RectangleF() { X = player.X - gThresh / 2f, Y = player.Y - gThresh / 2f, Width = gThresh, Height = gThresh });
                var dMin = float.MaxValue;
                foreach (var quad in quads)
                {
                    Matter[] matter;
                    lock (quad.Items)
                    {
                        matter = quad.Items.OfType<Matter>().ToArray();
                    }
                    for(int m = 0; m < matter.Length; m++)
                    {
                        var mm = matter[m];
                        float dx = player.X - mm.X, dy = player.Y - mm.Y;
                        var dsquared =(dx*dx + dy*dy).Clamp(1f, float.MaxValue);
                        var d = (float)Math.Sqrt(dsquared);
                        if (d <= gThresh)
                        {
                            float rx = dx / d, ry = dy / d;
                            var f = G * player.Mass / dsquared;
                            mm.Fx += rx * f;
                            mm.Fy += ry * f;
                            if (!_moving.Contains(mm))
                            {
                                _moving.Add(mm);
                            }

                            if (dsquared < dMin)
                                dMin = dsquared;
                        }
                    }
                }
                Debug.WriteLine(dMin);
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
                WorldSize = WorldSize,
                IsValid = true
            };

            List<IMovable> items = new List<IMovable>();
            lock(_quadTree)
            {
                //if (requester.IsNew)
                //{
                //    items = _updatables.ToArray();
                //}
                //else
                //{
                //    items = _quadTree.FindAll(new RectangleF() { X = requester.ViewPort.X, Y = requester.ViewPort.Y, Width = requester.ViewPort.Width, Height = requester.ViewPort.Height })
                //                     .SelectMany(q => q.Items)
                //                     .Where(i => i.LastUpdate == gameTime.Epoch)
                //                     .ToArray();
                //}
                foreach(var quad in _quadTree.FindAll(new RectangleF() { X = requester.ViewPort.X, Y = requester.ViewPort.Y, Width = requester.ViewPort.Width, Height = requester.ViewPort.Height }))
                {
                    ulong epoch = 0;
                    if (quad.PlayerUpdates.ContainsKey(requester.Requester))
                    {
                        epoch = quad.PlayerUpdates[requester.Requester];
                        quad.PlayerUpdates[requester.Requester] = gameTime.Epoch;
                    }
                    else
                    {
                        quad.PlayerUpdates.Add(requester.Requester, gameTime.Epoch);
                    }
                    items.AddRange(quad.Items.Where(i => i.LastUpdate >= epoch));
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
