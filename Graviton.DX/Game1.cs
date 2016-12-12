using Graviton.Common.Drawing;
using Graviton.Common.Indexing;
using Graviton.DX.Players;
using Graviton.XNA.Cameras;
using Graviton.XNA.Cursors;
using Graviton.XNA.Diagnostics;
using Graviton.XNA.Players;
using Graviton.XNA.Primitives;
using Graviton.XNA.Shapes.ThreeD;
using Graviton.XNA.Shapes.TwoD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graviton.XNA
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        SpriteFont _font;
        //Genesoid[] _players;
        List<Matter> _matter = new List<Matter>();
        List<Circle> _quadMarkers = new List<Circle>();

        Circle[] _stars;
        Disc _disc;
        Cross _indicator;
        Camera _gameCamera;
        KeyboardState _keyboardState;
        MouseState _mouseState;
        Label _label;
        Label _label2;
        Label _label3;
        Label _label4;
        Label _label5;
        Label _label6;
        Label _label7;
        Label _label8;
        Label _label9;
        Cursor _cursor;
        Texture2D _splitter2D;
        Texture2D _gold;
        Texture2D _background;
        Sphere _sun;
        Sphere _earth;
        QuadTree<IMovable3> _index;

        Bloom _bloom;
        RenderTarget2D _renderTarget1, _renderTarget2;
        static public int _screenW, _screenH;
        float _bloomSatPulse = 0.001f, _bloomSatDir = 0.0002f;
        PresentationParameters _pp;
        float _zoom = 2f;


        List<Splitter> _splitters = new List<Splitter>();

        const float worldSize = 10000f;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.TargetElapsedTime = TimeSpan.FromTicks((long)10000000 / (long)120);


            Window.AllowUserResizing = true;
            Window.IsBorderless = true;
            var form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            _index = new QuadTree<IMovable3>(7, new RectangleF() {X = -worldSize, Y = -worldSize, Width = worldSize * 2f, Height = worldSize * 2f });


            _graphics.SynchronizeWithVerticalRetrace = true;
            //this.IsFixedTimeStep = true;
            _graphics.ApplyChanges();
            _gameCamera = new Camera(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, worldSize * 8);
            _gameCamera.ZoomOut(100);
            _gameCamera.Range.Position.Max = new Vector3(worldSize, worldSize * 2f, worldSize);
            _gameCamera.Range.Position.Min = new Vector3(0, 10, 0);
            //_gameCamera.View = Matrix.CreateLookAt(_gameCamera.Position, Vector3.Zero, Vector3.Up);

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

        bool _resizing = false;
        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _resizing = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            BasicEffect be = new BasicEffect(GraphicsDevice);
            be.VertexColorEnabled = true;

            Random random = new Random();
            // Create a new SpriteBatch, which can be used to draw textures.

            //_players = new Genesoid[10];
            //for (int p = 0; p < _players.Length; p++)
            //{
            //    Genesoid player = new Genesoid(this.GraphicsDevice, 1.5f, Color.White);
            //    player.Position = new Vector3(RandomFloat(random, -10f, 10f), 0f, RandomFloat(random, -10f, 10f));
            //    player.Velocity = new Vector3(
            //        RandomFloat(random, -20f, 20f),
            //        0f,
            //        RandomFloat(random, -20f, 20f));
            //    _players[p] = player;
            //}
            _background = Content.Load<Texture2D>("Background");
            _gold = Content.Load<Texture2D>("Gold");
            Matter.Initialize(GraphicsDevice, _gold);

            for (int p = 0; p < 10000; p++)
            {
                var gold = new Matter(
                    new Vector3(RandomFloat(random, -worldSize, worldSize), 0f, RandomFloat(random, -worldSize, worldSize)),
                    new Vector3(RandomFloat(random, -10f, 10f), 0f, RandomFloat(random, -10f, 10f)),
                    RandomFloat(random, 1f, 1000f),
                    Matrix.CreateRotationY(RandomFloat(random, 0f, (float)Math.PI * 2f)));//RandomFloat(random, 1f, 100f));
                _matter.Add(gold);
                gold.Quad = _index.FindFirst(gold.BoundingBox);
                gold.Quad.Items.Add(gold);
            }

            _stars = new Circle[250];
            for(int i = 0; i < _stars.Length; i++)
            {
                var mod = (float)((i % 5) + 1);
                var star = new Circle(GraphicsDevice, 6f, 4, Color.White, Color.White);
                star.Position = new Vector3(RandomFloat(random, -worldSize * 2f, worldSize * 2f), -worldSize * mod / 20f, RandomFloat(random, -worldSize * 2f, worldSize * 2f));
                star.Velocity = Vector3.Zero;
                _stars[i] = star;
            }

            //BuildQuadMarkers();

            //_indicator = new Disc(GraphicsDevice, 1f, Content.Load<Texture2D>("Indicator"));
            _indicator = new Cross(GraphicsDevice, 1.6f, 0.2f, 6, Color.White);
            _sun = new Sphere(GraphicsDevice, 64f, 32, new Color(255, 255, 128, 255));
            _sun.Position = Vector3.Zero;
            _sun.Velocity = Vector3.Zero;

            _earth = new Sphere(GraphicsDevice, 18f, 32, new Color(200, 200, 255, 255));
            _earth.Position = new Vector3(120f, 0f, 165f);
            _earth.Velocity = Vector3.Zero;

            _splitter2D = Content.Load<Texture2D>("Splitter");
            
            _disc = new Disc(this.GraphicsDevice, 500f, Content.Load<Texture2D>("StarsCartoon"));
            _disc.Position = Vector3.Zero;


            _font = Content.Load<SpriteFont>(@"Font");
            FrameRate fr = new FrameRate(this, _spriteBatch, _font);
            Components.Add(fr);

            _label = new Label(this, _spriteBatch, _font);
            _label.Position = new Vector2(0, 20);
            Components.Add(_label);

            _label2 = new Label(this, _spriteBatch, _font);
            _label2.Position = new Vector2(0, 40);
            Components.Add(_label2);
            _label3 = new Label(this, _spriteBatch, _font);
            _label3.Position = new Vector2(0, 60);
            Components.Add(_label3);
            _label4 = new Label(this, _spriteBatch, _font);
            _label4.Position = new Vector2(0, 80);
            Components.Add(_label4);
            _label5 = new Label(this, _spriteBatch, _font);
            _label5.Position = new Vector2(0, 100);
            Components.Add(_label5);
            _label6 = new Label(this, _spriteBatch, _font);
            _label6.Position = new Vector2(0, 120);
            Components.Add(_label6);
            _label7 = new Label(this, _spriteBatch, _font);
            _label7.Position = new Vector2(0, 140);
            Components.Add(_label7);
            _label8 = new Label(this, _spriteBatch, _font);
            _label8.Position = new Vector2(0, 160);
            Components.Add(_label8);
            _label9 = new Label(this, _spriteBatch, _font);
            _label9.Position = new Vector2(0, 180);
            Components.Add(_label9);

            _cursor = new Cursor(this, _gameCamera);

            _label.Caption = "Camera";
            _label.Value = _gameCamera.Position.ToString();
            _label2.Caption = "Up";
            _label2.Value = _gameCamera.Up.ToString();
            _label3.Caption = "Theta";
            _label3.Value = _gameCamera.Theta.ToString();
            _label4.Caption = "Target";
            _label4.Value = _gameCamera.Target.ToString();
            _label5.Caption = "Mouse";
            _label5.Value = _cursor.Position.ToString();
            _label6.Caption = "Mass";
            _label6.Value = _disc.Mass.ToString();
            _label7.Caption = "Player";
            _label6.Value = _disc.Position.ToString();
            _label8.Caption = "Frustrum";
            _label9.Caption = "Quads";

            _bloom.LoadContent(Content, _pp);
        }

        private void BuildQuadMarkers()
        {
            _index.Traverse((quad) =>
            {
                var tl = new Circle(GraphicsDevice, 1f, 12, Color.Red, Color.Red)
                {
                    Position = new Vector3(quad.Bounds.X, 0f, quad.Bounds.Y),
                    Velocity = Vector3.Zero
                };
                var bl = new Circle(GraphicsDevice, 1f, 12, Color.Red, Color.Red)
                {
                    Position = new Vector3(quad.Bounds.X, 0f, quad.Bounds.Y + quad.Bounds.Height),
                    Velocity = Vector3.Zero
                };
                var tr = new Circle(GraphicsDevice, 1f, 12, Color.Red, Color.Red)
                {
                    Position = new Vector3(quad.Bounds.X + quad.Bounds.Width, 0f, quad.Bounds.Y),
                    Velocity = Vector3.Zero
                };
                var br = new Circle(GraphicsDevice, 1f, 12, Color.Red, Color.Red)
                {
                    Position = new Vector3(quad.Bounds.X + quad.Bounds.Width, 0f, quad.Bounds.Y + quad.Bounds.Height),
                    Velocity = Vector3.Zero
                };
                _quadMarkers.Add(tl);
                _quadMarkers.Add(bl);
                _quadMarkers.Add(tr);
                _quadMarkers.Add(br);
                quad.Items.Add(tl);
                quad.Items.Add(tr);
                quad.Items.Add(bl);
                quad.Items.Add(br);
            },
            (quad) => quad.IsLeaf);
        }

        private static float RandomFloat(Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            _bloom.UnloadContent();
            _renderTarget1.Dispose();
            _renderTarget2.Dispose();
        }

        ulong _count = 0;
        List<Matter> _visibleMatter;
        double _aveUpdateTime;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _count++;
            _aveUpdateTime = ((double)(_count - 1) * _aveUpdateTime + gameTime.ElapsedGameTime.TotalMilliseconds) / (double)_count;
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

            MovePlayer(_disc, gameTime);

            //foreach (Genesoid player in _players)
            //{
            //    BounceSphereInWorld(player, gameTime);
            //    player.Update(gameTime);
            //}
            var frustrum = new BoundingFrustum(_gameCamera.View * _gameCamera.Projection);
            var corners = frustrum.GetCorners();
            var viewRect = GetViewRect(corners, 0f);
            if (_disc.Radius * 2f / viewRect.Width < 0.02)
            {
                _disc.DrawTexture = false;
            }
            else
            {
                _disc.DrawTexture = true;
            }
            //var width = -1f * frustrum.Right.D - frustrum.Left.D;
            //var height = -1f * frustrum.Bottom.D - frustrum.Top.D;
            //var viewRect = new RectangleF() { X = frustrum.Left.D, Y = frustrum.Top.D, Width = width, Height = height };
            _label8.Value = viewRect.ToString();
            var quads = _index.FindAll(viewRect).ToArray();
            _label9.Value = string.Format("Count: {0}, Boundaries: {1}", quads.Length, GetQuadsBoundariesText(quads));
            _visibleMatter = quads.SelectMany(q => q.Items).OfType<Matter>().ToList();
            foreach (var mass in _visibleMatter)
            {
                ApplyGravity(mass, gameTime);
                mass.Update(gameTime);
                BounceSphereInWorld(mass, gameTime);

                if (!mass.Quad.Bounds.Intersects(mass.BoundingBox))
                {
                    mass.Quad.Items.Remove(mass);
                    mass.Quad = _index.FindFirst(mass.Position.X, mass.Position.Z);
                    mass.Quad.Items.Add(mass);
                }
            }

            foreach(var marker in _quadMarkers)
            {
                marker.Dispose();
            }
            _quadMarkers.Clear();
            if (Label.ShowLabels)
            {
                foreach(var quad in quads)
                {
                    _quadMarkers.Add(new Circle(GraphicsDevice, 1f, 8, Color.Red, Color.Red)
                    {
                        Position = new Vector3(quad.Bounds.X, 1f, quad.Bounds.Y),
                        Velocity = Vector3.Zero
                    });
                    _quadMarkers.Add(new Circle(GraphicsDevice, 1f, 8, Color.Red, Color.Red)
                    {
                        Position = new Vector3(quad.Bounds.X + quad.Bounds.Width, 1f, quad.Bounds.Y),
                        Velocity = Vector3.Zero
                    });
                    _quadMarkers.Add(new Circle(GraphicsDevice, 1f, 8, Color.Red, Color.Red)
                    {
                        Position = new Vector3(quad.Bounds.X + quad.Bounds.Width, 1f, quad.Bounds.Y + quad.Bounds.Height),
                        Velocity = Vector3.Zero
                    });
                    _quadMarkers.Add(new Circle(GraphicsDevice, 1f, 8, Color.Red, Color.Red)
                    {
                        Position = new Vector3(quad.Bounds.X, 1f, quad.Bounds.Y + quad.Bounds.Height),
                        Velocity = Vector3.Zero
                    });
                }
            }
            

            if (_count % 30 == 0)
            {
                var sample = _matter.Count / 6;
                var longTime = new GameTime(gameTime.TotalGameTime, new TimeSpan(gameTime.ElapsedGameTime.Ticks * 30));
                foreach (var mass in _matter.Skip((int)(_count % 6) * sample).Take(sample))
                {
                    if (quads.Contains(mass.Quad)) continue;

                    ApplyGravity(mass, longTime);
                    mass.Update(longTime);
                    BounceSphereInWorld(mass, longTime);

                    if (!mass.Quad.Bounds.Intersects(mass.BoundingBox))
                    {
                        mass.Quad.Items.Remove(mass);
                        mass.Quad = _index.FindFirst(mass.Position.X, mass.Position.Z);
                        mass.Quad.Items.Add(mass);
                    }
                }
            }
            

            MoveCamera(_gameCamera, _disc, gameTime);

            UpdateKeyboardState(gameTime);
            UpdateMouseState(gameTime);

            var count = _splitters.Count;
            for(int i = 0; i < count; i++)
            {
                var splitter = _splitters[i];
                splitter.Update(gameTime);
                if (!splitter.IsActive)
                {
                    _splitters.RemoveAt(i);
                    i--;
                    count--;
                }
            }

            _sun.Update(gameTime);
            _earth.Update(gameTime);

            _gameCamera.Update(gameTime);
            _cursor.Update(gameTime);

            _bloomSatPulse += _bloomSatDir;
            if (_bloomSatPulse > 2.5f) _bloomSatDir = -0.009f;
            if (_bloomSatPulse < 0.1f) _bloomSatDir = 0.009f;
            _bloom.Settings.BloomSaturation = _bloomSatPulse;
            _bloom.Settings.BlurAmount = 1200f / (float)Math.Sqrt(_gameCamera.Position.Y);

            _label6.Value = _disc.Mass.ToString();

            _label.Caption = "Camera";
            _label.Value = _gameCamera.Position.ToString();
            _label2.Caption = "Update Time";
            _label2.Value = _aveUpdateTime.ToString();
            //_label2.Value = _gameCamera.Up.ToString();
            _label3.Caption = "Theta";
            _label3.Value = _gameCamera.Theta.ToString();
            _label4.Caption = "Target";
            _label4.Value = _gameCamera.Target.ToString();

            CheckCollisions(gameTime);

            if (_gravityOn)
            {
                var mult = _gravityBoost ? 10f : 1f;
                _disc.Mass -= 0.1f * mult;
            }

            _disc.Update(gameTime);
            _indicator.Update(gameTime);
            if (_gameCamera.Position.Y >= 5000)
            {
                _visibleMatter.Clear();
            }
            else
            {
                _visibleMatter = _visibleMatter.Where(mass => mass.Radius * 2f / viewRect.Width > 0.0008).ToList();
            }

            base.Update(gameTime);
        }

        private const float G = -1f;
        private void ApplyGravity(Matter mass, GameTime gameTime)
        {
            if (_gravityOn)
            {
                var dx = mass.Position.X - _disc.Position.X;
                var dy = mass.Position.Z - _disc.Position.Z;
                var dsquared = dx * dx + dy * dy;
                var distance = (float)Math.Sqrt(dsquared);
                if (distance <= mass.Radius) return;
                var f = (_gravityBoost ? 10f : 1f) * G * _disc.Mass / (dsquared);
                var fx = f * dx / distance;
                var fy = f * dy / distance;
                if (fx < 0.001f && fy < 0.001f) return;
                mass.Velocity = new Vector3(mass.Velocity.X + fx * (float)gameTime.ElapsedGameTime.TotalSeconds, 0f, mass.Velocity.Z + fy * (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (mass.Velocity.Length() > maxSpeed * 3)
                {
                    var v = mass.Velocity;
                    v.Normalize();
                    mass.Velocity = v * maxSpeed * 3;
                }
            }
        }

        private void CheckCollisions(GameTime gameTime)
        {
            foreach (var mass in _visibleMatter.Where(m => m.Radius < _disc.Radius).ToArray())
            {
                if (Contains(_disc.Position, _disc.Radius, mass.Position, mass.Radius, 0.8f))
                {
                    _disc.Mass += mass.Mass;
                    mass.Quad.Items.Remove(mass);
                    _matter.Remove(mass);
                    mass.Quad = null;
                    mass.Dispose();
                }
            }
            foreach (var mass in _visibleMatter.Where(m => m.Radius >= _disc.Radius).ToArray())
            {
                if (_disc.BoundingSphere.Contains(mass.BoundingSphere) == ContainmentType.Intersects)
                {
                    // get angle between two circle centers and determine overlap to move the mass
                    var dx = mass.Position.X - _disc.Position.X;
                    var dy = mass.Position.Z - _disc.Position.Z;
                    var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    var minDistance = mass.Radius + _disc.Radius;
                    var delta = minDistance - distance;
                    mass.Position = new Vector3(mass.Position.X + delta * dx / distance, mass.Position.Y, mass.Position.Z + delta * dy / distance);
                    mass.Velocity = _disc.Velocity;
                    mass.Update(gameTime);
                }
            }
        }

        private bool Contains(Vector3 position1, float radius1, Vector3 position2, float radius2, float percent)
        {
            if (position2.X < position1.X + radius1 && position2.X > position1.X - radius1 && position2.Z < position1.Z + radius1 && position2.Z > position1.Z - radius1)
            {
                var dx = position1.X - position2.X;
                var dy = position1.Z - position2.Z;
                var distance = (float)Math.Sqrt(dx * dx + dy * dy);
                return distance / radius1 <= percent;
            }
            return false;
        }

        private RectangleF GetViewRect(Vector3[] corners, float planeDepth)
        {
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

        private string GetQuadsBoundariesText(QuadTree<IMovable3>.Quad[] quads)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach(var quad in quads)
            {
                sb.AppendLine(quad.Bounds.ToString());
            }

            return sb.ToString();
        }

        private void MoveCamera(Camera _gameCamera, Disc disc, GameTime gameTime)
        {
            if (disc.Position.X > _gameCamera.Position.X)
                _gameCamera.PanRight(disc.Position.X - _gameCamera.Position.X);
            else if (disc.Position.X < _gameCamera.Position.X)
                _gameCamera.PanLeft(_gameCamera.Position.X - disc.Position.X);

            if (disc.Position.Z > _gameCamera.Position.Z)
                _gameCamera.PanDown(disc.Position.Z - _gameCamera.Position.Z);
            else if (disc.Position.Z < _gameCamera.Position.Z)
                _gameCamera.PanUp(_gameCamera.Position.Z - disc.Position.Z);
        }
        float maxSpeed = 100f;
        private void MovePlayer(Disc s, GameTime gameTime)
        {
            // vy = ay * dt + v0;
            Vector3 velocity = s.Velocity;
            Vector3 position = s.Position;
            var w2 = GraphicsDevice.Viewport.Width / 2f;
            var h2 = GraphicsDevice.Viewport.Height / 2f;
            

            if (velocity.Y > 0)
                velocity.Y = velocity.Y - (float)(2f * 9.8f * gameTime.ElapsedGameTime.TotalSeconds);
            // First test along the X axis, flipping the velocity if a collision occurs.
            if (s.Position.X < -worldSize + s.Radius)
            {
                position.X = -worldSize + s.Radius;
                velocity.X = 0f;
            }
            else if (s.Position.X > worldSize - s.Radius)
            {
                position.X = worldSize - s.Radius;
                velocity.X = 0f;
            }
            else
            {
                var vx = (_mouseState.Position.X - w2) / w2;
                velocity.X = vx * maxSpeed;
            }

            // Then we test the Y axis
            if (s.Position.Y < s.Radius)
            {
                position.Y = s.Radius;
                velocity.Y = 0f;
            }
            else if (s.Position.Y > worldSize - s.Radius)
            {
                position.Y = worldSize - s.Radius;
                velocity.Y = 0f;
            }

            // And lastly the Z axis
            if (s.Position.Z < -worldSize + s.Radius)
            {
                position.Z = -worldSize + s.Radius;
                velocity.Z = 0f;
            }
            else if (s.Position.Z > worldSize - s.Radius)
            {
                position.Z = worldSize - s.Radius;
                velocity.Z = 0f;
            }
            else
            {
                var vy = (_mouseState.Position.Y - h2) / h2;
                velocity.Z = vy * maxSpeed;
            }
            s.Position = position;
            s.Velocity = velocity;

            _indicator.Velocity = velocity;
            var iv = velocity;
            iv.Normalize();

            var dist = (float)(Math.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z) / maxSpeed) * 7f * _gameCamera.View.Translation.Z / -100f;

            _indicator.Position = new Vector3((float)((s.Radius + dist) * iv.X + position.X ), 10f, (float)((s.Radius + dist) * iv.Z + position.Z));
            _label7.Value = s.Position.ToString() + ", Radius: " + _disc.Radius.ToString();
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
                        _zoom *= 2f;
                    }
                    else
                    {
                        // Zoom out 
                        _gameCamera.ZoomOut(2f);
                        _zoom /= 2f;
                    }
                }
            }
            finally
            {
                _label5.Caption = "Mouse";
                _label5.Value = _cursor.Position.ToString();
                _mouseState = state;
            }
        }

        double _lastFiredDelta = double.MaxValue;
        bool _gravityOn = false;
        bool _gravityBoost = false;
        private void UpdateKeyboardState(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            try
            {
                Vector3 camPos = _gameCamera.Position;
                Vector3 playPos = Vector3.Zero;
                Vector3 centered = camPos - playPos;
                _gravityOn = false;
                _gravityBoost = false;
                if (state.IsKeyDown(Keys.Down))
                {
                    _gameCamera.OrbitVertically(0.5f);
                }
                else if (state.IsKeyDown(Keys.Up))
                {
                    _gameCamera.OrbitVertically(-0.5f);
                }
                if (state.IsKeyDown(Keys.PageUp))
                {
                    _gameCamera.ZoomOut(16f);
                }
                else if (state.IsKeyDown(Keys.PageDown))
                {
                    _gameCamera.ZoomIn(16f);
                }
                if (state.IsKeyDown(Keys.Left))
                {
                    _gameCamera.OrbitHorizontally(-0.5f);
                }
                else if (state.IsKeyDown(Keys.Right))
                {
                    _gameCamera.OrbitHorizontally(0.5f);
                }
                if (state.IsKeyDown(Keys.OemMinus))
                {
                    _gameCamera.ZoomOut(2f);
                }
                else if (state.IsKeyDown(Keys.OemPlus))
                {
                    _gameCamera.ZoomIn(2f);
                }
                if (state.IsKeyDown(Keys.OemComma))
                {
                    _gameCamera.Roll(-0.5f);
                }
                else if (state.IsKeyDown(Keys.OemPeriod))
                {
                    _gameCamera.Roll(0.5f);
                }
                if (state.IsKeyDown(Keys.A))
                {
                    _gameCamera.PanLeft(0.25f);
                }
                else if (state.IsKeyDown(Keys.D))
                {
                    _gameCamera.PanRight(0.25f);
                }
                if (state.IsKeyDown(Keys.W))
                {
                    _gameCamera.PanUp(0.25f);
                }
                else if (state.IsKeyDown(Keys.S))
                {
                    _gameCamera.PanDown(0.25f);
                }
                else if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
                {
                    if (_disc.Mass >= 510f)
                    {
                        _gravityOn = true;
                        if (state.IsKeyDown(Keys.B))
                        {
                            _gravityBoost = true;
                        }
                    }
                }
                if (state.IsKeyDown(Keys.Space) && _lastFiredDelta > 333d)
                {
                    _lastFiredDelta = 0;
                    CreateSplitter(gameTime);
                }
                if (state.GetPressedKeys().Length > 0)
                {
                    _label.Caption = "Camera";
                    _label.Value = _gameCamera.Position.ToString();
                    _label2.Caption = "Up";
                    _label2.Value = _gameCamera.Up.ToString();
                    _label3.Caption = "Theta";
                    _label3.Value = _gameCamera.Theta.ToString();
                    _label4.Caption = "Target";
                    _label4.Value = _gameCamera.Target.ToString();
                }
                _lastFiredDelta += gameTime.ElapsedGameTime.TotalMilliseconds;
                Label.ShowLabels = System.Windows.Input.Keyboard.IsKeyToggled(System.Windows.Input.Key.CapsLock);
            }
            finally
            {
                _keyboardState = state;
            }
        }

        private void CreateSplitter(GameTime gameTime)
        {
            var v = _disc.Velocity;
            v.Normalize();
            var splitter = new Splitter(GraphicsDevice, 2f, _splitter2D)
            {
                Position = _disc.Position + (v * _disc.Radius),
                Velocity = v * maxSpeed
            };
            _splitters.Add(splitter);
            _disc.Mass -= 10f;
        }

        private static void BounceSphereInWorld(IMovable3 s, GameTime gameTime)
        {
            if (s.Velocity == Vector3.Zero) return;
            if (s.Velocity.Length() <= 0.001)
            {
                s.Velocity = Vector3.Zero;
                return;
            }

            s.Velocity = s.Velocity * 0.99996f;

            // vy = ay * dt + v0;
            Vector3 velocity = s.Velocity;
            Vector3 position = s.Position;
            if (velocity.Y > 0)
                velocity.Y = velocity.Y - (float)(2f * 9.8f * gameTime.ElapsedGameTime.TotalSeconds);
            // First test along the X axis, flipping the velocity if a collision occurs.
            if (s.Position.X < -worldSize)
            {
                position.X = -worldSize;
                if (s.Velocity.X < 0f)
                    velocity.X *= -1f;
            }
            else if (s.Position.X > worldSize)
            {
                position.X = worldSize ;
                if (s.Velocity.X > 0f)
                    velocity.X *= -1f;
            }

            //// Then we test the Y axis
            //if (s.Position.Y < s.Radius)
            //{
            //    position.Y = s.Radius;
            //    if (s.Velocity.Y < 0f)
            //        velocity.Y *= -1f;
            //}
            //else if (s.Position.Y > worldSize - s.Radius)
            //{
            //    position.Y = worldSize - s.Radius;
            //    if (s.Velocity.Y > 0f)
            //        velocity.Y *= -1f;
            //}

            // And lastly the Z axis
            if (s.Position.Z < -worldSize)
            {
                position.Z = -worldSize;
                if (s.Velocity.Z < 0f)
                    velocity.Z *= -1f;
            }
            else if (s.Position.Z > worldSize)
            {
                position.Z = worldSize;
                if (s.Velocity.Z > 0f)
                    velocity.Z *= -1f;
            }
            s.Position = position;
            s.Velocity = velocity;
            
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        //protected override void Draw(GameTime gameTime)
        //{
        //    GraphicsDevice.Clear(Color.Black);

        //    // Create a view and projection matrix for our camera
        //    foreach (Genesoid player in _players)
        //        player.Draw(_gameCamera.View, _gameCamera.Projection);
        //    foreach (Circle star in _stars)
        //        star.Draw(_gameCamera.View, _gameCamera.Projection);
        //    foreach (Splitter splitter in _splitters)
        //        splitter.Draw(_gameCamera.View, _gameCamera.Projection);
        //    _sun.Draw(_gameCamera.View, _gameCamera.Projection);
        //    _earth.Draw(_gameCamera.View, _gameCamera.Projection);
        //    _cursor.Draw(_gameCamera.View, _gameCamera.Projection);
        //    _disc.Draw(_gameCamera.View, _gameCamera.Projection);
        //    _indicator.Draw(_gameCamera.View, _gameCamera.Projection);
        //    base.Draw(gameTime);
        //}

       
        protected override void Draw(GameTime gameTime)
        {

            var frustrum = new BoundingFrustum(_gameCamera.View * _gameCamera.Projection);
            var corners = frustrum.GetCorners();
            var viewRect = GetViewRect(corners, frustrum.Near.D);

            GraphicsDevice.SetRenderTarget(_renderTarget1);
            GraphicsDevice.Clear(Color.TransparentBlack);
            _sun.Draw(_gameCamera.View, _gameCamera.Projection);
            _earth.Draw(_gameCamera.View, _gameCamera.Projection);
            if (_gravityOn)
            {
                _disc.Draw(_gameCamera.View, _gameCamera.Projection);
            }
            _bloom.Draw(_renderTarget1, _renderTarget2);
            

            GraphicsDevice.SetRenderTarget(null);
            //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            //_spriteBatch.Draw(_background, new Rectangle(0, 0, _screenW, _screenH), Color.White);
            //_spriteBatch.End();

            foreach (var mass in _visibleMatter)
                mass.Draw(_gameCamera.View, _gameCamera.Projection);
            foreach (Circle star in _stars)
                star.Draw(_gameCamera.View, _gameCamera.Projection);

            _spriteBatch.Begin(0, BlendState.AlphaBlend);
            _spriteBatch.Draw(_renderTarget2, new Rectangle(0, 0, _screenW, _screenH), Color.White);
            _spriteBatch.End();

            _sun.Draw(_gameCamera.View, _gameCamera.Projection);
            _earth.Draw(_gameCamera.View, _gameCamera.Projection);
            _cursor.Draw(_gameCamera.View, _gameCamera.Projection);
            _disc.Draw(_gameCamera.View, _gameCamera.Projection);
            _indicator.Draw(_gameCamera.View, _gameCamera.Projection);

            foreach (var marker in _quadMarkers)
                marker.Draw(_gameCamera.View, _gameCamera.Projection);
            foreach (Splitter splitter in _splitters)
                splitter.Draw(_gameCamera.View, _gameCamera.Projection);

            base.Draw(gameTime);
        }
    }
}
