using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Primitives
{
    public enum FillStyle
    {
        Inside,
        Outside,
        Center
    }

    public class ColoredArcPrimitive : GeometricPrimitive
    {
        public ColoredArcPrimitive(GraphicsDevice graphicsDevice, 
            float radius, int tessellation, float thickness, 
            FillStyle style, float startAngle, float sweepAngle, 
            Color baseColor, 
            float outerEdgeThickness, Color outerEdgeColor,
            float innerEdgeThickness, Color innerEdgeColor)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            if (sweepAngle > (float)(Math.PI * 2)) sweepAngle = (float)(Math.PI * 2);

            BaseColor = baseColor;

            float delta = sweepAngle / tessellation;
            float alpha = startAngle;
            float swept = 0f;
            int current = 0;
            int inc = 2;
            if (outerEdgeThickness > 0f) inc++;
            if (innerEdgeThickness > 0f) inc++;

            while (Math.Abs(swept) < Math.Abs(sweepAngle))
            {
                switch (style)
                {
                    case FillStyle.Center:
                        if (innerEdgeThickness > 0f)
                        {
                            AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius - (thickness / 2f) - innerEdgeThickness)), 0, (float)(Math.Sin(alpha) * (radius - (thickness / 2f) - innerEdgeThickness))), Vector3.Up, innerEdgeColor);
                        }
                        AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius - thickness / 2f)), 0, (float)(Math.Sin(alpha) * (radius - thickness / 2f))), Vector3.Up, BaseColor);
                        AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius + thickness / 2f)), 0, (float)(Math.Sin(alpha) * (radius + thickness / 2f))), Vector3.Up, BaseColor);
                        if (outerEdgeThickness > 0f)
                        {
                            AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius + (thickness / 2f) + outerEdgeThickness)), 0, (float)(Math.Sin(alpha) * (radius + (thickness / 2f) + outerEdgeThickness))), Vector3.Up, outerEdgeColor);
                        }
                        break;
                    case FillStyle.Inside:
                        if (innerEdgeThickness > 0f)
                        {
                            AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius - thickness - innerEdgeThickness)), 0, (float)(Math.Sin(alpha) * (radius - thickness - innerEdgeThickness))), Vector3.Up, innerEdgeColor);
                        }
                        AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius - thickness)), 0, (float)(Math.Sin(alpha) * (radius - thickness))), Vector3.Up, BaseColor);
                        AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius)), 0, (float)(Math.Sin(alpha) * (radius))), Vector3.Up, BaseColor);
                        if (outerEdgeThickness > 0f)
                        {
                            AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius + outerEdgeThickness)), 0, (float)(Math.Sin(alpha) * (radius + outerEdgeThickness))), Vector3.Up, outerEdgeColor);
                        }
                        break;
                    case FillStyle.Outside:
                        if (innerEdgeThickness > 0f)
                        {
                            AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius - innerEdgeThickness)), 0, (float)(Math.Sin(alpha) * (radius - innerEdgeThickness))), Vector3.Up, innerEdgeColor);
                        }
                        AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius)), 0, (float)(Math.Sin(alpha) * (radius))), Vector3.Up, BaseColor);
                        AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius + thickness)), 0, (float)(Math.Sin(alpha) * (radius + thickness))), Vector3.Up, BaseColor);
                        if (outerEdgeThickness > 0f)
                        {
                            AddVertex(new Vector3((float)(Math.Cos(alpha) * (radius + thickness + outerEdgeThickness)), 0, (float)(Math.Sin(alpha) * (radius + thickness + outerEdgeThickness))), Vector3.Up, outerEdgeColor);
                        }
                        break;
                }
                
                alpha += delta;
                current += inc;
                swept += delta;

                if (current > inc)
                {
                    if (outerEdgeThickness > 0 && innerEdgeThickness > 0f)
                    {
                        AddIndex(current - 8); // inner
                        AddIndex(current - 7);
                        AddIndex(current - 4);

                        AddIndex(current - 3);
                        AddIndex(current - 4);
                        AddIndex(current - 7);

                        AddIndex(current - 7); // center
                        AddIndex(current - 6);
                        AddIndex(current - 3);

                        AddIndex(current - 2);
                        AddIndex(current - 3);
                        AddIndex(current - 6);

                        AddIndex(current - 6); // outer
                        AddIndex(current - 5);
                        AddIndex(current - 2);

                        AddIndex(current - 1);
                        AddIndex(current - 2);
                        AddIndex(current - 5);
                    }
                    else if (outerEdgeThickness > 0 || innerEdgeThickness > 0f)
                    {
                        AddIndex(current - 6); 
                        AddIndex(current - 5);
                        AddIndex(current - 3);

                        AddIndex(current - 2);
                        AddIndex(current - 3);
                        AddIndex(current - 5);

                        AddIndex(current - 5); 
                        AddIndex(current - 4);
                        AddIndex(current - 2);

                        AddIndex(current - 1);
                        AddIndex(current - 2);
                        AddIndex(current - 4);
                    }
                    else
                    {
                        AddIndex(current - 4);
                        AddIndex(current - 3);
                        AddIndex(current - 2);

                        AddIndex(current - 1);
                        AddIndex(current - 2);
                        AddIndex(current - 3);
                    }
                }
            }

            InitializePrimitive(graphicsDevice);
        }

        public Color BaseColor { get; private set; }
    }
}
