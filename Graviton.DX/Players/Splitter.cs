using Graviton.XNA.Primitives;
using Graviton.XNA.Shapes.TwoD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graviton.DX.Players
{
    public class Splitter : IMovable3
    {
        private Vector3 _pos = Vector3.Zero;
        public Vector3 Position
        {
            get { return _pos; }
            set
            {
                Vector3 delta = value - _pos;
                face.Position += delta;
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
                _vel = value;
            }
        }
        public Color Color = Color.White;
        TexturedCircle face;
        //Arc edge;
        //Arc face;
        Arc tracking;
        private readonly float Radius0;

        public Splitter(GraphicsDevice graphics, float radius, Texture2D texture)
        {
            Radius = radius;
            Radius0 = radius;

            face = new TexturedCircle(graphics, radius, 32, texture);
            tracking = new Arc(graphics, radius, 64, radius / 12f, XNA.Primitives.FillStyle.Outside, (float)(5f * Math.PI / 4f), (float)(Math.PI / 2f), Color.HotPink, radius / 12f, Color.Transparent, radius / 12f, Color.Transparent);
            BoundingSphere = new BoundingSphere(_pos, Radius);
        }

        public float Radius { get; private set; }
        public bool IsTracking { get; set; }
        public BoundingSphere BoundingSphere { get; private set; }

        public bool IsActive { get { return _age < 3000; } }

        double _age = 0;
        public void Update(GameTime gameTime)
        {
            Vector3 dp = Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _age += gameTime.ElapsedGameTime.TotalMilliseconds;
            //var a = _age / 20d;
            //var b = a + 0.4f;
            //var c = Math.Pow(b, 2);
            //var d = c / 2000f;
            //var e = d - 2f;

            //Radius = -Radius0 * (float)Math.Tanh(e);
            //face.Radius = Radius;
            _pos += dp;
            BoundingSphere = new BoundingSphere(_pos, Radius);
            face.Update(gameTime);

            if (IsTracking)
                tracking.Update(gameTime);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            if (!IsActive) return;

            face.Draw(view, projection);

            if (IsTracking)
                tracking.Draw(view, projection);
        }
    }
}
