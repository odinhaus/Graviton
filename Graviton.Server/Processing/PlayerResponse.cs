using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    [StructLayout(LayoutKind.Sequential)]
    public class PlayerResponse : ICanSerialize
    {
        public bool _IsValid;

        public bool IsValid { get { return _IsValid; } set { _IsValid = value; } }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf<PlayerResponse>();
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static PlayerResponse Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new PlayerResponse();
            int size = Marshal.SizeOf<PlayerResponse>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (PlayerResponse)Marshal.PtrToStructure(ptr, typeof(PlayerResponse));
            Marshal.FreeHGlobal(ptr);
            return deserialized;
        }
    }
}
