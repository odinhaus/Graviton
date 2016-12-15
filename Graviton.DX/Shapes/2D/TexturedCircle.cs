using Graviton.XNA.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Shapes.TwoD
{
    public class TexturedCircle
    {
        private TexturedCirclePrimitive primitive;

        public Vector3 Position;
        public Vector3 Velocity;
        public Color Color = Color.TransparentBlack;
        private GraphicsDevice Graphics;
        public float Scale = 1f;

        float _radius;
        public float Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                var dr = value - _radius;
                if (dr != 0f && _radius > 0f)
                {
                    _radius = value;
                    ChangeRadius(dr);
                }
                else if (_radius == 0f)
                {
                    _radius = value;
                }
            }
        }

        

        public BoundingSphere Bounds
        {
            get { return new BoundingSphere(Position, Radius); }
        }

        public TexturedCircle(GraphicsDevice graphics, float radius, int tellselation, Texture2D texture)
            : this(graphics, radius, tellselation, texture, null)
        {
        }

        public TexturedCircle(GraphicsDevice graphics, float radius, int tesellelation, Texture2D texture, Effect customEffect)
        {
            Tesellelation = tesellelation;

            CustomEffect = customEffect;
            Graphics = graphics;
            Texture = texture;
            Radius = radius;
            Rotation = Matrix.Identity;
        }

        ulong _frame = 0;
        private VertexPositionNormalTexture[] Reference;
        private Texture2D Texture;
        public Matrix Rotation;
        private int Tesellelation;
        private Effect CustomEffect;

        public void Load()
        {
            primitive = new TexturedCirclePrimitive(Graphics, Radius * 2f, Tesellelation, Texture);
            Reference = primitive.vertices.ToArray();
            primitive.CustomEffect = CustomEffect;
        }

        public void Unload()
        {
            primitive.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //_frame++;

            //var alpha = (_frame / 15d) / Math.PI;
            //var delta = 0.008 * Radius * Math.Sin(alpha);

            //var r = new Random();
            //for (int v = 1; v < primitive.Vertices; v++)
            //{
            //    var flip = 1;
            //    if (v % 2 == 0)
            //        flip = -1;
            //    var vertex = Reference[v];
            //    var pos = vertex.Position;


            //    var angle = Math.Atan(pos.Z / pos.X);

            //    var dx = flip * delta * Math.Cos(angle);
            //    var dy = flip * delta * Math.Sin(angle);

            //    pos.X = (float)(pos.X + dx);
            //    pos.Z = (float)(pos.Z + dy);

            //    vertex.Position = pos;
            //    primitive.vertices[v] = vertex;
            //}
            //primitive.UpdateVertexBuffer(Graphics);

        }

        private void ChangeRadius(float dr)
        {
            float pi2 = (float)(Math.PI * 2f);
            float delta = pi2 / primitive.Tessellation;
            float alpha = 0;
            int v = 1;
            while(alpha < pi2)
            {
                var vertex = Reference[v];
                var pos = vertex.Position;

                var dx = dr * Math.Cos(alpha);
                var dy = dr * Math.Sin(alpha);

                pos.X = (float)(pos.X + dx);
                pos.Z = (float)(pos.Z + dy);

                vertex.Position = pos;
                primitive.vertices[v] = vertex;
                alpha += delta;
                v++;
            }

            primitive.UpdateVertexBuffer(Graphics);
            Reference = primitive.vertices.ToArray();
        }

        public void Draw(Matrix view, Matrix projection)
        {
            primitive.Draw(Rotation * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position), view, projection, Color);
        }

        public int Vertices { get { return primitive.Vertices; } }

    }
}
