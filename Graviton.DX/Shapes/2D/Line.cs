using Graviton.XNA.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graviton.XNA.Shapes.TwoD
{
    public class Line
    {
        private LinePrimitive primitive;

        public Vector3 Position;
        public Vector3 Velocity;
        public Color Color = Color.White;

        public Line(GraphicsDevice graphics, float length, float thickness, int tesselation, Color color)
        {
            primitive = new LinePrimitive(graphics, length, thickness, tesselation, color);
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            primitive.Draw(Matrix.CreateTranslation(Position), view, projection, Color);
        }
    }
}
