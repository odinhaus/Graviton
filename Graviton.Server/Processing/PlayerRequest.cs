using Graviton.Server.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    [StructLayout(LayoutKind.Sequential, Size=8+8+4+4+4+4+4+4+1+1)]
    public unsafe class PlayerRequest : ICanSerialize
    {
        public ulong _Requester;
        public ulong _KeyStateMask;
        public float _Vector_X;
        public float _Vector_Y;
        public float _Viewport_X;
        public float _Viewport_Y;
        public float _Viewport_W;
        public float _Viewport_H;
        public bool _IsFirstRequest;
        public bool _IsValid;
        public SocketState SocketState { get; set; }
        public bool IsValid { get { return _IsValid; } set { _IsValid = value; } }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf<PlayerRequest>();
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static PlayerRequest Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new PlayerRequest();
            int size = Marshal.SizeOf<PlayerRequest>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (PlayerRequest)Marshal.PtrToStructure(ptr, typeof(PlayerRequest));
            Marshal.FreeHGlobal(ptr);
            return deserialized;
        }
    }
}
