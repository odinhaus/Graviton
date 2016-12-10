using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public class AuthenticateResponse : ICanSerialize
    {
        _AuthenticateResponse m = new _AuthenticateResponse();
        public ulong Requester { get { return m._Requester; } set { m._Requester = value; } }
        public bool IsAuthenticated { get { return m._IsAuthenticated; } set { m._IsAuthenticated = value; } }
        public bool IsValid { get { return m._IsValid; } set { m._IsValid = value; } }
        public byte[] Serialize()
        {
            int size = Marshal.SizeOf(typeof(_AuthenticateResponse));
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public void Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new _AuthenticateResponse();
            int size = Marshal.SizeOf(typeof(_AuthenticateResponse));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (_AuthenticateResponse)Marshal.PtrToStructure(ptr, typeof(_AuthenticateResponse));
            Marshal.FreeHGlobal(ptr);
            m = deserialized;   
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class _AuthenticateResponse
    {
        public bool _IsAuthenticated;
        public ulong _Requester;
        public bool _IsValid;
    }
}
