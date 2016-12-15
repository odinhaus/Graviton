using Graviton.XNA.Primitives;
using Graviton.XNA.Shapes.TwoD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Players
{
    public class Genesoid : IMovable3
    {
        private Vector3 _pos = Vector3.Zero;
        public Vector3 Position
        {
            get { return _pos; }
            set
            {
                Vector3 delta = value - _pos;
                face.Position += delta;
                left.Position += delta;
                right.Position += delta;
                butt.Position += delta;
                _pos = value;
            }
        }

        private Vector3 _vel = Vector3.Zero;
        public Vector3 Velocity
        {
            get { return _vel; }
            set
            {
                face.Velocity = value;
                right.Velocity = value;
                left.Velocity = value;
                butt.Velocity = value;
                _vel = value;
            }
        }
        public Color Color = Color.White;
        Arc face;
        Arc right;
        Arc left;
        Arc butt;
        Arc tracking;

        public Genesoid(GraphicsDevice graphics, float mass, Color baseColor)
        {
            Mass = mass;
            Color = baseColor;
            face = new Arc(graphics, Radius, 64, Radius / 4f, Primitives.FillStyle.Center, (float)(Math.PI / 4f), (float)(Math.PI / 2f), baseColor, Radius / 12f, Color.Transparent, Radius / 12f, Color.Transparent);
            right = new Arc(graphics, Radius, 64, Radius / 8f, Primitives.FillStyle.Center, (float)(-Math.PI / 4f), (float)(Math.PI / 2f), Color.Yellow, Radius / 12f, Color.Transparent, Radius / 12f, Color.Transparent);
            left = new Arc(graphics, Radius, 64, Radius / 8f, Primitives.FillStyle.Center, (float)(3f * Math.PI / 4f), (float)(Math.PI / 2f), Color.Green, Radius / 12f, Color.Transparent, Radius / 12f, Color.Transparent);
            butt = new Arc(graphics, Radius, 64, Radius / 4f, Primitives.FillStyle.Center, (float)(5f * Math.PI / 4f), (float)(Math.PI / 2f), baseColor, Radius / 12f, Color.Transparent, Radius / 12f, Color.Transparent);
            tracking = new Arc(graphics, Radius, 64, Radius / 12f, Primitives.FillStyle.Outside, (float)(5f * Math.PI / 4f), (float)(Math.PI / 2f), Color.HotPink, Radius / 12f, Color.Transparent, Radius / 12f, Color.Transparent);
            BoundingSphere = new BoundingSphere(_pos, Radius);
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }

        public float Mass { get; private set; }

        public float Radius
        {
            get
            {
                return (float)Math.Tanh((Mass - 200f) / 500f);
            }
        }

        Matrix scale = Matrix.Identity;

        public bool IsTracking { get; set; }
        public BoundingSphere BoundingSphere { get; private set; }

        public void Update(GameTime gameTime)
        {
            Vector3 dp = Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _pos += dp;
            BoundingSphere = new BoundingSphere(_pos, Radius);
            face.Update(gameTime);
            right.Update(gameTime);
            left.Update(gameTime);
            butt.Update(gameTime);
            if (IsTracking)
                tracking.Update(gameTime);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            face.Draw(view, projection);
            left.Draw(view, projection);
            right.Draw(view, projection);
            butt.Draw(view, projection);
            if (IsTracking)
                tracking.Draw(view, projection);
        }
    }
}
