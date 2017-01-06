using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Common
{
    public class Connection
    {
        public const float MaxWeight = 75f;
        public Neuron Source;
        public Neuron Target;
        public float Sign = 1f;
        public float Weight = 1f;
        public DateTime LastSignaled;
        public bool IsSignaled = false;
    }
}
