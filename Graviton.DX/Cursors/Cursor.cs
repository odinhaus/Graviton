using Graviton.XNA.Cameras;
using Graviton.XNA.Shapes.ThreeD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Cursors
{
    /// <summary>
    /// Cursor is a DrawableGameComponent that draws a cursor on the screen.
    /// </summary>
    public class Cursor
    {
        #region Fields and Properties

        // this constant controls how fast the gamepad moves the cursor. this constant
        // is in pixels per second.
        const float CursorSpeed = 400.0f;

        Sphere _cursor;

        // Position is the cursor position, and is in screen space. 
        private Vector3 position;
        public Vector3 Position
        {
            get { return position; }
        }

        public Game Game { get; private set; }
        public Camera Camera { get; private set; }

        #endregion

        #region Initialization

        public Cursor(Game game, Camera gameCamera)
        {
            Game = game;
            Camera = gameCamera;
            LoadContent();
        }

        // LoadContent needs to load the cursor texture and find its center.
        // also, we need to create a SpriteBatch.
        protected void LoadContent()
        {
            _cursor = new Sphere(Game.GraphicsDevice, 2f, 16, Color.FromNonPremultiplied(255, 255, 255, 128));

            // we want to default the cursor to start in the center of the screen
            Viewport vp = Game.GraphicsDevice.Viewport;
            position.X = vp.X + (vp.Width / 2);
            position.Y = vp.Y + (vp.Height / 2);
        }

        #endregion

        #region Update

        public void Update(GameTime gameTime)
        {
            // We use different input on each platform:
            // On Xbox, we use the GamePad's DPad and left thumbstick to move the cursor around the screen.
            // On Windows, we directly map the cursor to the location of the mouse.
            // On Windows Phone, we use the primary touch point for the location of the cursor.
#if XBOX
            UpdateXboxInput(gameTime);
#elif WINDOWS
            UpdateWindowsInput();
#elif WINDOWS_PHONE
            UpdateWindowsPhoneInput();
#endif
            Vector3 farSource = new Vector3(position.X, position.Y, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = Game.GraphicsDevice.Viewport.Unproject(farSource,
                Camera.Projection, Camera.View, Matrix.Identity);
            position = nearPoint;

            _cursor.Position = position;
            _cursor.Update(gameTime);
        }

        /// <summary>
        /// Handles input for Xbox 360.
        /// </summary>
        private void UpdateXboxInput(GameTime gameTime)
        {
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            // we'll create a vector2, called delta, which will store how much the
            // cursor position should change.
            Vector3 delta = new Vector3(currentState.ThumbSticks.Left,0f);

            // down on the thumbstick is -1. however, in screen coordinates, values
            // increase as they go down the screen. so, we have to flip the sign of the
            // y component of delta.
            delta.Y *= -1;

            // check the dpad: if any of its buttons are pressed, that will change delta as well.
            if (currentState.DPad.Up == ButtonState.Pressed)
            {
                delta.Y = -1;
            }
            if (currentState.DPad.Down == ButtonState.Pressed)
            {
                delta.Y = 1;
            }
            if (currentState.DPad.Left == ButtonState.Pressed)
            {
                delta.X = -1;
            }
            if (currentState.DPad.Right == ButtonState.Pressed)
            {
                delta.X = 1;
            }

            // normalize delta so that we know the cursor can't move faster than CursorSpeed.
            if (delta != Vector3.Zero)
            {
                delta.Normalize();
            }

            // modify position using delta, the CursorSpeed constant defined above, and
            // the elapsed game time.
            position += delta * CursorSpeed *
                (float)gameTime.ElapsedGameTime.TotalSeconds;

            // clamp the cursor position to the viewport, so that it can't move off the screen.
            Viewport vp = Game.GraphicsDevice.Viewport;
            position.X = MathHelper.Clamp(position.X, vp.X, vp.X + vp.Width);
            position.Y = MathHelper.Clamp(position.Y, vp.Y, vp.Y + vp.Height);
            
        }

        /// <summary>
        /// Handles input for Windows.
        /// </summary>
        private void UpdateWindowsInput()
        {
            MouseState mouseState = Mouse.GetState();
            position.X = mouseState.X;
            position.Y = mouseState.Y;
           
        }

        /// <summary>
        /// Handles input for Windows Phone.
        /// </summary>
        private void UpdateWindowsPhoneInput()
        {
            TouchCollection touches = TouchPanel.GetState();
            if (touches.Count > 0)
            {
                position = new Vector3(touches[0].Position, 0f);
            }
        }

        #endregion

        #region Draw

        public void Draw(Matrix view, Matrix projection)
        {
            _cursor.Draw(view, projection);
        }

        #endregion

        #region CalculateCursorRay

        // CalculateCursorRay Calculates a world space ray starting at the camera's
        // "eye" and pointing in the direction of the cursor. Viewport.Unproject is used
        // to accomplish this. see the accompanying documentation for more explanation
        // of the math behind this function.
        public Ray CalculateCursorRay(Matrix projectionMatrix, Matrix viewMatrix)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = Position;
            Vector3 farSource = new Vector3(Position.X, Position.Y, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = Game.GraphicsDevice.Viewport.Unproject(nearSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = Game.GraphicsDevice.Viewport.Unproject(farSource,
                projectionMatrix, viewMatrix, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        #endregion
    }
}
