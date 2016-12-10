using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server.Tests
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            new ServerTests().CanSendClientState();
        }
    }
}
