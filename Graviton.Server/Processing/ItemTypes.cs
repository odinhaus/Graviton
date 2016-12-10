using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public static class ItemTypes
    {
        public static uint GetType(ICanSerialize item)
        {
            if (item is GameStateResponse)
            {
                return 1;
            }
            else if (item is PlayerStateResponse)
            {
                return 2;
            }
            else if (item is AuthenticateResponse)
            {
                return 3;
            }
            return 0;
        }

        public static ICanSerialize GetSerializer(uint type)
        {
            switch(type)
            {
                case 1: return new GameStateResponse();
                case 2: return new PlayerStateResponse();
                case 3: return new AuthenticateResponse();
                default: return null;
            }
        }

        internal static int GetLength(uint type)
        {
            switch (type)
            {
                case 1: return Marshal.SizeOf(typeof(GameStateResponse));
                case 2: return Marshal.SizeOf(typeof(PlayerStateResponse));
                case 3: return Marshal.SizeOf(typeof(AuthenticateResponse));
                default: return 0;
            }
        }
    }
}
