using Graviton.Common.Drawing;
using Graviton.Common.Indexing;
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
    public class Matter : IMovable3
    {
        public static void Initialize(GraphicsDevice graphics, Texture2D texture)
        {
            Face = new TexturedCircle(graphics, 1f, 16, texture)
            {
                Position = Vector3.Zero,
                Velocity = Vector3.Zero,
                Scale = 1f,
                Color = Color.Transparent
            };
        }


        public static Texture2D Texture { get; private set; }
        public float Mass { get; private set; }

        public float Radius
        {
            get
            {
                return 16f * (float)Math.Tanh(Mass / 50000f) + 1;
            }
        }

        public Vector3 Velocity
        {
            get;
            set;
        }

        public Vector3 Position
        {
            get;
            set;
        }

        private BoundingSphere _boundingSphere;
        public BoundingSphere BoundingSphere
        {
            get { return _boundingSphere; }
            private set
            {
                _boundingSphere = value;
            }
        }

        private RectangleF _boundingBox;
        public RectangleF BoundingBox
        {
            get { return _boundingBox; }
            private set
            {
                _boundingBox = value;
            }
        }

        public Matrix Rotation { get; private set; }

        private static TexturedCircle Face;

        public Matter(Vector3 position, Vector3 velocity, float mass, Matrix rotation)
        {
            Position = position;
            Velocity = velocity;
            Mass = mass;
            BoundingSphere = new BoundingSphere(Position, Radius);
            BoundingBox = new RectangleF() { X = Position.X - Radius, Y = Position.Z - Radius, Height = 2f * Radius, Width = 2f * Radius };
            Rotation = rotation;
        }

        public void Update(GameTime gameTime)
        {
            Vector3 dp = Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += dp;
            _boundingSphere.Center = Position;
            _boundingBox.X = Position.X;
            _boundingBox.Y = Position.Z;
            
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Face.Rotation = Rotation;
            Face.Scale = Radius;
            Face.Position = Position;
            Face.Draw(view, projection);
        }

        public QuadTree<IMovable3>.Quad Quad;
        internal bool DrawTexture;

        public void Dispose()
        {
            
        }
    }
}
