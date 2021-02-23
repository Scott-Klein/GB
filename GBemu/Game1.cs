using Emulator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using System;

namespace GBemu
{
    public class Game1 : Game
    {
        private const int GAMEBOY_WIDTH = 160;
        private const int GAMEBOY_HEIGHT = 144;
        Texture2D GBVideo;
        Color[] gbFrameBuffer;
        GameBoy gameBoy;
        private Desktop _desktop;
        string romPath;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        /// <summary>
        /// Sets up the monogame framework
        /// </summary>
        public Game1(string path)
        {
            this.romPath = path;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Set up my variables and start running the game.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this._graphics.PreferredBackBufferWidth = GAMEBOY_WIDTH;
            this._graphics.PreferredBackBufferHeight = GAMEBOY_HEIGHT;
            this._graphics.ApplyChanges();
            GBVideo = new Texture2D(GraphicsDevice, GAMEBOY_WIDTH, GAMEBOY_HEIGHT);
            gbFrameBuffer = new Color[GAMEBOY_HEIGHT * GAMEBOY_WIDTH];
        }

        protected override void LoadContent()
        {
            MyraEnvironment.Game = this;
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            GBVideo = new Texture2D(this.GraphicsDevice, GAMEBOY_WIDTH, GAMEBOY_HEIGHT, false, SurfaceFormat.Color);
            // TODO: use this.Content to load your game content here
            //gameBoy = new GameBoy(@"c:\roms\Tetris (W) (V1.1) [!].gb");
            gameBoy = new GameBoy(@romPath);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            gameBoy.JoyPad.Reset();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                gameBoy.JoyPad.Start = true;
            if (Keyboard.GetState().IsKeyDown(Keys.RightShift))
                gameBoy.JoyPad.Select = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Z))
                gameBoy.JoyPad.A = true;
            if (Keyboard.GetState().IsKeyDown(Keys.X))
                gameBoy.JoyPad.B = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                gameBoy.JoyPad.Up = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                gameBoy.JoyPad.Down = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                gameBoy.JoyPad.Left = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                gameBoy.JoyPad.Right = true;

            // TODO: Add your update logic here

            gameBoy.Run();
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            FillBufferFromEmulator();

            Rectangle dst = GenerateBlackBars();
            
            this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            this._spriteBatch.Draw(GBVideo, dst, Color.White);
            this._spriteBatch.End();

            base.Draw(gameTime);
        }


        /// <summary>
        /// Extracts the video data from the emulator and writes it to a texture to be drawn on the screen.
        /// </summary>
        private void FillBufferFromEmulator()
        {
            gbFrameBuffer = new Color[GAMEBOY_HEIGHT * GAMEBOY_WIDTH];
            for (int i = 0; i < this.gameBoy.Pixels.Length; i++)
            {
                int n = this.gameBoy.Pixels[i];
                switch(n)
                {
                    case 0:
                        gbFrameBuffer[i] = new Color(255, 255, 255);
                        break;
                    case 1:
                        gbFrameBuffer[i] = new Color(170, 170, 170);
                        break;
                    case 2:
                        gbFrameBuffer[i] = new Color(85, 85, 85);
                        break;
                    case 3:
                        gbFrameBuffer[i] = new Color(0, 0, 0);
                        break;
                }
            }
            GBVideo.SetData(gbFrameBuffer);
        }


        /// <summary>
        /// Upon window resize the aspect ratio can be preserved by calling this function, and black bars will be drawn allowing for the scaling of the video.
        /// </summary>
        /// <returns>A rectangle object that helps position the video within the game window</returns>
        private Rectangle GenerateBlackBars()
        {
            float outputAspect = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;
            float preferredAspect = GAMEBOY_WIDTH / (float)GAMEBOY_HEIGHT;

            Rectangle dst;
            if (outputAspect <= preferredAspect)
            {
                //We need bars on the top and the bottom
                int height = (int)((Window.ClientBounds.Width / preferredAspect) + 0.5f);
                height = (height / GAMEBOY_HEIGHT) * GAMEBOY_HEIGHT;
                int barHeight = (Window.ClientBounds.Height - height) / 2;
                dst = new Rectangle(0, barHeight, Window.ClientBounds.Width, height);
            }
            else
            {
                //We need bars on the sides.
                int width = (int)((Window.ClientBounds.Height * preferredAspect) + 0.5f);
                width = (width / GAMEBOY_WIDTH) * GAMEBOY_WIDTH;
                int barWidth = (Window.ClientBounds.Width - width) / 2;
                dst = new Rectangle(barWidth, 0, width, Window.ClientBounds.Height);
            }

            GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
            return dst;
        }

    }
}
