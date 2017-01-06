using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graviton.Server.Drawing;
using Graviton.Server.Processing;
using System.Runtime.InteropServices;
using Graviton.Server.Indexing;
using Graviton.Server.Serialization;
using Graviton.Server.Net;

namespace Graviton.Server
{
    
    public class Player : IMovable
    {
        _Player _p = new _Player();
        public Player() { }
        public Player(GameTime gameTime, RectangleF worldBounds)
        {
            LastUpdate = gameTime.Epoch;
            FirstUpdate = gameTime.Epoch;
            WorldBounds = worldBounds;
            ViewPort = new RectangleF()
            {
                X = -50f,
                Y = -33f,
                Width = 100f,
                Height = 66f
            };
            MaxSpeed = 10f;
        }

        public bool IsNew { get; set; }
        public RectangleF Bounds { get { return _p._Bounds; } set { _p._Bounds = value; } }
        public float Vx { get { return _p._Vx; } set { _p._Vx = value; } }
        public float MaxSpeed { get { return _p._MaxSpeed; } set { _p._MaxSpeed = value; } }
        public float Vy { get { return _p._Vy; } set { _p._Vy = value; } }
        public float X { get { return _p._X; } set { _p._X = value; } }
        public float Y { get { return _p._Y; } set { _p._Y = value; } }
        public float Fx { get; set; }
        public float Fy { get; set; }
        public bool IsValid { get { return _p._IsValid; } set { _p._IsValid = value; } }
        public float Mass
        {
            get { return _p._Mass; }
            set
            {
                _p._Mass = value;
                var radius = Radius;
                Bounds = new RectangleF()
                {
                    X = X - radius,
                    Y = Y - radius,
                    Width = radius * 2f,
                    Height = radius * 2f
                };
            }
        }
        public ulong Requester { get { return _p._Requester; } set { _p._Requester = value; } }
        public ulong FirstUpdate { get { return _p._FirstUpdate; } set { _p._FirstUpdate = value; } }
        public ulong LastUpdate { get { return _p._LastUpdate; } set { _p._LastUpdate = value; } }
        public ulong LocalEpoch { get { return _p._LocalEpoch; } set { _p._LocalEpoch = value; } }
        public RectangleF WorldBounds { get; set; }
        public RectangleF ViewPort { get; set; }
        public SocketState SocketState { get; set; }
        public QuadTree<IMovable>.Quad Quad { get; set; }
        public float Radius
        {
            get
            {
                return 80f * (float)Math.Tanh(Mass / 500f) + 0.02f;
            }
        }

        public ushort Type
        {
            get
            {
                return (ushort)ItemTypeId.Player;
            }
        }

        public void Update(GameTime gameTime)
        {
            LastUpdate = gameTime.Epoch;
            Vx += Fx * (float)gameTime.EpochGameTime.TotalSeconds / Mass;
            Vy += Fy * (float)gameTime.EpochGameTime.TotalSeconds / Mass;
            Vx = Vx.Clamp(-Game.MaxSpeed, Game.MaxSpeed);
            Vy = Vy.Clamp(-Game.MaxSpeed, Game.MaxSpeed);
            if (Vx != 0f || Vy != 0f)
            {
                if (X == Game.WorldSize && Vx > 0) Vx = 0;
                if (X == -Game.WorldSize && Vx < 0) Vx = 0;
                if (Y == Game.WorldSize && Vy > 0) Vx = 0;
                if (Y == -Game.WorldSize && Vy < 0) Vx = 0;

                var dx = Vx * (float)gameTime.EpochGameTime.TotalSeconds;
                var dy = Vy * (float)gameTime.EpochGameTime.TotalSeconds;
                X = (X + dx).Clamp(WorldBounds.X, WorldBounds.X + WorldBounds.Width);
                Y = (Y + dy).Clamp(WorldBounds.Y, WorldBounds.Y + WorldBounds.Height);

                var radius = Radius;

                Bounds = new RectangleF()
                {
                    X = X - radius,
                    Y = Y - radius,
                    Width = radius * 2f,
                    Height = radius * 2f
                };

                ViewPort = new RectangleF()
                {
                    X = ViewPort.X + dx,
                    Y = ViewPort.Y + dy,
                    Width = ViewPort.Width,
                    Height = ViewPort.Height
                };
            }
            Fx = 0;
            Fy = 0;
        }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf<_Player>();
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(_p, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public void Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new _Player();
            int size = Marshal.SizeOf<_Player>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (_Player)Marshal.PtrToStructure(ptr, typeof(_Player));
            Marshal.FreeHGlobal(ptr);
             _p = deserialized;
        }

        
    }

    [StructLayout(LayoutKind.Sequential)]
    public class _Player
    {
        public RectangleF _Bounds;
        public float _Vx;
        public float _Vy;
        public float _X;
        public float _Y;
        public float _MaxSpeed;
        public ulong _Requester;
        public ulong _FirstUpdate;
        public ulong _LastUpdate;
        public ulong _LocalEpoch;
        public float _Mass;
        public bool _IsValid;
    }
}
