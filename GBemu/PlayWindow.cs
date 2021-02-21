using Emulator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBemu
{
    class PlayWindow
    {
        private const int GAMEBOY_WIDTH = 160;
        private const int GAMEBOY_HEIGHT = 144;
        private GameBoy gameBoy;
        private Texture2D video;
        public Texture2D Video 
        { 
            get
            {
                return video;
            }
        }

        private Color[] frameBuffer;

        public PlayWindow(string gameBoyRomPath)
        {
            gameBoy = new GameBoy(gameBoyRomPath);
        }

    }
}
