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
    public class Arc
    {
        private ColoredArcPrimitive primitive;

        public Vector3 Position;
        public Vector3 Velocity;
        public Color Color = Color.White;
        public float Scale = 1f;

        public float Radius { get; private set; }

        public Arc(GraphicsDevice graphics, float radius, int tellselation, float thickness, FillStyle style, float startAngle, float sweepAngle, Color baseColor)
            : this(graphics, radius, tellselation, thickness, style, startAngle, sweepAngle, baseColor, null) { }

        public Arc(GraphicsDevice graphics, float radius, int tellselation, float thickness, FillStyle style, float startAngle, float sweepAngle, Color baseColor, Effect customEffect) 
            : this(graphics, radius, tellselation, thickness, style, startAngle, sweepAngle, baseColor, 0f, Color.Transparent, 0f, Color.Transparent, customEffect)
        {
        }
        public Arc(GraphicsDevice graphics, float radius, int tellselation, float thickness, FillStyle style, float startAngle, float sweepAngle, Color baseColor, float outerEdgeThickness, Color outerEdgeColor, float innerEdgeThickness, Color innerEdgeColor)
            : this(graphics, radius, tellselation, thickness, style, startAngle, sweepAngle, baseColor, outerEdgeThickness, outerEdgeColor, outerEdgeThickness, outerEdgeColor, null)
        {
        }

        public Arc(GraphicsDevice graphics, float radius, int tellselation, float thickness, FillStyle style, float startAngle, float sweepAngle, Color baseColor, float outerEdgeThickness, Color outerEdgeColor, float innerEdgeThickness, Color innerEdgeColor, Effect customEffect)
        {
            primitive = new ColoredArcPrimitive(graphics, radius, tellselation, thickness, style, startAngle, sweepAngle, baseColor, outerEdgeThickness, outerEdgeColor, innerEdgeThickness, innerEdgeColor);
            primitive.CustomEffect = customEffect;
            Radius = radius;
            Color = baseColor;
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            primitive.Draw(Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position), view, projection, Color);
        }

        public void ColorVertex(GraphicsDevice gd, int index, Color color)
        {
            primitive.ColorVertex(gd, index, color);
        }

        public void ColorVertices(GraphicsDevice gd, int[] indices, Color[] colors)
        {
            primitive.ColorVertices(gd, indices, colors);
        }

        public int Vertices { get { return primitive.Vertices; } }

        public BoundingSphere Bounds
        {
            get { return new BoundingSphere(Position, Radius); }
        }
    }
}
