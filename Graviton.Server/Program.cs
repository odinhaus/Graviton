using Graviton.Server.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpHost.Start();
            Console.Read();
            TcpHost.Stop();
        }
    }
}
