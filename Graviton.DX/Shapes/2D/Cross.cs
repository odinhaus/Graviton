using Graviton.XNA.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graviton.XNA.Shapes.TwoD
{
    public class Cross
    {
        private LinePrimitive primitive;

        public Vector3 Position;
        public Vector3 Velocity;
        public Color Color = Color.White;
        private float Length;
        private float Thickness;

        public Cross(GraphicsDevice graphics, float length, float thickness, int tesselation, Color color)
        {
            primitive = new LinePrimitive(graphics, length, thickness, tesselation, color);
            var rotate = Matrix.CreateRotationY((float)Math.PI / 2f);
            this.Length = length;
            this.Thickness = thickness;
        }

        public void Update(GameTime gameTime)
        {
            //Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            var scaleFactor = view.Translation.Z / -100f;
            var scale = Matrix.CreateScale(scaleFactor);
            var center = Matrix.CreateTranslation(new Vector3(-scaleFactor * Length / 2f, 0, -scaleFactor * Thickness / 2f));
            var rotate = Matrix.CreateRotationY((float)Math.PI / 2f);
            var position = Matrix.CreateTranslation(new Vector3(Position.X + Length / 2f, Position.Y, Position.Z + Thickness /2f));
           

            rotate.Translation = new Vector3(-Length / 2f, 0, -Thickness / 2f);

            primitive.Draw(scale * Matrix.CreateTranslation(new Vector3(Position.X - scaleFactor * Length / 2f, Position.Y, Position.Z - scaleFactor * Thickness / 2f)), view, projection, Color);
            primitive.Draw(scale * (center * rotate * position), view, projection, Color);
        }
    }
}
