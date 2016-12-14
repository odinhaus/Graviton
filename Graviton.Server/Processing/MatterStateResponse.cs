using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public enum MatterType
    {
        Gold,
        Silicate,
        HydroCarbon,
        Water,
        Gas
    }
    public class MatterStateResponse : ICanSerialize
    {
        _MatterStateResponse m = new _MatterStateResponse();


        public long Id { get { return m._Id; } set { m._Id = value; } }
        public MatterType MatterType { get { return m._Type; } set { m._Type = value; } }
        public float Vx { get { return m._Vx; } set { m._Vx = value; } }
        public float Angle { get { return m._Angle; } set { m._Angle = value; } }
        public float Vy { get { return m._Vy; } set { m._Vy = value; } }
        public float X { get { return m._X; } set { m._X = value; } }
        public float Y { get { return m._Y; } set { m._Y = value; } }
        public float Mass { get { return m._Mass; } set { m._Mass = value; } }
        public ulong FirstUpdate { get { return m._FirstUpdate; } set { m._FirstUpdate = value; } }
        public ulong LastUpdate { get { return m._LastUpdate; } set { m._LastUpdate = value; } }
        public bool IsValid { get { return m._IsValid; } set { m._IsValid = value; } }

        public ushort Type
        {
            get
            {
                return (ushort)ItemTypeId.MatterStateResponse;
            }
        }

        public byte[] Serialize()
        {
            int size = Marshal.SizeOf(typeof(_MatterStateResponse));
            var bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(m, ptr, true);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public void Deserialize(byte[] bytes, int offset = 0)
        {
            var deserialized = new _MatterStateResponse();
            int size = Marshal.SizeOf(typeof(_MatterStateResponse));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, offset, ptr, size);
            deserialized = (_MatterStateResponse)Marshal.PtrToStructure(ptr, typeof(_MatterStateResponse));
            Marshal.FreeHGlobal(ptr);
            m = deserialized;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public class _MatterStateResponse
    {
        public MatterType _Type;
        public long _Id;
        public float _Angle;
        public float _Vx;
        public float _Vy;
        public float _X;
        public float _Y;
        public float _Mass;
        public ulong _FirstUpdate;
        public ulong _LastUpdate;
        public bool _IsValid;
    }
}
