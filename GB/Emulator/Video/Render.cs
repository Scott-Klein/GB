using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator.Video
{
    public class Render
    {
        MMU mmu;
        LCDCRegisters LCDC;
        byte[] VRAM;

        public byte SCX;
        public byte SCY;

        int tileRow;
        byte[][] TileMapCache;
        int BGTileOffset;
        const int BG_TILE_ADDR_MASK = 0x3ff;
        const int GB_SCREEN_WIDTH = 160;
        const int GB_SCREEN_HEIGHT = 144;
        private int[] rawPixels;
        private int ScanLine;
        public int[] Pixels
        {
            get
            {
                //convert rawPixels into 32bit color;
                return rawPixels;
            }
        }
        public Render(MMU mmu, LCDCRegisters regs, byte[] vram)
        {
            this.VRAM = vram;
            this.LCDC = regs;
            this.mmu = mmu;
            this.rawPixels = new int[GB_SCREEN_WIDTH * GB_SCREEN_HEIGHT];
        }
        public void RenderLine(int scanLine)
        {
            ScanLine = scanLine;
            //Render background and window
            if (LCDC.BGEnabled_0)
            {
                RenderBackGround();
                RenderWindow();
            }
            else
            {
                //Set the entire scanline white.
            }
            RenderSprite();
        }
        int WinTileOffset;
        int winTileRow;
        int WY;
        int WX;
        private void RenderWindow()
        {
            WY = mmu.rb(0xff4a);
            WX = mmu.rb(0xff4b) - 7;
            if (ScanLine < WY || !LCDC.WindowEnable_5 || WX >= 160)
            {
                return;
            }

            BGTileOffset = LCDC.WindowTileMap_6 ? 0x9800 : 0x9c00;

            int offsetY = ScanLine - WY;
            if (offsetY >> 3 != winTileRow)
            {
                CacheBackgroundRow(offsetY);
            }
            int offsetX;

            offsetY = offsetY & 3;//get the sub tile offset.
            for (int i = WX; i < GB_SCREEN_WIDTH; i++)
            {
                offsetX = i - WX;
                var tileBytes = TileMapCache[(i - WX) >> 3];

                //get the sub tile offset
                offsetX = offsetX & 0x7;

                int color = GetTilePixel(offsetY, offsetX, tileBytes);
                rawPixels[(ScanLine * GB_SCREEN_WIDTH) + i] = BGColor(color);
            }

        }

        private int GetTilePixel(int offsetY, int offsetX, byte[] tileBytes)
        {
            byte msb = tileBytes[offsetY << 1];
            byte lsb = tileBytes[(offsetY << 1) + 1];

            //Extract the high bit, low bit, bitwise or them together.
            int bitH = ((0x80 >> offsetX) & msb) >> (8 - offsetX);
            int bitL = ((0x80 >> offsetX) & lsb) >> (7 - offsetX);
            return  (bitH | bitL);

            //convert to the current pallette
        }

        const int TOTAL_SPRITES = 40;
        const int SPRITE_BUFFER_SIZE =10;
        private void RenderSprite()
        {
            int spriteHeight = 8;
            if (LCDC.SpritesEnabled_1)
            {
                List<Sprite> sprites = new List<Sprite>();
                //Search OAM memory
                //reading memory is expensive in the emulator,
                //So we are going to try to continue early each loop
                //on sprites that do fail any off these conditions
                //
                //>>Sprite X-Position must be greater than 0
                //>>LY + 16 must be greater than or equal to Sprite Y-Position
                //>>LY + 16 must be less than Sprite Y-Position + Sprite Height (8 in Normal Mode, 16 in Tall-Sprite-Mode)
                //..The amount of sprites already stored in the OAM Buffer must be less than 10
                for (int sprite = 0xfe00; sprite < 0xfe9f; sprite += 4)
                {
                    Sprite s = new Sprite();
                    s.Ypos = mmu.rb(sprite);
                    if ((ScanLine + 16) < s.Ypos || (ScanLine + 16) > (s.Ypos + spriteHeight))
                        continue;
                    s.Xpos = mmu.rb(sprite + 1);
                    if (s.Xpos < 0)
                        continue;
                    s.TileNum = mmu.rb(sprite + 2);
                    s.Flags = mmu.rb(sprite + 3);

                    if (sprites.Count < SPRITE_BUFFER_SIZE)
                    {
                        sprites.Add(s);
                    }
                }

                for (int i = 0; i < sprites.Count; i++)
                {
                    //get the tile
                    byte[] tile = new byte[TILE_BYTE_SIZE];
                    for (int j = 0; j < TILE_BYTE_SIZE; j++)
                    {
                        tile[j] = VRAM[(sprites[i].TileNum * TILE_BYTE_SIZE) + j];
                    }

                    int yOffset = ScanLine - sprites[i].Ypos;
                    for (int xPos = sprites[i].Xpos; xPos < sprites[i].Xpos + 8; xPos++)
                    {

                        var color = GetTilePixel(yOffset, xPos, tile);
                        rawPixels[(ScanLine * GB_SCREEN_WIDTH) + xPos] = SpriteColor(color, sprites[i].Flags & 0x10);
                    }
                }
            }
        }
        private void RenderBackGround()
        {
            //Select correct tilemap
            BGTileOffset = LCDC.BGTileMap_3 ? 0x9800 : 0x9c00;

            //Select the correct tile map row and store it in memory
            int absoluteY = ScanLine + SCY;

            if (absoluteY >> 3 != tileRow)
            {
                //swap the cached background row
                tileRow = absoluteY >> 3;
                CacheBackgroundRow(tileRow);
            }
            tileRow = absoluteY >> 3;
            int tilePixelRow = absoluteY & 0x7;

            int absoluteX = SCX;
            for (int i = 0; i < GB_SCREEN_WIDTH; i++)
            {
                absoluteX = SCX + i;
                int tile = absoluteX >> 3;
                var tileBytes = TileMapCache[tile];
                int OffsetX = absoluteX & 0x7;
                int OffsetY = absoluteY & 0x7;

                int color = GetTilePixel(OffsetY, OffsetX, tileBytes);


                //convert to the current pallette
                rawPixels[(ScanLine * GB_SCREEN_WIDTH) + i] = BGColor(color);
            }
        }
        int SpriteColor(int raw, int BP0)
        {
            byte palette;
            if (BP0 == 0)
            {
                palette = mmu.rb(0xff48);
            }
            else
            {
                palette = mmu.rb(0xff49);
            }

            switch (raw)
            {
                case 0:
                    return palette & 3;
                case 1:
                    return (palette >> 2) & 0x3;
                case 2:
                    return (palette >> 4) & 0x3;
                case 3:
                    return (palette >> 6) & 0x3;
            }
            return 0;
        }
        int BGColor(int raw)
        {
            byte BPG = mmu.rb(0xff47);
            switch (raw)
            {
                case 0:
                    return BPG & 0x3;
                case 1:
                    return (BPG >> 2) & 0x3;
                case 2:
                    return (BPG >> 4) & 0x3;
                case 3:
                    return (BPG >> 6) & 0x3;
            }
            return 3;
        }
        const int TILE_COLUMNS = 32;
        const int TILE_BYTE_SIZE = 16;
        private byte[][] CacheBackgroundRow(int row)
        {
            byte[][] Cache = new byte[32][];
            int index = row * TILE_COLUMNS;

            for (int i = 0; i < TILE_COLUMNS; i++)
            {
                byte[] tile = new byte[TILE_BYTE_SIZE];
                var tileId = VRAM[(BGTileOffset - 0x9000) + index];
                for (int j = 0; j < TILE_BYTE_SIZE; j++)
                {
                    if (LCDC.BGWindowTileSet_4)
                    {
                        // Unsigned byte offset of 0x8000
                        tile[j] = VRAM[(tileId * TILE_BYTE_SIZE) + j];
                    }
                    else
                    {
                        // Signed byte offset from 0x9000
                        tile[j] = VRAM[0x1000 + ((sbyte)tileId * TILE_BYTE_SIZE) + j];
                    }
                }
                Cache[i] = tile;
            }
            return Cache;
        }
    }
    public struct Sprite
    {
        public byte Ypos;
        public byte Xpos;
        public byte TileNum;
        public byte Flags;
    }
}
