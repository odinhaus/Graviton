﻿using Graviton.Server.Drawing;
using Graviton.Server.Indexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server
{
    public interface IMovable : IUpdatable
    {
        float X { get; set; }
        float Y { get; set; }
        float Vx { get; set; }
        float Vy { get; set; }
        float Fx { get; set; }
        float Fy { get; set; }
        float Radius { get; }
        float Mass { get; }
        RectangleF Bounds { get; }
        QuadTree<IMovable>.Quad Quad { get; set; }
    }
}
