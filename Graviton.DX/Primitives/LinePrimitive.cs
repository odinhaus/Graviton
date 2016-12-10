using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graviton.XNA.Primitives
{
    public class LinePrimitive : GeometricPrimitive
    {
        public LinePrimitive(GraphicsDevice graphics, float length, float thickness, int tesselation, Color color)
        {
            this.Graphics = graphics;
            this.Length = length;
            this.Thickness = thickness;
            this.Tesselation = tesselation;
            this.Color = color;

            AddVertex(Vector3.Zero, Vector3.Up, color);
            AddVertex(new Vector3(length, 0f, 0f), Vector3.Up, color);
            AddVertex(new Vector3(length, 0f, thickness), Vector3.Up, color);
            AddVertex(new Vector3(0f, 0f, thickness), Vector3.Up, color);

            AddIndex(0);
            AddIndex(1);
            AddIndex(2);
            AddIndex(2);
            AddIndex(3);
            AddIndex(0);
            InitializePrimitive(graphics);
        }

        public Color Color { get; private set; }
        public GraphicsDevice Graphics { get; private set; }
        public float Length { get; private set; }
        public int Tesselation { get; private set; }
        public float Thickness { get; private set; }
    }
}
