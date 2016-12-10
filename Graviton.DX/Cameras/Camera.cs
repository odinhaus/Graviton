using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Cameras
{
    public class CameraRange
    {
        public PositionRange3 Position { get; set; }
        public AngularRange3 Theta { get; set; }
    }

    public class AngularRange
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", 180f * Min / MathHelper.Pi, 180f * Max / MathHelper.Pi);
        }
    }

    public class AngularRange3
    {
        public AngularRange Azimuth { get; set; }
        public AngularRange Altitude { get; set; }

        public override string ToString()
        {
            return "{" + string.Format("Azimuth: {0} Altitude: {1}", Azimuth, Altitude) + "}";
        }
    }

    public class PositionRange3
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }
    }

    public class Camera
    {
        private float FOV;
        private float NearPlane;
        private float FarPlane;

        public Camera(float fov, float ar, float nearPlane, float farPlane)
        {
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            FOV = fov;
            NearPlane = nearPlane;
            FarPlane = farPlane;
            Projection = Matrix.CreatePerspectiveFieldOfView(
                    fov, ar, nearPlane, farPlane);
            Position = Vector3.Up;
            Target = Vector3.Zero;
            Up = Vector3.Forward;
            Fwd = Vector3.Down;

            Range = new CameraRange()
            {
                Position = new PositionRange3()
                {
                    Min = Vector3.Zero,
                    Max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)
                },
                Theta = new AngularRange3()
                {
                    Azimuth = new AngularRange()
                    {
                        Min = 0f,
                        Max = MathHelper.TwoPi
                    },
                    Altitude = new AngularRange()
                    {
                        Min = -MathHelper.PiOver2,
                        Max = MathHelper.PiOver2
                    }
                }
            };

            Theta = SetTheta(Position);
        }

        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; private set; }
        public Vector3 Fwd { get; private set; }
        public CameraRange Range { get; set; }
        public AngularRange3 Theta { get; private set; }

        public void SetAspectRatio(float ar)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(
                    FOV, ar, NearPlane, FarPlane);
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.View = Matrix.CreateLookAt(this.Position, Target, Up);
        }

        public void OrbitVertically(float amount)
        {
            Vector3 left = Vector3.Cross(Up, Position);
            left.Normalize();
            Vector3 newPos = Vector3.Transform(Position, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
            if (CheckNewPosition(newPos))
            {
                Position = newPos;
                Up = Vector3.Transform(Up, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
                Fwd = Vector3.Transform(Fwd, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
                Theta = SetTheta(newPos);
            }
        }

        public void OrbitHorizontally(float amount)
        {
            Vector3 left = Up;// Vector3.Cross(Fwd, Up);
            left.Normalize();

            Vector3 newPos = Vector3.Transform(Position, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
            if (CheckNewPosition(newPos))
            {
                Position = newPos;
                Up = Vector3.Transform(Up, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
                Fwd = Vector3.Transform(Fwd, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
                Theta = SetTheta(newPos);
            }
        }

        public void Roll(float amount)
        {
            Vector3 left = Position;// Vector3.Cross(Fwd, Up);
            left.Normalize();

            Vector3 newPos = Vector3.Transform(Position, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
            if (CheckNewPosition(newPos))
            {
                Position = newPos;
                Up = Vector3.Transform(Up, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
                Fwd = Vector3.Transform(Fwd, Matrix.CreateFromAxisAngle(left, MathHelper.ToRadians(amount)));
                Theta = SetTheta(newPos);
            }
        }

        public void ZoomIn(float amount)
        {
            Vector3 newPos = Vector3.Transform(Position, Matrix.CreateTranslation(Fwd * amount));
            if (CheckNewPosition(newPos))
            {
                Position = newPos;
                Theta = SetTheta(newPos);
            }
        }

        public void ZoomOut(float amount)
        {
            ZoomIn(-amount);
        }

        public void PanLeft(float amount)
        {
            PanRight(-amount);
        }

        public void PanRight(float amount)
        {
            Vector3 right = Vector3.Cross(Up, Fwd) * -1;
            right.Normalize();

            Vector3 newPos = Vector3.Transform(Position, Matrix.CreateTranslation(right * amount));
            if (CheckNewPosition(newPos))
            {
                Position = newPos;
                Target = Vector3.Transform(Target, Matrix.CreateTranslation(right * amount));
                Theta = SetTheta(newPos);
            }
        }

        public void PanUp(float amount)
        {
            PanDown(-amount);
        }

        public void PanDown(float amount)
        {
            Vector3 newPos = Vector3.Transform(Position, Matrix.CreateTranslation(-Up * amount));
            if (CheckNewPosition(newPos))
            {
                Position = newPos;
                Target = Vector3.Transform(Target, Matrix.CreateTranslation(-Up * amount));
                Theta = SetTheta(newPos);
            }
        }

        private bool CheckNewPosition(Vector3 newPos)
        {
            AngularRange3 newTheta = SetTheta(newPos);
            return Math.Abs(newPos.X) <= Math.Abs(Range.Position.Max.X)
                && Math.Abs(newPos.X) >= Math.Abs(Range.Position.Min.X)
                && Math.Abs(newPos.Y) <= Math.Abs(Range.Position.Max.Y)
                && Math.Abs(newPos.Y) >= Math.Abs(Range.Position.Min.Y)
                && Math.Abs(newPos.Z) <= Math.Abs(Range.Position.Max.Z)
                && Math.Abs(newPos.Z) >= Math.Abs(Range.Position.Min.Z)
                && newTheta.Altitude.Max <= Range.Theta.Altitude.Max
                && newTheta.Altitude.Max >= Range.Theta.Altitude.Min
                && newTheta.Azimuth.Max <= Range.Theta.Azimuth.Max
                && newTheta.Azimuth.Max >= Range.Theta.Azimuth.Min;
        }

        private AngularRange3 SetTheta(Vector3 position)
        {
            Vector3 norm = position;
            norm.Normalize();

            float azimuth = 0f;

            if (norm.Z > 0)
            {
                if (norm.X <= 0)
                    azimuth = MathHelper.Pi + (float)Math.Abs(Math.Asin(norm.X)); // quad 3
                else
                    azimuth = MathHelper.Pi - (float)Math.Abs(Math.Asin(norm.X)); // quad 2
            }
            else if (norm.X < 0 && norm.Z < 0)
                azimuth = MathHelper.TwoPi - (float)Math.Abs(Math.Asin(norm.X)); // quad 4
            else
                azimuth = (float)Math.Abs(Math.Asin(norm.X)); // quad 1

            var theta = new AngularRange3()
            {
                Azimuth = new AngularRange() { Min = 0f, Max = azimuth },
                Altitude = new AngularRange() { Min = 0f, Max = (float)Math.Asin(Vector3.Dot(norm, Vector3.Up)) }
            };
            return theta;
        }
    }
}
