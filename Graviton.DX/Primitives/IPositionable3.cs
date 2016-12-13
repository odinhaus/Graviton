using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Primitives
{
    public interface IPositionable3 : Graviton.XNA.Primitives.IDrawable
    {
        Vector3 Position { get; set; }
        float Radius { get; }
        BoundingSphere BoundingSphere { get; }
        void Update(GameTime gameTime);
    }
}
