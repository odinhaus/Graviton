#region File Description
//-----------------------------------------------------------------------------
// Sphere.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Graviton.XNA.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Graviton.XNA.Shapes.ThreeD
{
    public class Sphere
    {
        private SpherePrimitive primitive;

        public Vector3 Position;
        public Vector3 Velocity;
        public Color Color = Color.White;

        public float Radius { get; private set; }

        public BoundingSphere Bounds
        {
            get { return new BoundingSphere(Position, Radius); }
        }

        public Sphere(GraphicsDevice graphics, float radius, int tellselation, Color baseColor)
        {
            primitive = new SpherePrimitive(graphics, radius * 2f, tellselation, baseColor);
            Radius = radius;
            Color = baseColor;
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            primitive.Draw(Matrix.CreateTranslation(Position), view, projection, Color);
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
    }
}