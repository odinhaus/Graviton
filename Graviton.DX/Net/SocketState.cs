using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Graviton.Server.Net
{
    public class SocketState
    {
        public const int BUFFER_SIZE = 1024 * 1;
        public byte[] Buffer = new byte[BUFFER_SIZE];
        public Socket Socket;
        public int Offset;
        public ulong Requester;
        public uint PendingMessageType;
    }
}
