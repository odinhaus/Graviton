using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Processing
{
    public enum ItemTypeId : uint
    {
        GameStateResponse = 1,
        PlayerStateResponse,
        AuthenticateResponse,
        AuthenticateRequest,
        PlayerStateRequest,
        MatterStateResponse,
        Matter,
        Player
    }
    public static class ItemTypes
    {
        private static GameStateResponse _gsr = new GameStateResponse();
        private static PlayerStateResponse _psr = new PlayerStateResponse();
        private static AuthenticateResponse _ar = new AuthenticateResponse();
        private static MatterStateResponse _msr = new MatterStateResponse();
        private static int _gsrL = Marshal.SizeOf(typeof(_GameStateResponse));
        private static int _psrL = Marshal.SizeOf(typeof(_PlayerStateResponse));
        private static int _arL = Marshal.SizeOf(typeof(_AuthenticateResponse));
        private static int _msrL = Marshal.SizeOf(typeof(_MatterStateResponse));

        public static ICanSerialize GetSerializer(ItemTypeId type)
        {
            switch(type)
            {
                case ItemTypeId.GameStateResponse: return _gsr;
                case ItemTypeId.PlayerStateResponse: return _psr;
                case ItemTypeId.AuthenticateResponse: return _ar;
                case ItemTypeId.MatterStateResponse: return _msr;
                default: return null;
            }
        }

        internal static int GetLength(ItemTypeId type)
        {
            switch (type)
            {
                case ItemTypeId.GameStateResponse: return _gsrL;
                case ItemTypeId.PlayerStateResponse: return _psrL;
                case ItemTypeId.AuthenticateResponse: return _arL;
                case ItemTypeId.MatterStateResponse: return _msrL;
                default: return 0;
            }
        }
    }
}
