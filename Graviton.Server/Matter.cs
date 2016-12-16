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
        public float Fx;
        public float Fy;
        public float Vx { get { return m._Vx; } set { m._Vx = value; } }
        public float Angle { get { return m._Angle; } set { m._Angle = value; } }
        public float Vy { get { return m._Vy; } set { m._Vy = value; } }
        public float X { get { return m._X; } set { m._X = value; } }
        public float Y { get { return m._Y; } set { m._Y = value; } }
        public float Mass { get { return m._Mass; } set { m._Mass = value; } }
        public ulong FirstUpdate { get { return m._FirstUpdate; } set { m._FirstUpdate = value; } }
        public ulong LastUpdate { get { return m._LastUpdate; } set { m._LastUpdate = value; } }
        public bool IsValid { get { return m._IsValid; } set { m._IsValid = value; } }

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
            Vx += (Fx * (float)gameTime.EpochGameTime.TotalSeconds / Mass).Clamp(-Game.MaxSpeed, Game.MaxSpeed);
            Vy += (Fy * (float)gameTime.EpochGameTime.TotalSeconds / Mass).Clamp(-Game.MaxSpeed, Game.MaxSpeed);
            if (Vx != 0f || Vy != 0f)
            {
                LastUpdate = gameTime.Epoch;
                if (X == Game.WorldSize && Vx > 0) Vx = 0;
                if (X == -Game.WorldSize && Vx < 0) Vx = 0;
                if (Y == Game.WorldSize && Vy > 0) Vy = 0;
                if (Y == -Game.WorldSize && Vy < 0) Vy = 0;

                var dx = Vx * (float)gameTime.EpochGameTime.TotalSeconds;
                var dy = Vy * (float)gameTime.EpochGameTime.TotalSeconds;
                X = (X + dx).Clamp(WorldBounds.X, WorldBounds.X + WorldBounds.Width);
                Y = (Y + dy).Clamp(WorldBounds.Y, WorldBounds.Y + WorldBounds.Height);

                X = X.Clamp(-Game.WorldSize, Game.WorldSize);
                Y = Y.Clamp(-Game.WorldSize, Game.WorldSize);

                Bounds = new RectangleF()
                {
                    X = X - Bounds.Width / 2f,
                    Y = Y - Bounds.Height / 2f,
                    Width = Bounds.Width,
                    Height = Bounds.Height
                };
            }
            Fx = 0;
            Fy = 0;
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
