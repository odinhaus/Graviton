using Graviton.Server.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    [StructLayout(LayoutKind.Sequential)]
    public class PlayerRequest : ICanSerialize
    {
        _PlayerRequest m = new _PlayerRequest();

        public SocketState SocketState { get; set; }

        public ulong Requester { get { return m._Requester; } set { m._Requester = value; } }
        public ulong KeyStateMask { get { return m._KeyStateMask; } set { m._KeyStateMask = value; } }
        public float Vector_X { get { return m._Vector_X; } set { m._Vector_X = value; } }
        public float Vector_Y { get { return m._Vector_Y; } set { m._Vector_Y = value; } }
        public float ViewPort_X { get { return m._Viewport_X; } set { m._Viewport_X = value; } }
        public float ViewPort_Y { get { return m._Viewport_Y; } set { m._Viewport_Y = value; } }
        public float ViewPort_W { get { return m._Viewport_W; } set { m._Viewport_W = value; } }
        public float ViewPort_H { get { return m._Viewport_H; } set { m._Viewport_H = value; } }
        public bool IsFirstRequest { get { return m._IsFirstRequest; } set { m._IsFirstRequest = value; } }

        public bool IsValid { get { return m._IsValid; } set { m._IsValid = value; } }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf(typeof(_PlayerRequest));
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public void Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new _PlayerRequest();
            int size = Marshal.SizeOf(typeof(_PlayerRequest));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (_PlayerRequest)Marshal.PtrToStructure(ptr, typeof(_PlayerRequest));
            Marshal.FreeHGlobal(ptr);
            m = deserialized;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class _PlayerRequest
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
    }
}
