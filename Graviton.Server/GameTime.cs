using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server
{
    public class GameTime
    {
        public GameTime(TimeSpan totalGameTime, TimeSpan epochGameTime, ulong epoch)
        {
            this.TotalGameTime = totalGameTime;
            this.EpochGameTime = epochGameTime;
            this.Epoch = epoch;
        }

        public TimeSpan TotalGameTime { get; private set; }
        public TimeSpan EpochGameTime { get; private set; }
        public ulong Epoch { get; private set; }
    }
}
