using Graviton.XNA;
using System;

namespace Graviton.DX
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //using (var game = new Game1()) game.Run();

            using (var game = new HostedGame()) game.Run();
        }
    }
#endif
}
