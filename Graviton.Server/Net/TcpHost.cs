using Graviton.Server.Processing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Graviton.Server.Net
{
    public class TcpHost
    {
        static bool _running = true;
        static List<SocketState> _clients = new List<SocketState>();
        static Thread _listener;

        public static void Start()
        {
            var ipAddress = IPAddress.Parse(ConfigurationManager.AppSettings["hostIp"]);
            var port = int.Parse(ConfigurationManager.AppSettings["hostPort"]);
            var ipEndPoint = new IPEndPoint(ipAddress, port);

            _listener = new Thread(new ParameterizedThreadStart(AcceptConnections));
            _listener.IsBackground = true;
            _listener.Name = "Host Listener";
            _listener.Start(ipEndPoint);
       
        }

        static ManualResetEventSlim _ready = new ManualResetEventSlim(false);
        private static void AcceptConnections(object obj)
        {
            // Create a TCP/IP socket.
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind((IPEndPoint)obj);
                listener.Listen(500);

                while(_running)
                {
                    _ready.Reset();

                    listener.BeginAccept(new AsyncCallback(AcceptConnection), listener);

                    _ready.Wait();
                }
            }
            catch
            {

            }
        }

        private static void AcceptConnection(IAsyncResult ar)
        {
            _ready.Set();

            var listener = ar.AsyncState as Socket;
            var client = listener.EndAccept(ar);

            var state = new SocketState()
            {
                Socket = client
            };
            _clients.Add(state);
            client.BeginReceive(state.Buffer, 0, SocketState.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(Receive), state);
        }

        private static void Receive(IAsyncResult ar)
        {
            var state = ar.AsyncState as SocketState;
            var client = state.Socket;
            try
            {
                var read = client.EndReceive(ar);

                if (read > 0)
                {
                    ReadData(read, state);
                    client.BeginReceive(state.Buffer, state.Offset, SocketState.BUFFER_SIZE - state.Offset, SocketFlags.None, new AsyncCallback(Receive), state);
                }
                else
                {
                    client.Dispose();
                    _clients.Remove(state);
                }
            }
            catch
            {
                client.Dispose();
                _clients.Remove(state);
            }
        }

        private static void ReadData(int read, SocketState state)
        {
            int offset = 0;
            state.Offset += read;
            if (state.Requester == 0)
            {
                ulong requester;
                var response = new AuthenticateResponse();
                if (UserInputs.Authenticate(state.Buffer, out requester))
                {
                    response.IsAuthenticated = true;
                    response.Requester = requester;
                    state.Requester = requester;
                }
                else
                {
                    response.IsAuthenticated = false;
                }
                response.IsValid = true;
                state.Socket.Send(ItemTypes.GetType(response).GetBytes());
                state.Socket.Send(response.Serialize());
            }
            else if (UserInputs.Process(state, out offset))
            {
                var buffer = new byte[SocketState.BUFFER_SIZE];
                if (offset > 0)
                {
                    state.Buffer.Copy(SocketState.BUFFER_SIZE - offset, buffer, (uint)offset);
                }
                state.Buffer = buffer;
                state.Offset = offset;
            }
        }

        public static void Stop()
        {
            _running = false;
        }
    }
}
