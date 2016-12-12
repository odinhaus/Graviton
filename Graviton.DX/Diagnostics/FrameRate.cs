using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.XNA.Diagnostics
{
    public class FrameRate : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;


        public FrameRate(Game game, SpriteBatch Batch, SpriteFont Font)
            : base(game)
        {
            spriteFont = Font;
            spriteBatch = Batch;
        }

        ulong uc = 0, fc = 0;
        long mem = 0;
        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                mem = GC.GetTotalMemory(false);
                frameCounter = 0;
            }
            uc++;
        }


        public override void Draw(GameTime gameTime)
        {
            fc++;
            frameCounter++;

            string fps = string.Format("fps: {0} mem : {1} upd: {2} frm: {3}", frameRate, mem, uc, fc);
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, fps, new Vector2(3, 3), Color.Black);
            spriteBatch.DrawString(spriteFont, fps, new Vector2(2, 2), Color.White);
            spriteBatch.End();
        }
    }
}
