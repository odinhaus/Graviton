#region File Description
//-----------------------------------------------------------------------------
// GeometricPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
#endregion

namespace Graviton.XNA.Primitives
{
    public struct ColoredVertexPositionNormal : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColoredVertexPositionNormal(Vector3 position, Vector3 normal, Color color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }

        /// <summary>
        /// A VertexDeclaration object, which contains information about the vertex
        /// elements contained within this struct.
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float)* 3* 2, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return ColoredVertexPositionNormal.VertexDeclaration; }
        }

    }
 
    /// <summary>
    /// Base class for simple geometric primitive models. This provides a vertex
    /// buffer, an index buffer, plus methods for drawing the model. Classes for
    /// specific types of primitive (CubePrimitive, SpherePrimitive, etc.) are
    /// derived from this common base, and use the AddVertex and AddIndex methods
    /// to specify their geometry.
    /// 
    /// This class is borrowed from the Primitives3D sample.
    /// </summary>
    public abstract class GeometricPrimitive : IDisposable
    {
        #region Fields


        // During the process of constructing a primitive model, vertex
        // and index data is stored on the CPU in these managed lists.
        public List<ColoredVertexPositionNormal> vertices = new List<ColoredVertexPositionNormal>();
        public List<ushort> indices = new List<ushort>();


        // Once all the geometry has been specified, the InitializePrimitive
        // method copies the vertex and index data into these buffers, which
        // store it on the GPU ready for efficient rendering.
        DynamicVertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        BasicEffect basicEffect;


        #endregion

        #region Initialization


        /// <summary>
        /// Adds a new vertex to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(Vector3 position, Vector3 normal, Color color)
        {
            vertices.Add(new ColoredVertexPositionNormal(position, normal, color));
        }

        public virtual void ColorVertex(GraphicsDevice gd, int index, Color color)
        {
            ColoredVertexPositionNormal v = vertices[index];
            v.Color = color;
            vertices[index] = v;
            UpdateVertexBuffer(gd);
        }

        public virtual void ColorVertices(GraphicsDevice gd, int[] indices, Color[] colors)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                ColoredVertexPositionNormal v = vertices[indices[i]];
                v.Color = colors[i];
                vertices[indices[i]] = v;
            }
            UpdateVertexBuffer(gd);
        }

        /// <summary>
        /// Adds a new index to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            indices.Add((ushort)index);
        }


        /// <summary>
        /// Queries the index of the current vertex. This starts at
        /// zero, and increments every time AddVertex is called.
        /// </summary>
        protected int CurrentVertex
        {
            get { return vertices.Count; }
        }


        /// <summary>
        /// Once all the geometry has been specified by calling AddVertex and AddIndex,
        /// this method copies the vertex and index data into GPU format buffers, ready
        /// for efficient rendering.
        protected virtual void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            // Create a vertex declaration, describing the format of our vertex data.

            UpdateVertexBuffer(graphicsDevice);

            // Create an index buffer, and copy our index data into it.
            indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort),
                                          indices.Count, BufferUsage.None);

            indexBuffer.SetData(indices.ToArray());

            // Create a BasicEffect, which will be used to render the primitive.
            basicEffect = new BasicEffect(graphicsDevice);

            basicEffect.EnableDefaultLighting();
            basicEffect.PreferPerPixelLighting = false;
            basicEffect.VertexColorEnabled = true;
            basicEffect.LightingEnabled = true; // turn on the lighting subsystem.
            //basicEffect.DirectionalLight0.DiffuseColor = new Vector3(1f, 0.8f, 0.8f); // a red light
            //basicEffect.DirectionalLight0.Direction = new Vector3(0.1f, -1f, 0.3f);  // coming along the x-axis
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(1f, 1f, 0.8f); // with green highlight
        }


        public virtual void UpdateVertexBuffer(GraphicsDevice graphicsDevice)
        {
            // Create a vertex buffer, and copy our vertex data into it.
            vertexBuffer = new DynamicVertexBuffer(graphicsDevice,
                                            typeof(ColoredVertexPositionNormal),
                                            vertices.Count, BufferUsage.None);

            vertexBuffer.SetData(vertices.ToArray());
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~GeometricPrimitive()
        {
            Dispose(false);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (vertexBuffer != null)
                    vertexBuffer.Dispose();

                if (indexBuffer != null)
                    indexBuffer.Dispose();

                if (basicEffect != null)
                    basicEffect.Dispose();
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the primitive model, using the specified effect. Unlike the other
        /// Draw overload where you just specify the world/view/projection matrices
        /// and color, this method does not set any renderstates, so you must make
        /// sure all states are set to sensible values before you call it.
        /// </summary>
        public void Draw(Effect effect)
        {
            GraphicsDevice graphicsDevice = effect.GraphicsDevice;

            // Set our vertex declaration, vertex buffer, and index buffer.
            graphicsDevice.SetVertexBuffer(vertexBuffer);

            graphicsDevice.Indices = indexBuffer;
            


            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                int primitiveCount = indices.Count / 3;

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                     vertices.Count, 0, primitiveCount);

            }
        }

        public Effect CustomEffect { get; set; }
        public bool HasCustomEffect { get { return CustomEffect != null; } }

        /// <summary>
        /// Draws the primitive model, using a BasicEffect shader with default
        /// lighting. Unlike the other Draw overload where you specify a custom
        /// effect, this method sets important renderstates to sensible values
        /// for 3D model rendering, so you do not need to set these states before
        /// you call it.
        /// </summary>
        public void Draw(Matrix world, Matrix view, Matrix projection, Color color)
        {
            Effect e = null;
            if (HasCustomEffect)
            {
                if (CustomEffect is BasicEffect)
                {
                    ((BasicEffect)CustomEffect).World = world;
                    ((BasicEffect)CustomEffect).View = view;
                    ((BasicEffect)CustomEffect).Projection = projection;
                    
                } 
                e = CustomEffect;
            }
            else
            {
                // Set BasicEffect parameters.
                basicEffect.World = world;
                basicEffect.View = view;
                basicEffect.Projection = projection;
                e = basicEffect;
            }

            GraphicsDevice device = basicEffect.GraphicsDevice;
            device.DepthStencilState = DepthStencilState.Default;

            if (color.A < 255)
            {
                // Set renderstates for alpha blended rendering.
                device.BlendState = BlendState.AlphaBlend;
            }
            else
            {
                // Set renderstates for opaque rendering.
                device.BlendState = BlendState.Opaque;
            }

            // Draw the model, using BasicEffect.
            Draw(e);
        }


        public int Vertices { get { return vertices.Count; } }

        #endregion
    }
}