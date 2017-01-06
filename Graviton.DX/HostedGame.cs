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
using Graviton.Server;

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
        //private QuadTree<IMovable3> _index;
        private Dictionary<long, Matter> _matter = new Dictionary<long, Matter>();
        //private List<QuadTree<IMovable3>.Quad> _quads = new List<QuadTree<IMovable3>.Quad>();
        private Random _r = new Random();
        private Line _top, _left, _right, _bottom, _center;
        private List<IMovable3> _items = new List<IMovable3>();
        private Arc _leftCrease, _rightCrease;

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

                _player.Value = string.Format("X: {0}  Y: {1}", _disc.Position.X, _disc.Position.Z);
                _viewPort.Value = string.Format("X: {0}  Y: {1}  W: {2}  H: {3}", ViewPort.X, ViewPort.Y, ViewPort.Width, ViewPort.Height);
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

                var matter = new Matter(GraphicsDevice, new Vector3(r.X, -5f, r.Y), new Vector3(r.Vx, RandomFloat(_r, -10f, -0.1f), r.Vy), r.Mass, Matrix.CreateRotationY(r.Angle), texture);
                //matter.Quad = _index.FindFirst(matter.BoundingBox);

                //lock (matter.Quad.Items)
                //{
                //    matter.Quad.Items.Add(matter);
                //}

                matter.DrawTexture = true;
                _matter.Add(r.Id, matter);
                _items.Add(matter);
            }
            else
            {
                var matter = _matter[r.Id];
                matter.Position = new Vector3(r.X, matter.Position.Y, r.Y);
                matter.Velocity = new Vector3(r.Vx, 0, r.Vy);
                //if (!matter.Quad.Bounds.Intersects(matter.BoundingBox))
                //{
                //    lock (matter.Quad.Items)
                //    {
                //        matter.Quad.Items.Remove(matter);
                //        matter.Quad.Items.Add(matter);
                //    }
                //}
                matter.Update(GameTime);
            }
        }

        private GameTime GameTime;
        private void UpdateGameState(GameStateResponse r)
        {
            GameTime = new GameTime(TimeSpan.FromSeconds(r.T), TimeSpan.FromSeconds(r.dT));
            if (_worldSize == 0 && r.WorldSize > 0)
            {
                //_index = new QuadTree<IMovable3>(1, new RectangleF() { X = -r.WorldSize, Y = -r.WorldSize, Width = r.WorldSize * 2f, Height = r.WorldSize * 2f });
                Random random = new Random();
            }

            if (r.WorldSize != _worldSize)
            {
                _worldSize = r.WorldSize;
                _gameCamera = new Camera(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, _worldSize * 3f);
                _gameCamera.ZoomOut(_worldSize * 1.45f);
                _gameCamera.Range.Position.Max = new Vector3(_worldSize, _worldSize * 3f, _worldSize);
                _gameCamera.Range.Position.Min = new Vector3(0, 2, 0);
                var height = _worldSize * 2f / r.AspectRatio;
                var width = _worldSize * 2f;
                _left = new Line(GraphicsDevice, height, 24f, 24, Color.Blue);
                _left.Rotation = Matrix.CreateRotationY((float)Math.PI / 2f);
                _left.Position = new Vector3(-width / 2f, -150f, height / 2f);
                _right = new Line(GraphicsDevice, height, 24f, 24, Color.Blue);
                _right.Rotation = Matrix.CreateRotationY((float)Math.PI / 2f);
                _right.Position = new Vector3(width / 2f, -150f, height / 2f);
                _center = new Line(GraphicsDevice, height, 24f, 24, Color.White);
                _center.Rotation = Matrix.CreateRotationY((float)Math.PI / 2f);
                _center.Position = new Vector3(-12f, -250f, height / 2f);
                _top = new Line(GraphicsDevice, width, 24f, 24, Color.Blue);
                _top.Position = new Vector3(-width / 2f, -150f, -height / 2f);
                _bottom = new Line(GraphicsDevice, width, 24f, 24, Color.Blue);
                _bottom.Position = new Vector3(-width / 2f, -150f, height / 2f - 24f);
                var creaseSize = 400f;
                _leftCrease = new Arc(GraphicsDevice, creaseSize, 64, 24f, FillStyle.Center, 0f, (float)Math.PI * 2f, Color.White);
                _leftCrease.Position = new Vector3(-creaseSize / 2f - (width * 2f / 6f), -150f, -creaseSize / 2f);
                _rightCrease = new Arc(GraphicsDevice, creaseSize, 64, 24f, FillStyle.Center, 0f, (float)Math.PI * 2f, Color.White);
                _rightCrease.Position = new Vector3(-creaseSize / 2f + (width * 2f / 6f), -150f, -creaseSize / 2f);
            }
            this.ViewPort = GetViewRect(_gameCamera);


            this.ServerEpoch = r.Epoch;
        }

        private RectangleF GetViewRect(Camera gameCamera)
        {
            var z = gameCamera.Position.Y;
            var h = (float)(2f * z * Math.Tan(gameCamera.FoV / 2f));
            var w = (float)(h * gameCamera.AR);
            return new RectangleF() { X = gameCamera.Position.X - w / 2f, Y = gameCamera.Position.Z - h / 2f, Width = w, Height = h };
        }

        protected override void LoadContent()
        {
            Label.ShowLabels = true;

            _font = Content.Load<SpriteFont>(@"Font");
            FrameRate fr = new FrameRate(this, _spriteBatch, _font);
            Components.Add(fr);

            _disc = new Disc(this.GraphicsDevice, 1000f, Content.Load<Texture2D>("StarsCartoon"));
            _disc.Position = new Vector3(0f, -_worldSize + 10f, 0f);
            _disc.DrawTexture = true;

            _bg = Content.Load<Texture2D>("bg");
            _gold = Content.Load<Texture2D>("Gold");

            _player = new Label(this, _spriteBatch, _font);
            _player.Caption = "Player:";
            _player.Position = new Vector2(0, 20);

            _viewPort = new Label(this, _spriteBatch, _font);
            _viewPort.Caption = "Viewport:";
            _viewPort.Position = new Vector2(0, 40);

            _mouseInWorld = new Label(this, _spriteBatch, _font);
            _mouseInWorld.Caption = "Mouse:";
            _mouseInWorld.Position = new Vector2(0, 60);

            Components.Add(_player);
            Components.Add(_viewPort);
            Components.Add(_mouseInWorld);

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
        private Label _mouseInWorld;

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
            var mousePtInWorld = new Vector2(
                (((float)mouse.Position.X / (float)GraphicsDevice.Viewport.Width) * this.ViewPort.Width + this.ViewPort.X),
                (((float)mouse.Position.Y / (float)GraphicsDevice.Viewport.Height) * this.ViewPort.Height + this.ViewPort.Y));
            var vector = new Vector2(
                ((mousePtInWorld.X - _disc.Position.X) / (this.ViewPort.Width / 3f)).Clamp(-1f, 1f), 
                ((mousePtInWorld.Y - _disc.Position.Z) / (this.ViewPort.Height / 3f)).Clamp(-1f, 1f));
            
            _mouseInWorld.Value = string.Format("X: {0}, Y: {1}, X': {2}, Y': {3}", 
                mousePtInWorld.X, 
                mousePtInWorld.Y,
                vector.X,
                vector.Y);
            return vector;
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
                //var quads = _index.FindAll(ViewPort).ToArray();
                //foreach (var quad in quads)
                //{
                //    lock (quad.Items)
                //    {
                //        foreach (var item in quad.Items)
                //        {
                //            item.Draw(_gameCamera.View, _gameCamera.Projection);
                //        }
                //    }
                //    if (!_quads.Contains(quad))
                //    {
                //        _quads.Add(quad);
                //    }
                //}

                //foreach(var oldQuad in _quads.ToArray())
                //{
                //    if (!quads.Contains(oldQuad))
                //    {
                //        lock (oldQuad.Items)
                //        {
                //            foreach (var item in oldQuad.Items)
                //            {
                //                item.Unload();
                //            }
                //            _quads.Remove(oldQuad);
                //        }
                //    }
                //}
                foreach (var item in _items)
                    item.Draw(_gameCamera.View, _gameCamera.Projection);

                _disc.Draw(_gameCamera.View, _gameCamera.Projection);
                _left.Draw(_gameCamera.View, _gameCamera.Projection);
                _right.Draw(_gameCamera.View, _gameCamera.Projection);
                _top.Draw(_gameCamera.View, _gameCamera.Projection);
                _bottom.Draw(_gameCamera.View, _gameCamera.Projection);
                _center.Draw(_gameCamera.View, _gameCamera.Projection);
                _leftCrease.Draw(_gameCamera.View, _gameCamera.Projection);
                _rightCrease.Draw(_gameCamera.View, _gameCamera.Projection);
            }
            base.Draw(gameTime);
        }
    }
}
