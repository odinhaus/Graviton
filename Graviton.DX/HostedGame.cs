using Graviton.DX.Net;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graviton.Server.Processing;

namespace Graviton.DX
{
    public class HostedGame : Game
    {
        TcpClient _client;
        GraphicsDeviceManager _graphics;

        public HostedGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _client = new TcpClient();
            _client.Connected += (s, e) =>
            {
                this.IsConnected = true;
            };
            _client.Authenticated += (s, r) =>
            {
                this.Requester = r.Requester;
                this.IsAuthenticated = r.IsAuthenticated;
            };
            _client.GameStateUpdated += (s, r) =>
            {
                UpdateGameState(r);
            };
            _client.PlayerStateUpdated += (s, r) =>
            {
                UpdatePlayerState(r);
            };

            this.TargetElapsedTime = TimeSpan.FromTicks((long)10000000 / (long)120);


            Window.AllowUserResizing = true;
            Window.IsBorderless = true;

            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

            _client.Connect();

            base.Initialize();
        }


        private void UpdatePlayerState(PlayerStateResponse r)
        {
            
        }

        private void UpdateGameState(GameStateResponse r)
        {
            
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public bool IsAuthenticated { get; private set; }
        public ulong Requester { get; private set; }
        public GameStateResponse GameState { get; private set; }
        public PlayerStateResponse PlayerState { get; private set; }
        public bool IsConnected { get; private set; }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsAuthenticated)
            {

            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (IsAuthenticated)
            {

            }
            base.Draw(gameTime);
        }
    }
}
