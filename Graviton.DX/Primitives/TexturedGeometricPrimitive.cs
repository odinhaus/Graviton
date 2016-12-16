using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Primitives
{
    public class TexturedGeometricPrimitive
    {
        #region Fields
        // During the process of constructing a primitive model, vertex
        // and index data is stored on the CPU in these managed lists.
        public List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
        public List<ushort> indices = new List<ushort>();


        // Once all the geometry has been specified, the InitializePrimitive
        // method copies the vertex and index data into these buffers, which
        // store it on the GPU ready for efficient rendering.
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        BasicEffect basicEffect;


        #endregion

        #region Initialization


        /// <summary>
        /// Adds a new vertex to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(Vector3 position, Vector3 normal, Vector2 texturePosition)
        {
            vertices.Add(new VertexPositionNormalTexture(position, normal, texturePosition));
        }

        public virtual void ColorVertex(GraphicsDevice gd, int index, Vector2 texturePosition)
        {
            VertexPositionNormalTexture v = vertices[index];
            v.TextureCoordinate = texturePosition;
            vertices[index] = v;
            UpdateVertexBuffer(gd);
        }

        public virtual void ColorVertices(GraphicsDevice gd, int[] indices, Vector2[] texturePositions)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                VertexPositionNormalTexture v = vertices[indices[i]];
                v.TextureCoordinate = texturePositions[i];
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

        public Texture2D Texture { get; private set; }

        public TexturedGeometricPrimitive(GraphicsDevice graphicsDevice, Texture2D texture)
        {
            GraphicsDevice = graphicsDevice;
            Texture = texture;
        }

        /// <summary>
        /// Once all the geometry has been specified by calling AddVertex and AddIndex,
        /// this method copies the vertex and index data into GPU format buffers, ready
        /// for efficient rendering.
        protected virtual void InitializePrimitive()
        {
            // Create a vertex declaration, describing the format of our vertex data.

            UpdateVertexBuffer(GraphicsDevice);

            // Create an index buffer, and copy our index data into it.
            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort),
                                          indices.Count, BufferUsage.None);

            indexBuffer.SetData(indices.ToArray());

            // Create a BasicEffect, which will be used to render the primitive.
            basicEffect = new BasicEffect(GraphicsDevice);

            //basicEffect.EnableDefaultLighting();
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = Texture;
            basicEffect.PreferPerPixelLighting = false;
            basicEffect.VertexColorEnabled = false;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        }

        public GraphicsDevice GraphicsDevice { get; private set; }


        public virtual void UpdateVertexBuffer(GraphicsDevice graphicsDevice)
        {
            // Create a vertex buffer, and copy our vertex data into it.
            vertexBuffer = new VertexBuffer(graphicsDevice,
                                            typeof(VertexPositionNormalTexture),
                                            vertices.Count, BufferUsage.None);

            vertexBuffer.SetData(vertices.ToArray());
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~TexturedGeometricPrimitive()
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
            //var rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //graphicsDevice.RasterizerState = rs;

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
                //var blend = new BlendState()
                //{
                //    AlphaBlendFunction = BlendFunction.Add,
                //    AlphaSourceBlend = Blend.SourceAlpha,
                //    AlphaDestinationBlend = Blend.InverseSourceAlpha,
                //};
                //device.BlendState = blend; // BlendState.AlphaBlend;
                //var stencil = new DepthStencilState()
                //{
                //    DepthBufferEnable = true,
                //    DepthBufferWriteEnable = false,
                //};
                //device.DepthStencilState = stencil;
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
