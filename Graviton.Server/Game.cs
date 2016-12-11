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
        static float _maxSpeed = 10f;
        static float _worldSize = 10000f;
        static ulong _requester = 0;
        static RectangleF _worldBounds = new RectangleF() { X = -_worldSize, Y = -_worldSize, Height = _worldSize * 2f, Width = _worldSize * 2f };
        static QuadTree<IMovable> _quadTree = new QuadTree<IMovable>(6, _worldBounds);
        static Dictionary<ulong, Player> _players = new Dictionary<ulong, Player>();
        static List<IMovable> _updatables = new List<IMovable>();
        public static bool Authenticate(AuthenticateRequest request, SocketState state, out ulong requester)
        {
            _requester++;
            requester = _requester;
            var player = CreateNewPlayer(UserInputs.GameTime);
            player.SocketState = state;
            player.Requester = requester;
            player.Quad = _quadTree.FindFirst(player.Bounds);
            player.Quad.Items.Add(player);
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
                Mass = 10,
                IsValid = true
            };
        }

        public static void Update(GameTime gameTime)
        {
            for(int i = 0; i < _updatables.Count; i++)
            {
                var updatable = _updatables[i];
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

        public static IEnumerable<ICanSerialize> GetUserUpdates(GameTime gameTime, float x, float y, float width, float height)
        {
            yield return new GameStateResponse()
            {
                Epoch = gameTime.Epoch,
                TimespanTicks = gameTime.TotalGameTime.Ticks,
                WorldSize = _worldSize,
                IsValid = true
            };

            foreach(var item in _quadTree.FindAll(new RectangleF() { X = x, Y = y, Width = width, Height = height})
                                         .SelectMany(q => q.Items).Where(m => m.LastUpdate == gameTime.Epoch))
            {
                if (item is Player)
                {
                    var p = (Player)item;
                    yield return new PlayerStateResponse()
                    {
                        X = p.X,
                        Y = p.Y,
                        Mass = p.Mass,
                        Vx = p.Vx,
                        Vy = p.Vy,
                        LastUpdate = p.LastUpdate,
                        IsValid = true,
                        LocalEpoch = p.LocalEpoch
                    };
                }
            }
        }
    }
}
