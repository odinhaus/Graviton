using Graviton.Server.Drawing;
using Graviton.Server.Indexing;
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
        static float _worldSize = 200000f;
        static ulong _requester = 0;
        static RectangleF _worldBounds = new RectangleF() { X = -_worldSize, Y = -_worldSize, Height = _worldSize * 2f, Width = _worldSize * 2f };
        static QuadTree<IMovable> _quadTree = new QuadTree<IMovable>(6, _worldBounds);
        static Dictionary<ulong, Player> _players = new Dictionary<ulong, Player>();
        public static bool Authenticate(AuthenticateRequest request, out ulong requester)
        {
            _requester++;
            requester = _requester;
            return true;
        }

        public static void ProcessUserInput(GameTime gameTime, PlayerRequest request)
        {
            Player player;
            if (request._IsFirstRequest && !_players.ContainsKey(request._Requester))
            {
                player = CreateNewPlayer(gameTime, request);
            }
            else
            {
                player = _players[request._Requester];
            }

            ProcessKeyboardInput(gameTime, player, request._KeyStateMask);
            MovePlayer(gameTime, player, request);
        }

        private static void MovePlayer(GameTime gameTime, Player player, PlayerRequest request)
        {
            player.Vx = request._Vector_X * _maxSpeed;
            player.Vy = request._Vector_Y * _maxSpeed;
            player.Update(gameTime);

            
            if (player.Quad != null && !player.Quad.Bounds.Intersects(player.Bounds))
            {
                player.Quad.Items.Remove(player);
                player.Quad = null;
            }

            if (player.Quad == null)
            {
                var quad = _quadTree.FindFirst(player.Bounds);
                quad.Items.Add(player);
                player.Quad = quad;
            }
        }

        private static void ProcessKeyboardInput(GameTime gameTime, Player player, ulong keyStateMask)
        {
            
        }

        private static Player CreateNewPlayer(GameTime gameTime, PlayerRequest request)
        {
            return new Player(gameTime, _worldBounds)
            {
                X = 0,
                Y = 0,
                Vx = 0,
                Vy = 0,
                Bounds = new RectangleF() { X = -0.5f, Y = -0.5f, Width = 1f, Height = 1f },
                IsValid = true
            };
        }

        public static void Update(GameTime gameTime)
        {
            
        }

        public static IEnumerable<ICanSerialize> GetUserUpdates(GameTime gameTime, float x, float y, float width, float height)
        {
            yield return new GameStateResponse()
            {
                _Epoch = gameTime.Epoch,
                _TimespanTicks = gameTime.TotalGameTime.Ticks,
                _WorldSize = _worldSize,
                _IsValid = true
            };

            foreach(var item in _quadTree.FindAll(new RectangleF() { X = x, Y = y, Width = width, Height = height})
                                         .SelectMany(q => q.Items).Where(m => m.LastUpdate == gameTime.Epoch))
            {
                yield return item;
            }
        }
    }
}
