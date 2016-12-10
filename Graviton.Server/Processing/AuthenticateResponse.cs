using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    [StructLayout(LayoutKind.Sequential, Size = 1 + 8 + 1)]
    public class AuthenticateResponse : ICanSerialize
    {
        public bool _IsAuthenticated;
        public ulong _Requester;
        public bool _IsValid;

        public bool IsValid { get { return _IsValid; } set { _IsValid = value; } }
        public byte[] Serialize()
        {
            int size = Marshal.SizeOf<AuthenticateResponse>();
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static AuthenticateResponse Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new AuthenticateResponse();
            int size = Marshal.SizeOf<AuthenticateResponse>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (AuthenticateResponse)Marshal.PtrToStructure(ptr, typeof(AuthenticateResponse));
            Marshal.FreeHGlobal(ptr);
            return deserialized;
        }
    }
}
