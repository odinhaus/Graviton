using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public class PlayerStateResponse : ICanSerialize
    {
        _PlayerStateResponse m = new _PlayerStateResponse();

        public bool IsValid { get { return m._IsValid; } set { m._IsValid = value; } }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf(typeof(_PlayerStateResponse));
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public void Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new _PlayerStateResponse();
            int size = Marshal.SizeOf(typeof(_PlayerStateResponse));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (_PlayerStateResponse)Marshal.PtrToStructure(ptr, typeof(_PlayerStateResponse));
            Marshal.FreeHGlobal(ptr);
            m = deserialized;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class _PlayerStateResponse
    {
        public bool _IsValid;
    }
}
