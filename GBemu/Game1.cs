using Emulator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GBemu
{
    public class Game1 : Game
    {
        private const int GAMEBOY_WIDTH = 160;
        private const int GAMEBOY_HEIGHT = 144;
        Texture2D GBVideo;

        GameBoy gameBoy;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        /// <summary>
        /// Sets up the monogame framework
        /// </summary>
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        /// <summary>
        /// Set up my variables and start the running of the game.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            this._graphics.PreferredBackBufferWidth = GAMEBOY_WIDTH* 6;
            this._graphics.PreferredBackBufferHeight = GAMEBOY_HEIGHT*6;
            this._graphics.ApplyChanges();
            GBVideo = new Texture2D(GraphicsDevice, GAMEBOY_WIDTH, GAMEBOY_HEIGHT);
            gameBoy.Run();
            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            GBVideo = new Texture2D(this.GraphicsDevice, GAMEBOY_WIDTH, GAMEBOY_HEIGHT, false, SurfaceFormat.Color);
            // TODO: use this.Content to load your game content here
            gameBoy = new GameBoy(@"c:\roms\Tetris (W) (V1.1) [!].gb");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            FillBufferFromEmulator();

            Rectangle dst = GenerateBlackBars();

            this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            this._spriteBatch.Draw(GBVideo, dst, Color.White);
            this._spriteBatch.End();
            base.Draw(gameTime);
        }


        /// <summary>
        /// Extracts the video data from the emulator and writes it to a texture to be drawn on the screen.
        /// </summary>
        private void FillBufferFromEmulator()
        {
            GBVideo.SetData(this.gameBoy.Pixels);
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
                int barHeight = (Window.ClientBounds.Height - height) / 2;
                dst = new Rectangle(0, barHeight, Window.ClientBounds.Width, height);
            }
            else
            {
                //We need bars on the sides.
                int width = (int)((Window.ClientBounds.Height * preferredAspect) + 0.5f);
                int barWidth = (Window.ClientBounds.Width - width) / 2;
                dst = new Rectangle(barWidth, 0, width, Window.ClientBounds.Height);
            }

            GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
            return dst;
        }

    }
}
