using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Diagnostics
{
    public class Label : DrawableGameComponent
    {
        public static bool ShowLabels = false;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        public Label(Game game, SpriteBatch Batch, SpriteFont Font)
            : base(game)
        {
            spriteFont = Font;
            spriteBatch = Batch;
            Caption = Value = "";
        }

        public string Caption { get; set; }
        public string Value { get; set; }
        public Vector2 Position { get; set; }

        public override void Draw(GameTime gameTime)
        {
            if (ShowLabels)
            {
                spriteBatch.Begin();
                string text = string.Format("{0}: {1}", Caption, Value);
                try
                {
                    spriteBatch.DrawString(spriteFont, text, Position + Vector2.One, Color.Black);
                }
                catch { }
                try
                {
                    spriteBatch.DrawString(spriteFont, text, Position, Color.White);
                }
                catch { }
                spriteBatch.End();
                base.Draw(gameTime);
            }
        }
    }
}
