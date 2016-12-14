using Graviton.XNA.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Graviton.XNA.Shapes.TwoD;

namespace Graviton.XNA.Players
{
    public class Disc : IMovable3
    {
        private Vector3 _pos = Vector3.Zero;
        public Vector3 Position
        {
            get { return _pos; }
            set
            {
                Vector3 delta = value - _pos;
                face.Position += delta;
                //edge.Position += delta;
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
                //edge.Velocity = value;
                _vel = value;
            }
        }
        
        TexturedCircle face;
        //Circle edge;
        //Arc edge;
        //Arc face;
        Arc tracking;
        internal bool DrawTexture;

        public Disc(GraphicsDevice graphics, float mass, Texture2D texture)
        {
            Mass = mass;

            //edge = new Circle(graphics, Radius + 0.03f, 48, Color.White, new Color(255, 255, 255, 0));
            //edge = new Arc(graphics, Radius, 64, 0.15f, FillStyle.Center, 0f, (float)Math.PI * 2f, Color.White, 0.05f, Color.TransparentBlack, 0f, Color.TransparentBlack);
            face = new TexturedCircle(graphics, Radius * 1f, 48, texture);
            face.Rotation = Matrix.CreateRotationY(0f);
            //edge.Position = new Vector3(0f, -0.2f, 0f);
            tracking = new Arc(graphics, Radius, 64, Radius / 12f, Primitives.FillStyle.Outside, (float)(5f * Math.PI / 4f), (float)(Math.PI / 2f), Color.HotPink, Radius / 12f, Color.Transparent, Radius / 12f, Color.Transparent);
            BoundingSphere = new BoundingSphere(_pos, Radius);
        }

        public float Mass { get; set; }

        public float Radius
        {
            get
            {
                return 16f * (float)Math.Tanh(Mass / 50000f) + 0.02f;
            }
        }

        public bool IsTracking { get; set; }
        public BoundingSphere BoundingSphere { get; private set; }

        public void Update(GameTime gameTime)
        {
            Vector3 dp = Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _pos += dp;
            BoundingSphere = new BoundingSphere(_pos, Radius);
            //edge.Update(gameTime);
            face.Update(gameTime);

            //Mass += (float)gameTime.ElapsedGameTime.TotalMilliseconds * 5f;

            ///edge.Scale = Radius;
            face.Scale = Radius;
           
            if (IsTracking)
                tracking.Update(gameTime);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            //edge.Draw(view, projection);
            if (DrawTexture)
                face.Draw(view, projection);
            
            if (IsTracking)
                tracking.Draw(view, projection);
        }
    }
}
