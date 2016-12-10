using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    [StructLayout(LayoutKind.Sequential, Size = 4 + 8 + 8 + 1)]
    public class GameStateResponse : ICanSerialize
    {
        public float _WorldSize;
        public ulong _Epoch;
        public long _TimespanTicks;
        public bool _IsValid;

        public bool IsValid { get { return _IsValid; } set { _IsValid = value; } }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf<GameStateResponse>();
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static GameStateResponse Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new GameStateResponse();
            int size = Marshal.SizeOf<GameStateResponse>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (GameStateResponse)Marshal.PtrToStructure(ptr, typeof(GameStateResponse));
            Marshal.FreeHGlobal(ptr);
            return deserialized;
        }
    }
}
