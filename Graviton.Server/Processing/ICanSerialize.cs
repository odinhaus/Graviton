using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public interface ICanSerialize
    {
        bool IsValid { get; set; }
        byte[] Serialize();
        void Deserialize(byte[] bytes, int offset = 0);
        ushort Type { get; }
    }
}
