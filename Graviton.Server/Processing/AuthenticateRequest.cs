using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    [StructLayout(LayoutKind.Sequential, Size = 64 + 64 + 1)]
    public unsafe class AuthenticateRequest : ICanSerialize
    {
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 64)]
        public string _Username;
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 64)]
        public string _Password;
        public bool _IsValid;

        public bool IsValid { get { return _IsValid; } set { _IsValid = value; } }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf<AuthenticateRequest>();
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static AuthenticateRequest Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new AuthenticateRequest();
            int size = Marshal.SizeOf<AuthenticateRequest>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (AuthenticateRequest)Marshal.PtrToStructure(ptr, typeof(AuthenticateRequest));
            Marshal.FreeHGlobal(ptr);
            return deserialized;
        }
    }
}
