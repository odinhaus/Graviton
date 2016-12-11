using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graviton.Server.Drawing;
using Graviton.Server.Processing;
using System.Runtime.InteropServices;
using Graviton.Server.Indexing;

namespace Graviton.Server
{
    [StructLayout(LayoutKind.Sequential)]
    public class Matter : IMovable
    {
        _Matter m = new _Matter();

        public Matter() { }
        public Matter(GameTime gameTime, RectangleF worldBounds)
        {
            LastUpdate = gameTime.Epoch;
            FirstUpdate = gameTime.Epoch;
            WorldBounds = worldBounds;
        }

        public RectangleF Bounds { get { return m._Bounds; } set { m._Bounds = value; } }
        public float Vx { get { return m._Vx; } set { m._Vx = value; } }
        public float Vy { get { return m._Vy; } set { m._Vy = value; } }
        public float X { get { return m._X; } set { m._X = value; } }
        public float Y { get { return m._Y; } set { m._Y = value; } }
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
            if (Vx != 0f || Vy != 0f)
            {
                LastUpdate = gameTime.Epoch;
                var dx = Vx * (float)gameTime.EpochGameTime.TotalSeconds;
                var dy = Vy * (float)gameTime.EpochGameTime.TotalSeconds;
                X += dx;
                Y += dy;
                Bounds = new RectangleF()
                {
                    X = Bounds.X + dx,
                    Y = Bounds.Y + dy,
                    Width = Bounds.Width,
                    Height = Bounds.Height
                };
            }
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
        public float _Vx;
        public float _Vy;
        public float _X;
        public float _Y;
        public ulong _FirstUpdate;
        public ulong _LastUpdate;
        public bool _IsValid;
    }
}
