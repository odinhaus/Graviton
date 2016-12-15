using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Primitives
{
    public class ColoredCirclePrimitive : GeometricPrimitive
    {
        public ColoredCirclePrimitive(GraphicsDevice graphicsDevice, float diameter, int tessellation, Color centerColor, Color edgeColor)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            CenterColor = centerColor;
            EdgeColor = edgeColor;

            // start with a vertex at the center
            float radius = diameter / 2f;
            AddVertex(Vector3.Zero, Vector3.Up, CenterColor);

            float pi2 = (float)(Math.PI * 2f);
            float delta =  pi2 / tessellation;
            float alpha = 0;
            int current = 0;
            while (alpha < pi2)
            {
                if (current > 1)
                {
                    AddIndex(0);
                    AddIndex(current - 1);
                    AddIndex(current);
                }
                Vector3 vtx = new Vector3((float)Math.Cos(alpha), 0, (float)Math.Sin(alpha)) * radius;
                AddVertex(vtx, Vector3.Up, EdgeColor);
                
                alpha += delta;
                current++;
            }
            AddIndex(0);
            AddIndex(current - 1);
            AddIndex(current);
            AddIndex(0);
            AddIndex(current);
            AddIndex(1);
            InitializePrimitive(graphicsDevice);
        }

        public Color CenterColor { get; private set; }
        public Color EdgeColor { get; private set; }
    }
}
