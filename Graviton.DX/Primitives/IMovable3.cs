using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Primitives
{
    public interface IMovable3 : IPositionable3
    {
        Vector3 Velocity { get; set; }
    }
}
