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
    public enum CollisionType
    {
        None,
        Edge,
        Intersection,
        Containment
    }
    public static class Game
    {
        public const float MaxSpeed = 2000f;
        public const float WorldSize = 3500;
        public const float AR = 1.6666666f;
        static ulong _requester = 0;
        static RectangleF _worldBounds = new RectangleF() { X = -WorldSize, Y = -WorldSize / AR, Height = WorldSize * 2f / AR, Width = WorldSize * 2f};
        //static QuadTree<IMovable> _quadTree = new QuadTree<IMovable>(1, _worldBounds);
        static Dictionary<ulong, Player> _players = new Dictionary<ulong, Player>();
        static List<IMovable> _updatables = new List<IMovable>();
        static List<IMovable> _moving = new List<IMovable>();
        static int _matterCount = 5;
        const float G = 8e+06f;
        const float DRAG = 3.28f;
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
                matter.Y = RandomFloat(r, (-WorldSize + 20f) / AR, (WorldSize - 20f) / AR);
                matter.Vx = 0;
                matter.Vy = 0;
                matter.Angle = RandomFloat(r, 0, (float)Math.PI * 2f);
                matter.FirstUpdate = UserInputs.GameTime.Epoch;
                matter.LastUpdate = UserInputs.GameTime.Epoch;
                matter.Mass = (uint)r.Next(12000, 162500);
                var width = 2f * (16f * (float)Math.Tanh(matter.Mass / 50000f) + 0.02f);
                matter.Bounds = new RectangleF()
                {
                    X = matter.X - width / 2,
                    Y = matter.Y - width / 2,
                    Width = width,
                    Height = width
                };
                //matter.Quad = _quadTree.FindFirst(matter.Bounds);
                //matter.Quad.Items.Add(matter);
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
            //player.Quad = _quadTree.FindFirst(player.Bounds);
            //player.Quad.Items.Add(player);
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
            // the normalized vector provided represents the desired motion path chosen by the user
            // need to compute force components that would be necessary to translate the current
            // player momentum vector onto the new vector

            // target result momentum
            var Pt = new Vector2d()
            {
                X = request.Vector_X * MaxSpeed * player.Mass,
                Y = request.Vector_Y * MaxSpeed * player.Mass
            };

            // current momentum
            var P0 = new Vector2d()
            {
                X = player.Vx * player.Mass,
                Y = player.Vy * player.Mass
            };

            // impulse momentum required to move the player
            var J = Pt - P0;

            // J = F * dt
            player.Fx += J.X / (float)gameTime.EpochGameTime.TotalSeconds;
            player.Fy += J.Y / (float)gameTime.EpochGameTime.TotalSeconds;

        }

        private static void ProcessKeyboardInput(GameTime gameTime, Player player, ulong keyStateMask)
        {
            // key mask
            // 0 - catch
            // 1 - spin
            //
        }

        private static Player CreateNewPlayer(GameTime gameTime)
        {
            var player = new Player(gameTime, _worldBounds)
            {
                X = 0,
                Y = 0,
                Vx = 0,
                Vy = 0,
                Mass = 500000,
                IsValid = true,
                FirstUpdate = gameTime.Epoch,
                LastUpdate = gameTime.Epoch,
                IsNew = true
            };
            player.Update(gameTime);
            return player;
        }

        public static void Update(GameTime gameTime)
        {
            UpdateGravity(gameTime);

            for(int i = 0; i < _updatables.Count; i++)
            {
                var updatable = _updatables[i];
                //if ( updatable.IsNew 
                //    || ( updatable.Vx != 0f && updatable.Vy != 0f)
                //    || (updatable is Matter && ((Matter)updatable).Fx != 0f && ((Matter)updatable).Fy != 0f))
                //{
                    UpdateDrag(gameTime, updatable);

                    updatable.Update(gameTime);

                    if (updatable.Vx < MinV && updatable.Vx > -MinV)
                        updatable.Vx = 0;
                    if (updatable.Vy < MinV && updatable.Vy > -MinV)
                        updatable.Vy = 0;

                    //if (updatable.Quad != null && !updatable.Quad.Bounds.Intersects(updatable.Bounds))
                    //{
                    //    updatable.Quad.Items.Remove(updatable);
                    //    updatable.Quad = null;
                    //}

                    //if (updatable.Quad == null)
                    //{
                    //    var quad = _quadTree.FindFirst(updatable.Bounds);
                    //    quad.Items.Add(updatable);
                    //    updatable.Quad = quad;
                    //}

                    //foreach(var item in updatable.Quad.Items)
                    //{
                    //    if (item == updatable) continue;
                    //    var collisionType = CheckCollision(updatable, item);
                    //    if (collisionType != CollisionType.None)
                    //    {
                    //        HandleCollision(updatable, item, collisionType);
                    //    }
                    //}
                    for (var j = i + 1; j < _updatables.Count; j++)
                    {
                        var item = _updatables[j];
                        var collisionType = CheckCollision(updatable, item);
                        if (collisionType != CollisionType.None)
                        {
                            HandleCollision(updatable, item, collisionType, gameTime);
                        }
                    }
                //}
                //else
                //{
                //    //if (!(updatable is Player))
                //    //{
                //    //    _moving.RemoveAt(i);
                //    //}
                //}
                updatable.IsNew = false;
            }
        }

        private static void HandleCollision(IMovable updatable, IMovable item, CollisionType type, GameTime gameTime)
        {
            //Console.WriteLine(type.ToString());
            

            // matter bounces off each other
            IMovable m1 = (IMovable)updatable, m2 = (IMovable)item;
            // initial veclocity vectors
            Vector2d v10 = new Vector2d() { X = m1.Vx, Y = m1.Vy },
                        v20 = new Vector2d() { X = m2.Vx, Y = m2.Vy };
            // initial momentum vectors
            Vector2d p10 = v10 * m1.Mass, 
                        p20 = v20 * m2.Mass;
            // final momentum vectors
            Vector2d p1f, p2f;
            // diameter of the momentum circle
            Vector2d D = new Vector2d() { X = p10.X - p20.X, Y = p10.Y - p20.Y };
            var d = D.Magnitude();
            // momentum is conserved during the collision, and exchanged during an equal/opposite Impulse vector
            // representing the momentum swap between the items
            // the impulse is exchanged along the line connecting the two circle centers
            Vector2d j = new Vector2d() { X = m1.X - m2.X, Y = m1.Y - m2.Y };
            j.Normalize(); 
            // we just want the angle here - we need to compute the magnitude
            // we need to get the angle of the midline connecting the two momentum vector end points
            // when the momentum vector tails are connected
            D.Normalize();
            // now get the angle between the impulse vector and the midline
            float a = D.Angle(j) + (float)Math.PI; //(float)(Math.Atan(m.Y / m.X) - Math.Atan(j.Y / j.X));
            // the magnitude of the impulse vector is length of the midline * cos a
            j = j * d * (float)Math.Cos(a);

            p1f = p10 + j;
            p2f = p20 - j;

            m1.Fx += p1f.X / (float)gameTime.EpochGameTime.TotalSeconds;
            m1.Fy += p1f.Y / (float)gameTime.EpochGameTime.TotalSeconds;
            m2.Fx += p2f.X / (float)gameTime.EpochGameTime.TotalSeconds;
            m2.Fy += p2f.Y / (float)gameTime.EpochGameTime.TotalSeconds;

            //var v1 = v10.Magnitude();
            //var v2 = v20.Magnitude();

            //m1.Vx = p1f.X / m1.Mass;
            //m1.Vy = p1f.Y / m1.Mass;

            //m2.Vx = p2f.X / m2.Mass;
            //m2.Vy = p2f.Y / m2.Mass;
            if (!_moving.Contains(m2))
            {
                _moving.Add(m2);
            }

            //            var v1f = (new Vector2d() { X = m1.Vx, Y = m1.Vy }).Magnitude();
            //            var v2f = (new Vector2d() { X = m2.Vx, Y = m2.Vy }).Magnitude();

            //#if (DEBUG)
            //Vector2d pNet0 = p10 + p20;
            //Vector2d pNetf = new Vector2d() { X = m1.Vx * m1.Mass + m2.Vx * m2.Mass, Y = m1.Vy * m1.Mass + m2.Vy * m2.Mass };
            //Vector2d dp = pNetf - pNet0; // this should be 0 (ignoring floating point rounding errors)

            //Vector2d vNetf = new Vector2d() { X = m1.Vx + m2.Vx, Y = m1.Vy + m2.Vy };
            //Debug.Assert(dp.Magnitude() < 100);
            //Debug.Assert(!(v1f > v1 && v2f > v2));
            //#endif

            // we need to completely reset the velocity vectors during the collisions, so that the
            // the sum of the forces on the items will produce the new net velocity during Update()
            m1.Vx = 0f;
            m1.Vy = 0f;
            m2.Vx = 0f;
            m2.Vy = 0f;

            var rr = m1.Radius + m2.Radius;
            var dr = rr - (new Vector2d() { X = m1.X - m2.X, Y = m1.Y - m2.Y }).Magnitude() + 1f;
            var sqt2 = (float)Math.Sqrt(2);

            var dr2 = (dr / 2f) / sqt2;
            if (m2.X > m1.X)
            {
                m2.X += dr2;
                m1.X -= dr2;
            }
            else
            {
                m2.X -= dr2;
                m1.X += dr2;
            }
            if (m2.Y > m1.Y)
            {
                m2.Y += dr2;
                m1.Y -= dr2;
            }
            else
            {
                m2.Y -= dr2;
                m1.Y += dr2;
            }

        }

        private static CollisionType CheckCollision(IMovable updatable, IMovable item)
        {
            //if (!updatable.Bounds.Intersects(item.Bounds)) return CollisionType.None;

            // A = r^2 acos [(d^2 + r^2 - R^2) / 2 * d * r] 
            //   + R^2 acos [(d^2 + R^2 - r^2) / 2 * d * R] 
            //   = sqrt[(-d + r + R)(d + r - R)(d - r + R)(d + r + R)]
            // where d = distance between midpoints, R = radius of circle 1, r = radius of circle 2
            var r = updatable.Radius;
            var R = item.Radius;
            var dx = updatable.X - item.X;
            var dy = updatable.Y - item.Y;
            var d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d > r + R) return CollisionType.None;

            var a = r * r * (float)Math.Acos((d * d + r * r - R * R) / (2f * d * r))
                  + R * R * (float)Math.Acos((d * d + R * R - r * r) / (2f * d * R))
                  - (float)Math.Sqrt((-d + r + R) * (d + r - R) * (d - r + R) * (d + r + R))/2f;
            float pct = r < R ? a / ((float)Math.PI * r * r) : a / ((float)Math.PI * R * R);
            if (pct >= 0.8)
            {
                return CollisionType.Containment;
            }
            else
            {
                return pct <= 0.05 ? CollisionType.Edge : CollisionType.Intersection;
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
            return;
            Player[] players;
            lock (_players)
            {
                players = _players.Values.ToArray();
            }

            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                var gThresh = (float)Math.Sqrt(G * player.Mass); // maximum distance to still produce an acceleration of 1 unit/s^2
                //var quads = _quadTree.FindAll(new RectangleF() { X = player.X - gThresh / 2f, Y = player.Y - gThresh / 2f, Width = gThresh, Height = gThresh });
                var dMin = float.MaxValue;
                //foreach (var quad in quads)
                foreach(var item in _updatables)
                {
                    //Matter[] matter;
                    //lock (quad.Items)
                    //{
                    //    matter = quad.Items.OfType<Matter>().ToArray();
                    //}
                    //for(int m = 0; m < matter.Length; m++)
                    //{
                        //var mm = matter[m];
                        var mm = item as Matter;
                        if (mm == null) continue;

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
                    //}
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
                AspectRatio = AR,
                IsValid = true
            };

            //List<IMovable> items = new List<IMovable>();
            //lock(_quadTree)
            //{
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
                //foreach(var quad in _quadTree.FindAll(new RectangleF() { X = requester.ViewPort.X, Y = requester.ViewPort.Y, Width = requester.ViewPort.Width, Height = requester.ViewPort.Height }))
                //{
                //    ulong epoch = 0;
                //    if (quad.PlayerUpdates.ContainsKey(requester.Requester))
                //    {
                //        epoch = quad.PlayerUpdates[requester.Requester];
                //        quad.PlayerUpdates[requester.Requester] = gameTime.Epoch;
                //    }
                //    else
                //    {
                //        quad.PlayerUpdates.Add(requester.Requester, gameTime.Epoch);
                //    }
                //    items.AddRange(quad.Items.Where(i => i.LastUpdate >= epoch));
                //}
            //}

            //foreach(var item in items)
            foreach(var item in _updatables)
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
