using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server
{
    public struct Vector2d
    {
        public float X;
        public float Y;

        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public void Normalize()
        {
            var length = (float)Math.Sqrt(X * X + Y * Y);
            X = X / length;
            Y = Y / length;
        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}", X, Y);
        }

        public float Dot(Vector2d other)
        {
            return this.X * other.X + this.Y * other.Y;
        }

        public float Angle(Vector2d other)
        {
            return (float)Math.Acos(Dot(other));
        }

        public static Vector2d operator +(Vector2d v1, Vector2d v2)
        {
            return new Vector2d()
            {
                X = v1.X + v2.X,
                Y = v1.Y + v2.Y
            };
        }
        public static Vector2d operator -(Vector2d v1, Vector2d v2)
        {
            return new Vector2d()
            {
                X = v1.X - v2.X,
                Y = v1.Y - v2.Y
            };
        }
        public static Vector2d operator *(Vector2d v, float scalar)
        {
            return new Vector2d()
            {
                X = v.X * scalar,
                Y = v.Y* scalar
            };
        }
        public static Vector2d operator /(Vector2d v, float scalar)
        {
            return new Vector2d()
            {
                X = v.X / scalar,
                Y = v.Y / scalar
            };
        }
    }
}
