using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Net
{
    public class SocketState
    {
        static ulong _id;
        static SocketState()
        {
            _id++;
        }

        public SocketState()
        {
            Id = _id;
        }

        public const int BUFFER_SIZE = 1024 * 1;
        public byte[] Buffer = new byte[BUFFER_SIZE];
        public Socket Socket;
        public ulong Id;
        public int Offset;
        public ulong Requester;
    }
}
