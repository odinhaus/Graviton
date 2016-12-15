using Graviton.Common.Drawing;
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
    public class Circle : IMovable3, IDisposable
    {
        private ColoredCirclePrimitive primitive;

        Vector3 _position;
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _boundingSphere.Center = value;
                _boundingBox = new RectangleF()
                {
                    X = value.X - Radius / 2f,
                    Y = value.Z - Radius / 2f,
                    Width = Radius * 2f,
                    Height = Radius * 2f
                };
            }
        }
        public Vector3 Velocity { get; set; }
        public Color CenterColor = Color.White;
        public Color EdgeColor = Color.White;
        private GraphicsDevice Graphics;

        public float Radius { get; private set; }

        private BoundingSphere _boundingSphere;
        public BoundingSphere BoundingSphere
        {
            get { return _boundingSphere; }
        }

        private RectangleF _boundingBox;
        public RectangleF BoundingBox
        {
            get { return _boundingBox; }
        }

        public Circle(GraphicsDevice graphics, float radius, int tellselation, Color centerColor, Color edgeColor)
            : this(graphics, radius, tellselation, centerColor, edgeColor, null)
        {
        }

        public Circle(GraphicsDevice graphics, float radius, int tellselation, Color centerColor, Color edgeColor, Effect customEffect) 
        {
            primitive = new ColoredCirclePrimitive(graphics, radius * 2f, tellselation, centerColor, edgeColor);
            primitive.CustomEffect = customEffect;
            Radius = radius;
            Position = Vector3.Zero;
            CenterColor = centerColor;
            EdgeColor = edgeColor;
            Graphics = graphics;
            Reference = primitive.vertices.ToArray();
            _boundingSphere = new BoundingSphere(Position, Radius);
            _boundingBox = new RectangleF()
            {
                X = Position.X - Radius / 2f,
                Y = Position.Y - Radius / 2f,
                Width = Radius * 2f,
                Height = Radius * 2f
            };
        }

        ulong _frame = 0;
        private readonly ColoredVertexPositionNormal[] Reference;
        internal float Scale = 1f;

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frame++;
            

            //var alpha = (_frame / 15d) / Math.PI;
            //var delta = 0.005 * Radius * Math.Sin(alpha);

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
            //    var dy = flip * delta * Math.Sign(angle);

            //    pos.X = (float)(pos.X + dx);
            //    pos.Z = (float)(pos.Z + dy);

            //    vertex.Position = pos;
            //    primitive.vertices[v] = vertex;
            //}
            //primitive.UpdateVertexBuffer(Graphics);

        }

        public void Draw(Matrix view, Matrix projection)
        {
            primitive.Draw(Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position), view, projection, EdgeColor);
        }

        public void ColorVertex(GraphicsDevice gd, int index, Color color)
        {
            primitive.ColorVertex(gd, index, color);
        }

        public void ColorVertices(GraphicsDevice gd, int[] indices, Color[] colors)
        {
            primitive.ColorVertices(gd, indices, colors);
        }

        public void Dispose()
        {
            primitive.Dispose();
        }

        public int Vertices { get { return primitive.Vertices; } }

    }
}
