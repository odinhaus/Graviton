using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public class GameStateResponse : ICanSerialize
    {
        _GameStateResponse m = new _GameStateResponse();

        public float WorldSize { get { return m._WorldSize; } set { m._WorldSize = value; } }
        public ulong Epoch { get { return m._Epoch; } set { m._Epoch = value; } }
        public long TimespanTicks { get { return m._TimespanTicks; } set { m._TimespanTicks = value; } }
        public bool IsValid { get { return m._IsValid; } set { m._IsValid = value; } }
        public ushort Type
        {
            get
            {
                return (ushort)ItemTypeId.GameStateResponse;
            }
        }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf(typeof(_GameStateResponse));
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public void Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new _GameStateResponse();
            int size = Marshal.SizeOf(typeof(_GameStateResponse));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (_GameStateResponse)Marshal.PtrToStructure(ptr, typeof(_GameStateResponse));
            Marshal.FreeHGlobal(ptr);
            m = deserialized;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class _GameStateResponse
    {
        public float _WorldSize;
        public ulong _Epoch;
        public long _TimespanTicks;
        public bool _IsValid;
    }
}
