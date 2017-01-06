using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graviton.Server.Drawing;
using Graviton.Server.Processing;
using System.Runtime.InteropServices;
using Graviton.Server.Indexing;
using System.Threading;

namespace Graviton.Server
{
    public class Matter : IMovable
    {
        _Matter m = new _Matter();
        static long _id = 0;

        public Matter()
        {
            Interlocked.Increment(ref _id);
            this.Id = _id;
        }
        public Matter(GameTime gameTime, RectangleF worldBounds) : this()
        {
            LastUpdate = gameTime.Epoch;
            FirstUpdate = gameTime.Epoch;
            WorldBounds = worldBounds;
        }

        public bool IsNew { get; set; }
        public long Id { get { return m._Id; } set { m._Id = value; } }
        public RectangleF Bounds { get { return m._Bounds; } set { m._Bounds = value; } }
        public MatterType MatterType { get { return m._Type; } set { m._Type = value; } }
        public float Fx { get; set; }
        public float Fy { get; set; }
        public float Vx { get { return m._Vx; } set { m._Vx = value; } }
        public float Angle { get { return m._Angle; } set { m._Angle = value; } }
        public float Vy { get { return m._Vy; } set { m._Vy = value; } }
        public float X { get { return m._X; } set { m._X = value; } }
        public float Y { get { return m._Y; } set { m._Y = value; } }
        public float Mass
        {
            get { return m._Mass; }
            set
            {
                m._Mass = value;
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
        public ulong FirstUpdate { get { return m._FirstUpdate; } set { m._FirstUpdate = value; } }
        public ulong LastUpdate { get { return m._LastUpdate; } set { m._LastUpdate = value; } }
        public bool IsValid { get { return m._IsValid; } set { m._IsValid = value; } }

        public float Radius
        {
            get
            {
                return 80f * (float)Math.Tanh(Mass / 50000f) + 0.02f;
            }
        }

        public RectangleF WorldBounds { get; set; }
        public QuadTree<IMovable>.Quad Quad { get; set; }

        public ushort Type
        {
            get
            {
                return (ushort)ItemTypeId.Matter;
            }
        }

        public void Update(GameTime gameTime)
        {
            Vx += Fx * (float)gameTime.EpochGameTime.TotalSeconds / Mass;
            Vy += Fy * (float)gameTime.EpochGameTime.TotalSeconds / Mass;
            Vx = Vx.Clamp(-Game.MaxSpeed, Game.MaxSpeed);
            Vy = Vy.Clamp(-Game.MaxSpeed, Game.MaxSpeed);
            if (Vx != 0f || Vy != 0f)
            {
                LastUpdate = gameTime.Epoch;
                
                var dx = Vx * (float)gameTime.EpochGameTime.TotalSeconds;
                var dy = Vy * (float)gameTime.EpochGameTime.TotalSeconds;
                Bounce(dx, dy);

                var radius = Radius;

                Bounds = new RectangleF()
                {
                    X = X - radius,
                    Y = Y - radius,
                    Width = radius * 2f,
                    Height = radius * 2f
                };
            }
            Fx = 0;
            Fy = 0;
        }

        private void Bounce(float dx, float dy)
        {
            if (X + dx > WorldBounds.X + WorldBounds.Width || X + dx < WorldBounds.X)
            {
                dx = -dx;
                Vx = -Vx;
            }
            if (Y + dy > WorldBounds.Y + WorldBounds.Height || Y + dy < WorldBounds.Y)
            {
                dy = -dy;
                Vy = -Vy;
            }
            X += dx;
            Y += dy;
        }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf<_Matter>();
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public void Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new _Matter();
            int size = Marshal.SizeOf<_Matter>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (_Matter)Marshal.PtrToStructure(ptr, typeof(_Matter));
            Marshal.FreeHGlobal(ptr);
            m = deserialized;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public class _Matter
    {
        public RectangleF _Bounds;
        public MatterType _Type;
        public long _Id;
        public float _Angle;
        public float _Vx;
        public float _Vy;
        public float _X;
        public float _Y;
        public float _Mass;
        public ulong _FirstUpdate;
        public ulong _LastUpdate;
        public bool _IsValid;
    }
}
