using System;
using System.Collections.Generic;
using System.Linq;
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
            else if (item is PlayerResponse)
            {
                return 2;
            }
            return 0;
        }

        public static ICanSerialize GetSerializer(uint type)
        {
            switch(type)
            {
                case 1: return new GameStateResponse();
                case 2: return new PlayerResponse();
                default: return null;
            }
        }
    }
}
