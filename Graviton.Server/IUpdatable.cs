using Graviton.Server.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server
{
    public interface IUpdatable : ICanSerialize
    {
        bool IsNew { get; set; }
        ulong LastUpdate { get; }
        void Update(GameTime gameTime);
    }
}
