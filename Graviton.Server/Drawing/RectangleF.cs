using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Drawing
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public bool Intersects(RectangleF r)
        {
            return !(X + Width < r.X || r.X + r.Width < X || Y + Height < r.Y || r.Y + r.Height < Y);
        }

        public float Distance(RectangleF other)
        {
            return (float)Math.Sqrt(DistanceSquared(other));
        }

        public float DistanceSquared(RectangleF other)
        {
            float dx, dy;
            return DistanceSquared(other, out dx, out dy);
        }

        public float DistanceSquared(RectangleF other, out float dx, out float dy)
        {
            var c1x = X + Width / 2f;
            var c1y = Y + Height / 2f;
            var c2x = other.X + other.Width / 2f;
            var c2y = other.Y + other.Height / 2f;
            dx = c2x - c1x;
            dy = c2y - c1y;
            return dx * dx + dy * dy;
        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}, R: {2}, B: {3}, W: {4}, H: {5}",
                X.ToString("f1"),
                Y.ToString("f1"),
                (X + Width).ToString("f1"),
                (Y + Height).ToString("f1"),
                Width.ToString("f1"),
                Height.ToString("f1"));
        }
    }
}
