using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public class AuthenticateRequest : ICanSerialize
    {
        _AuthenticateRequest m = new _AuthenticateRequest();

        public string Username { get { return m._Username; } set { m._Username = value; } }
        public string Password { get { return m._Password; } set { m._Password = value; } }
        public bool IsValid { get { return m._IsValid; } set { m._IsValid = value; } }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf(typeof(_AuthenticateRequest));
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public void Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new _AuthenticateRequest();
            int size = Marshal.SizeOf(typeof(_AuthenticateRequest));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (_AuthenticateRequest)Marshal.PtrToStructure(ptr, typeof(_AuthenticateRequest));
            Marshal.FreeHGlobal(ptr);
            m = deserialized;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class _AuthenticateRequest
    {
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 64)]
        public string _Username;
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 64)]
        public string _Password;
        public bool _IsValid;
    }
}
