using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Graviton.Server.Processing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Graviton.Server.Tests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        public unsafe void CanSerializeRequests()
        {
            var size = Marshal.SizeOf<PlayerRequest>();
            var bytes = new byte[size];
            var request = new PlayerRequest()
            {
                _KeyStateMask = 1536,
                _Requester = 12,
                _Vector_X = 23,
                _Vector_Y = 43,
                _Viewport_H = 1234,
                _Viewport_W = 1232,
                _Viewport_X = 300,
                _Viewport_Y = 321
            };

            //fixed(byte* serialized = bytes)
            //{
            //    var ptr = serialized;
            //    ptr = (byte*)&request;
            //}

            var sw = new Stopwatch();
            var count = 1000000;
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                var s = request.Serialize();
            }
            sw.Stop();
            var sTime = sw.Elapsed.TotalSeconds;

            sw.Reset();
            bytes = request.Serialize();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                var d = PlayerRequest.Deserialize(bytes);
            }
            sw.Stop();
            var dTime = sw.Elapsed.TotalSeconds;

            Assert.Inconclusive(string.Format("Serialize: {0}, Deserialize: {1}", ((double)count / sTime).ToString("G3"), ((double)count / dTime).ToString("G3")));

        }

        [TestMethod]
        public void CanSerializePlayer()
        {
            var player = new Player()
            {
                Bounds = new Drawing.RectangleF() { X = 10f, Y = 20f, Width = 200f, Height = 300f },
                IsValid = true,
                Mass = 10,
                Requester = 12,
                Vx = 3f,
                Vy = 2f,
                X = 128,
                Y = 110
            };

            var bytes = player.Serialize();
            var player2 = Player.Deserialize(bytes);
            Assert.IsTrue(player2.Bounds.X == player.Bounds.X);
            Assert.IsTrue(player2.Bounds.Y == player.Bounds.Y);
            Assert.IsTrue(player2.Bounds.Width == player.Bounds.Width);
            Assert.IsTrue(player2.Bounds.Height == player.Bounds.Height);
            Assert.IsTrue(player2.Vx == player.Vx);
            Assert.IsTrue(player2.Vy == player.Vy);
        }

        public void CanSendClientState()
        {
            Thread.Sleep(3000);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 9000));
            var request = new PlayerRequest()
            {
                _KeyStateMask = 1536,
                _Requester = 12,
                _Vector_X = 23,
                _Vector_Y = 43,
                _Viewport_H = 1234,
                _Viewport_W = 1232,
                _Viewport_X = 300,
                _Viewport_Y = 321,
                _IsValid = true
            };
            var count = 1000000;
            for (int i = 0; i < count; i++)
            {
                socket.Send(request.Serialize());
            }
    
            socket.Close();
            socket.Dispose();
        }
    }
}
