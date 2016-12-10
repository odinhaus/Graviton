using Graviton.Server.Drawing;
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
        RectangleF Bounds { get; }
    }
}
