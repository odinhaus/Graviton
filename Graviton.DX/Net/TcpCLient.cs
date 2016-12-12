using Graviton.Server;
using Graviton.Server.Net;
using Graviton.Server.Processing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Graviton.DX.Net
{
    public delegate void ConnectedHandler(object sender, EventArgs e);
    public delegate void AuthenticatedHandler(object sender, AuthenticateResponse response);
    public delegate void GameStateHandler(object sender, GameStateResponse response);
    public delegate void PlayerStateHandler(object sender, PlayerStateResponse response);

    public class TcpClient : IDisposable
    {
        public event AuthenticatedHandler Authenticated;
        public event GameStateHandler GameStateUpdated;
        public event PlayerStateHandler PlayerStateUpdated;
        public event ConnectedHandler Connected;

        Socket _socket;
        Thread _connectThread;

        public TcpClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect()
        {
            if (!_socket.Connected)
            {
                var ipAddress = IPAddress.Parse(ConfigurationManager.AppSettings["hostIp"]);
                var port = int.Parse(ConfigurationManager.AppSettings["hostPort"]);
                var ipEndPoint = new IPEndPoint(ipAddress, port);
                _connectThread = new Thread(new ParameterizedThreadStart(Connect));
                _connectThread.IsBackground = true;
                _connectThread.Name = "Connect Thread";
                _connectThread.Start(ipEndPoint);
            }
        }

        private void Connect(object ipEndPoint)
        {
            var connected = false;
            while(!connected)
            {
                try
                {
                    _socket.Bind(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 0));
                    _socket.Connect((IPEndPoint)ipEndPoint);

                    var state = new SocketState()
                    {
                        Socket = _socket
                    };

                    _socket.BeginReceive(state.Buffer, 0, SocketState.BUFFER_SIZE, SocketFlags.None, new AsyncCallback(Receive), state);

                    _socket.Send(new AuthenticateRequest()
                    {
                        Username = "foo",
                        Password = "fum",
                        IsValid = true
                    }.Serialize());
                    connected = true;

                    if (Connected != null)
                        Connected(this, new EventArgs());
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public void Send(PlayerRequest update)
        {
            var bytes = update.Serialize();
            _socket.Send(bytes);
        }


        private void Receive(IAsyncResult ar)
        {
            var state = ar.AsyncState as SocketState;
            var client = state.Socket;
            var read = client.EndReceive(ar);

            if (read > 0)
            {
                ReadData(read, state);
                client.BeginReceive(state.Buffer, state.Offset, SocketState.BUFFER_SIZE - state.Offset, SocketFlags.None, new AsyncCallback(Receive), state);
            }
            else
            {
                client.Dispose();
            }
        }

        private void ReadData(int read, SocketState state)
        {
            state.Offset += read;

            var idx = 0;
            while(idx < state.Offset)
            {
                if (state.Offset - idx >= 2)
                {
                    var type = state.Buffer.ToUInt16(idx);
                    
                    var length = ItemTypes.GetLength((ItemTypeId)type);
                    if (state.Offset - idx >= length)
                    {
                        var item = ItemTypes.GetSerializer((ItemTypeId)type);
                        item.Deserialize(state.Buffer, idx + 2);
                        idx += 2 + length; // only update the index if we read the whole thing
                        ProcessMessage(item);
                    }
                    else break;
                }
                else break;
            }

            if (idx > 0)
            {
                if (idx != state.Offset)
                {
                    // copy residual buffer back to the state buffer
                    state.Buffer.Copy(idx, state.Swap, (uint)state.Offset - (uint)idx);
                    state.Swap.Copy(0, state.Buffer, (uint)state.Offset - (uint)idx);
                }
                state.Offset = state.Offset - idx;
            }
        }


        private void ProcessMessage(ICanSerialize item)
        {
            if (item is AuthenticateResponse)
            {
                Authenticated?.Invoke(this, (AuthenticateResponse)item);
            }
            else if (item is GameStateResponse)
            {
                GameStateUpdated?.Invoke(this, (GameStateResponse)item);
            }
            else if (item is PlayerStateResponse)
            {
                PlayerStateUpdated?.Invoke(this, (PlayerStateResponse)item);
            }
        }

        public void Dispose()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket.Dispose();
                _socket = null;
            }
        }
    }
}
