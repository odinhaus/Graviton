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
        public static ICanSerialize GetSerializer(ItemTypeId type)
        {
            switch(type)
            {
                case ItemTypeId.GameStateResponse: return new GameStateResponse();
                case ItemTypeId.PlayerStateResponse: return new PlayerStateResponse();
                case ItemTypeId.AuthenticateResponse: return new AuthenticateResponse();
                default: return null;
            }
        }

        internal static int GetLength(ItemTypeId type)
        {
            switch (type)
            {
                case ItemTypeId.GameStateResponse: return Marshal.SizeOf(typeof(_GameStateResponse));
                case ItemTypeId.PlayerStateResponse: return Marshal.SizeOf(typeof(_PlayerStateResponse));
                case ItemTypeId.AuthenticateResponse: return Marshal.SizeOf(typeof(_AuthenticateResponse));
                default: return 0;
            }
        }
    }
}
