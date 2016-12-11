using Graviton.DX.Net;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graviton.Server.Processing;
using Graviton.XNA.Diagnostics;
using Graviton.XNA.Cameras;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Graviton.XNA;
using Graviton.XNA.Shapes.TwoD;
using Graviton.Common.Drawing;

namespace Graviton.DX
{
    public class HostedGame : Game
    {
        TcpClient _client;
        GraphicsDeviceManager _graphics;
        Label _user;
        Camera _gameCamera = null;
        float _worldSize = 0f;
        private SpriteBatch _spriteBatch;
        private KeyboardState _keyboardState;
        private MouseState _mouseState;
        private PresentationParameters _pp;
        private int _screenH;
        private int _screenW;
        private Bloom _bloom;
        private RenderTarget2D _renderTarget1;
        private RenderTarget2D _renderTarget2;
        private bool _resizing;
        private Circle[] _stars = new Circle[0];

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

            var form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowUserResizing = true;
            Window.IsBorderless = true;

            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

            _client.Connect();

            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pp = GraphicsDevice.PresentationParameters;
            _screenH = _pp.BackBufferHeight;
            _screenW = _pp.BackBufferWidth;

            _bloom = new Bloom(GraphicsDevice, _spriteBatch);
            _renderTarget1 = new RenderTarget2D(GraphicsDevice, _screenW, _screenH, false, _pp.BackBufferFormat, _pp.DepthStencilFormat, _pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            _renderTarget2 = new RenderTarget2D(GraphicsDevice, _screenW, _screenH, false, _pp.BackBufferFormat, _pp.DepthStencilFormat, _pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            _bloom.Settings = BloomSettings.PresetSettings[1];

            base.Initialize();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _resizing = true;
        }


        private void UpdatePlayerState(PlayerStateResponse r)
        {
            
        }

        private void UpdateGameState(GameStateResponse r)
        {
            if (_worldSize == 0 && r.WorldSize > 0)
            {
                Random random = new Random();
                _stars = new Circle[250];
                for (int i = 0; i < _stars.Length; i++)
                {
                    
                    var star = new Circle(GraphicsDevice, 2f, 4, Color.White, Color.White);
                    if (i < 75)
                        star.Position = new Vector3(RandomFloat(random, -r.WorldSize, r.WorldSize), -r.WorldSize / 5f, RandomFloat(random, -r.WorldSize, r.WorldSize));
                    else if (i < 175)
                        star.Position = new Vector3(RandomFloat(random, -r.WorldSize, r.WorldSize), -r.WorldSize / 8f, RandomFloat(random, -r.WorldSize, r.WorldSize));
                    else
                        star.Position = new Vector3(RandomFloat(random, -r.WorldSize, r.WorldSize), -r.WorldSize / 11f, RandomFloat(random, -r.WorldSize, r.WorldSize));
                    //star.Position = new Vector3(0, -2000, 0);
                    star.Velocity = Vector3.Zero;
                    _stars[i] = star;
                }
            }

            if (r.WorldSize != _worldSize)
            {
                _worldSize = r.WorldSize;
                _gameCamera = new Camera(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, _worldSize * 8);
                _gameCamera.ZoomOut(10);
                _gameCamera.Range.Position.Max = new Vector3(_worldSize, _worldSize * 2f, _worldSize);
                _gameCamera.Range.Position.Min = new Vector3(0, 10, 0);
                IsReady = true;
            }

            this.ServerEpoch = r.Epoch;
        }

        protected override void LoadContent()
        {
            

            base.LoadContent();
        }

        private static float RandomFloat(Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        public bool IsAuthenticated;
        public ulong Requester;
        public GameStateResponse GameState;
        public PlayerStateResponse PlayerState;
        public bool IsConnected;
        public ulong ServerEpoch;
        public ulong Epoch;
        private bool IsReady;

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            Epoch++;

            if (IsReady)
            {
                if (_resizing)
                {
                    _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                    _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                    _graphics.ApplyChanges();
                    _gameCamera.SetAspectRatio(GraphicsDevice.Viewport.AspectRatio);

                    _screenH = Window.ClientBounds.Height;
                    _screenW = Window.ClientBounds.Width;

                    _renderTarget1.Dispose();
                    _renderTarget2.Dispose();

                    _renderTarget1 = new RenderTarget2D(GraphicsDevice, _screenW, _screenH, false, _pp.BackBufferFormat, _pp.DepthStencilFormat, _pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
                    _renderTarget2 = new RenderTarget2D(GraphicsDevice, _screenW, _screenH, false, _pp.BackBufferFormat, _pp.DepthStencilFormat, _pp.MultiSampleCount, RenderTargetUsage.DiscardContents);


                    _resizing = false;
                }



                _gameCamera.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.TransparentBlack);
            if (IsReady)
            {
                var frustrum = new BoundingFrustum(_gameCamera.View * _gameCamera.Projection);
                var corners = frustrum.GetCorners();
                var viewRect = GetViewRect(corners, frustrum.Near.D);


                for (int i = 0; i < _stars.Length; i++)
                {
                    if (viewRect.Intersects(_stars[i].BoundingBox))
                        _stars[i].Draw(_gameCamera.View, _gameCamera.Projection);
                }
            }
            base.Draw(gameTime);
        }

        private RectangleF GetViewRect(Vector3[] corners, float planeDepth)
        {
            if (float.IsNaN(corners[4].X))
            {
                corners[4] = new Vector3(_worldSize, _worldSize, _worldSize);
                corners[5] = corners[4];
                corners[6] = corners[4];
                corners[7] = corners[4];
            }

            var ratio = (corners[0].Y - planeDepth) / (corners[0].Y - corners[4].Y);

            var tl = new Vector3(ratio * (corners[4].X - corners[0].X) + corners[0].X, 0f, ratio * (corners[4].Z - corners[0].Z) + corners[0].Z);
            var tr = new Vector3(ratio * (corners[5].X - corners[1].X) + corners[1].X, 0f, ratio * (corners[5].Z - corners[1].Z) + corners[1].Z);
            var br = new Vector3(ratio * (corners[6].X - corners[2].X) + corners[2].X, 0f, ratio * (corners[6].Z - corners[2].Z) + corners[2].Z);
            var bl = new Vector3(ratio * (corners[7].X - corners[3].X) + corners[3].X, 0f, ratio * (corners[7].Z - corners[3].Z) + corners[3].Z);
            return new RectangleF()
            {
                X = tl.X,
                Y = tl.Z,
                Width = tr.X - tl.X,
                Height = bl.Z - tl.Z
            };
        }
    }
}
