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
using Graviton.XNA.Players;
using Graviton.DX.Players;
using Graviton.XNA.Primitives;
using Graviton.Common.Indexing;
using System.Diagnostics;

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
        //private Circle[] _stars = new Circle[0];
        bool _isFirstRequest = true;
        private SpriteFont _font;
        private Disc _disc;
        private Texture2D _bg;
        private Texture2D _gold;
        private QuadTree<IMovable3> _index;
        private Dictionary<long, Matter> _matter = new Dictionary<long, Matter>();
        private List<QuadTree<IMovable3>.Quad> _quads = new List<QuadTree<IMovable3>.Quad>();
        private Random _r = new Random();

        public HostedGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.TargetElapsedTime = TimeSpan.FromTicks((long)10000000 / (long)60);

            var form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowUserResizing = true;
            Window.IsBorderless = true;

            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

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
            if (r.Requestor == Requester)
            {
                _disc.Velocity = new Vector3(r.Vx, 0, r.Vy);
                _disc.Position = new Vector3(r.X, _disc.Position.Y, r.Y);
                _disc.Mass = r.Mass;
                _disc.Update(GameTime);

                if (_disc.Position.X > _gameCamera.Position.X)
                    _gameCamera.PanRight(_disc.Position.X - _gameCamera.Position.X);
                else
                    _gameCamera.PanLeft(_gameCamera.Position.X - _disc.Position.X);

                if (_disc.Position.Z > _gameCamera.Position.Z)
                    _gameCamera.PanDown(_disc.Position.Z - _gameCamera.Position.Z);
                else
                    _gameCamera.PanUp(_gameCamera.Position.Z - _disc.Position.Z);

                _gameCamera.Update(GameTime);

                ViewPort.X = ViewPort.X + (float)(r.Vx * GameTime.ElapsedGameTime.TotalSeconds);
                ViewPort.Y = ViewPort.Y + (float)(r.Vy * GameTime.ElapsedGameTime.TotalSeconds);

                _player.Value = string.Format("X: {0}  Y: {1}", _disc.Position.X.ToString("g3"), _disc.Position.Z.ToString("g3"));
                _viewPort.Value = string.Format("X: {0}  Y: {1}  W: {2}  H: {3}", ViewPort.X.ToString("g3"), ViewPort.Y.ToString("g3"), ViewPort.Width.ToString("g3"), ViewPort.Height.ToString("g3"));
            }
            IsReady = true;
        }

        private void UpdateMatterState(MatterStateResponse r)
        {
            if (!_matter.ContainsKey(r.Id))
            {
                Texture2D texture = null;
                switch (r.MatterType)
                {
                    case MatterType.Gold:
                        {
                            texture = _gold;
                            break;
                        }
                }

                var matter = new Matter(GraphicsDevice, new Vector3(r.X, -2f, r.Y), new Vector3(r.Vx, RandomFloat(_r, -10f, -0.1f), r.Vy), r.Mass, Matrix.CreateRotationY(r.Angle), texture);
                matter.Quad = _index.FindFirst(matter.BoundingBox);

                lock (matter.Quad.Items)
                {
                    matter.Quad.Items.Add(matter);
                }

                matter.DrawTexture = true;
                _matter.Add(r.Id, matter);
            }
            else
            {
                var matter = _matter[r.Id];
                matter.Position = new Vector3(r.X, matter.Position.Y, r.Y);
                matter.Velocity = new Vector3(r.Vx, 0, r.Vy);
                if (!matter.Quad.Bounds.Intersects(matter.BoundingBox))
                {
                    lock (matter.Quad.Items)
                    {
                        matter.Quad.Items.Remove(matter);
                        matter.Quad.Items.Add(matter);
                    }
                }
                matter.Update(GameTime);
            }
        }

        private GameTime GameTime;
        private void UpdateGameState(GameStateResponse r)
        {
            GameTime = new GameTime(TimeSpan.FromSeconds(r.T), TimeSpan.FromSeconds(r.dT));
            if (_worldSize == 0 && r.WorldSize > 0)
            {
                _index = new QuadTree<IMovable3>(6, new RectangleF() { X = -r.WorldSize, Y = -r.WorldSize, Width = r.WorldSize * 2f, Height = r.WorldSize * 2f });
                Random random = new Random();
                //_stars = new Circle[550];
                //for (int i = 0; i < _stars.Length; i++)
                //{
                //    var star = new Circle(GraphicsDevice, 1.5f, 8, Color.White, Color.White);
                //    if (i < 75)
                //        star.Position = new Vector3(RandomFloat(random, -r.WorldSize, r.WorldSize), -r.WorldSize / 4f, RandomFloat(random, -r.WorldSize, r.WorldSize));
                //    else if (i < 175)
                //        star.Position = new Vector3(RandomFloat(random, -r.WorldSize, r.WorldSize), -r.WorldSize / 5.5f, RandomFloat(random, -r.WorldSize, r.WorldSize));
                //    else
                //        star.Position = new Vector3(RandomFloat(random, -r.WorldSize, r.WorldSize), -r.WorldSize / 7f, RandomFloat(random, -r.WorldSize, r.WorldSize));
                //    //star.Position = new Vector3(10, -10, 10);
                //    star.Velocity = Vector3.Zero;
                //    _stars[i] = star;
                //    lock (_index)
                //    {
                //        _index.FindFirst(star.BoundingBox).Items.Add(star);
                //    }
                //}
            }

            if (r.WorldSize != _worldSize)
            {
                _worldSize = r.WorldSize;
                _gameCamera = new Camera(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, _worldSize * 8);
                _gameCamera.ZoomOut(15);
                _gameCamera.Range.Position.Max = new Vector3(_worldSize, _worldSize / 4f, _worldSize);
                _gameCamera.Range.Position.Min = new Vector3(0, 2, 0);
            }

            //var frustrum = new BoundingFrustum(_gameCamera.View * _gameCamera.Projection);
            //var corners = frustrum.GetCorners();
            this.ViewPort = GetViewRect(_gameCamera);

            this.ServerEpoch = r.Epoch;
        }

        private RectangleF GetViewRect(Camera _gameCamera)
        {
            var z = _gameCamera.Position.Y;
            var h = (float)(2f * z * Math.Tan(_gameCamera.FoV));
            var w = (float)(h * _gameCamera.AR);
            return new RectangleF() { X = _gameCamera.Position.X - w / 2f, Y = _gameCamera.Position.Z - h / 2f, Width = w, Height = h };
        }

        protected override void LoadContent()
        {
            Label.ShowLabels = true;

            _font = Content.Load<SpriteFont>(@"Font");
            FrameRate fr = new FrameRate(this, _spriteBatch, _font);
            Components.Add(fr);

            _disc = new Disc(this.GraphicsDevice, 1000f, Content.Load<Texture2D>("StarsCartoon"));
            _disc.Position = Vector3.Zero;
            _disc.DrawTexture = true;

            _bg = Content.Load<Texture2D>("bg");
            _gold = Content.Load<Texture2D>("Gold");

            _player = new Label(this, _spriteBatch, _font);
            _player.Caption = "Player:";
            _player.Position = new Vector2(0, 20);

            _viewPort = new Label(this, _spriteBatch, _font);
            _viewPort.Caption = "Viewport:";
            _viewPort.Position = new Vector2(0, 40);

            Components.Add(_player);
            Components.Add(_viewPort);

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
            _client.MatterStateUpdated += (s, r) =>
            {
                UpdateMatterState(r);
            };
            _client.Connect();

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
        private RectangleF ViewPort;
        private Label _player;
        private Label _viewPort;

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

                    _spriteBatch = new SpriteBatch(GraphicsDevice);

                    _resizing = false;
                }

                UpdateMouseState(gameTime);

                var playerUpdate = new PlayerRequest()
                {
                    LocalEpoch = Epoch,
                    IsFirstRequest = _isFirstRequest,
                    Requester = this.Requester,
                    ViewPort_X = ViewPort.X,
                    ViewPort_Y = ViewPort.Y,
                    ViewPort_W = ViewPort.Width,
                    ViewPort_H = ViewPort.Height,
                    IsValid = true
                };

                var movementVector = GetMovementVector(gameTime);
                playerUpdate.Vector_X = movementVector.X;
                playerUpdate.Vector_Y = movementVector.Y;


                _client.Send(playerUpdate);
            }

            base.Update(gameTime);
        }

        private Vector2 GetMovementVector(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var w2 = GraphicsDevice.Viewport.Width / 2f;
            var h2 = GraphicsDevice.Viewport.Height / 2f;
            var vx = (mouse.Position.X - w2) / w2;
            var vy = (mouse.Position.Y - h2) / h2;
            return new Vector2(vx, vy);
        }

        private void UpdateMouseState(GameTime gameTime)
        {
            MouseState state = Mouse.GetState();
            try
            {
                int wheel = state.ScrollWheelValue - _mouseState.ScrollWheelValue;
                if (wheel != 0)
                {
                    if (wheel > 0)
                    {
                        // Zoom in 
                        _gameCamera.ZoomIn(2f);
                    }
                    else
                    {
                        // Zoom out 
                        _gameCamera.ZoomOut(2f);
                    }
                }
            }
            finally
            {
                _mouseState = state;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.TransparentBlack);
            
            if (IsReady)
            {
                var quads = _index.FindAll(ViewPort).ToArray();
                foreach (var quad in quads)
                {
                    lock (quad.Items)
                    {
                        foreach (var item in quad.Items)
                        {
                            item.Draw(_gameCamera.View, _gameCamera.Projection);
                        }
                    }
                    if (!_quads.Contains(quad))
                    {
                        _quads.Add(quad);
                    }
                }

                foreach(var oldQuad in _quads.ToArray())
                {
                    if (!quads.Contains(oldQuad))
                    {
                        lock (oldQuad.Items)
                        {
                            foreach (var item in oldQuad.Items)
                            {
                                item.Unload();
                            }
                            _quads.Remove(oldQuad);
                        }
                    }
                }

                _disc.Draw(_gameCamera.View, _gameCamera.Projection);
            }
            base.Draw(gameTime);
        }

        //private RectangleF GetViewRect(Vector3[] corners, float planeDepth)
        //{
        //    if (float.IsNaN(corners[4].X))
        //    {
        //        corners[4] = new Vector3(_worldSize, _worldSize, _worldSize);
        //        corners[5] = corners[4];
        //        corners[6] = corners[4];
        //        corners[7] = corners[4];
        //    }

        //    var ratio = (corners[0].Y - planeDepth) / (corners[0].Y - corners[4].Y);

        //    var tlX = ratio * (corners[4].X - corners[0].X) + corners[0].X;
        //    var tlZ = ratio * (corners[5].Z - corners[1].Z) + corners[1].Z;
        //    var w = ratio * (corners[5].X - corners[1].X) + corners[1].X - tlX;
        //    var h = ratio * (corners[7].Z - corners[3].Z) + corners[3].Z - tlZ;

        //    return new RectangleF()
        //    {
        //        X = tlX, 
        //        Y = tlZ, 
        //        Width = w,
        //        Height = h,
        //    };
        //}
    }
}
