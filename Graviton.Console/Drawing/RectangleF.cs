using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Common.Drawing
{
    public class RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public bool Intersects(RectangleF r)
        {
            return !(X + Width < r.X || r.X + r.Width < X || Y + Height < r.Y || r.Y + r.Height < Y);
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
