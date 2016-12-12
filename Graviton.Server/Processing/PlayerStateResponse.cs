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

        public double T { get { return m._T; } set { m._T = value; } }
        public double dT { get { return m._dT; } set { m._dT = value; } }
        public float X { get { return m._X; } set { m._X = value; } }
        public float MaxSpeed { get { return m._MaxSpeed; } set { m._MaxSpeed = value; } }
        public float Y { get { return m._Y; } set { m._Y = value; } }
        public uint Mass { get { return m._Mass; } set { m._Mass = value; } }
        public float Vx { get { return m._Vx; } set { m._Vx = value; } }
        public float Vy { get { return m._Vy; } set { m._Vy = value; } }
        public ulong LastUpdate { get { return m._LastUpdate; } set { m._LastUpdate = value; } }
        public ulong LocalEpoch { get { return m._LocalEpoch; } set { m._LocalEpoch = value; } }

        public ushort Type
        {
            get
            {
                return (ushort)ItemTypeId.PlayerStateResponse;
            }
        }

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
        public double _T;
        public double _dT;
        public float _X;
        public float _Y;
        public float _MaxSpeed;
        public uint _Mass;
        public float _Vx;
        public float _Vy;
        public ulong _LastUpdate;
        public ulong _LocalEpoch;
        public bool _IsValid;
    }
}
