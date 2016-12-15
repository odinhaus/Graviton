using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graviton.XNA.Primitives
{
    public interface IDrawable
    {
        void Load();
        void Unload();
        void Draw(Matrix view, Matrix projection);
    }
}
