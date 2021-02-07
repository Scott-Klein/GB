using System;
using System.Collections.Generic;

namespace GB.Emulator.Video
{
    public struct Sprite
    {
        public byte Flags;
        public byte TileNum;
        public byte Xpos;
        public byte Ypos;
    }

    public class Render
    {
        public byte SCX;
        public byte SCY;
        private const int BG_TILE_ADDR_MASK = 0x3ff;
        private const int SPRITE_PRIORITY = 0x80;
        private const int X_FLIP = 0x20;
        private const int Y_FLIP = 0x40;
        private const int GB_SCREEN_HEIGHT = 144;
        private const int GB_SCREEN_WIDTH = 160;
        private const int SPRITE_BUFFER_SIZE = 12; //10 is the correct number, but 12 is a work around for a graphical issue.
        private const int TILE_BYTE_SIZE = 16;
        private const int TILE_COLUMNS = 32;
        private const int TOTAL_SPRITES = 40;
        private int BGTileOffset;
        private LCDCRegisters LCDC;
        private PPU ppu;
        private int[] rawPixels;
        private int ScanLine;
        private byte[][] TileMapCache;
        private int tileRow;
        private byte[] VRAM;
        private int WinTileOffset;

        private int winTileRow;

        public byte WX;

        public byte WY;

        public Render(PPU ppu, LCDCRegisters regs, byte[] vram)
        {
            this.VRAM = vram;
            this.LCDC = regs;
            this.ppu = ppu;
            this.rawPixels = new int[GB_SCREEN_WIDTH * GB_SCREEN_HEIGHT];
        }

        public int[] Pixels
        {
            get
            {
                //convert rawPixels into 32bit color;
                return rawPixels;
            }
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
            if (LCDC.SpritesEnabled_1)
            {
                RenderSprite();
            }
            
        }
        private int BGColor(int raw)
        {
            byte BPG = ppu.ReadByte(0xff47);
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

        private byte[][] CacheBackgroundRow(int row)
        {
            byte[][] Cache = new byte[32][];
            int index = row * TILE_COLUMNS;

            for (int i = 0; i < TILE_COLUMNS; i++)
            {
                var tileId = VRAM[(BGTileOffset - 0x8000) + index + i];
                var tile = GetTile(tileId);
                Cache[i] = tile;
            }
            return Cache;
        }
        private byte[] GetTile(int tileId)
        {
            byte[] tile = new byte[TILE_BYTE_SIZE];

            if (LCDC.BGWindowTileSet_4)
            {
                Array.Copy(VRAM, tileId * TILE_BYTE_SIZE, tile, 0, TILE_BYTE_SIZE);
            }
            else
            {
                var off = 0x1000 + ((sbyte)tileId * TILE_BYTE_SIZE);
                Array.Copy(VRAM, off, tile, 0, TILE_BYTE_SIZE);
            }
            return tile;
        }

        private int GetTilePixel(int offsetY, int offsetX, byte[] tileBytes)
        {
            byte msb = tileBytes[offsetY << 1];
            byte lsb = tileBytes[(offsetY << 1) + 1];

            //Extract the high bit, low bit, bitwise or them together.
            int bitH = ((0x80 >> offsetX) & lsb);
            int bitL = ((0x80 >> offsetX) & msb);
            return ((bitH > 0 ? 0x2 : 0x0) | (bitL > 0 ? 0x1 : 0x0));

            //convert to the current pallette
        }

        private void RenderBackGround()
        {
            //Select correct tilemap
            BGTileOffset = LCDC.BGTileMap_3 ? 0x9c00 : 0x9800;

            //Select the correct tile map row and store it in memory
            int absoluteY = (ScanLine + SCY) % 256;//256 is the height of the bg tilemap.

            if (absoluteY >> 3 != tileRow)
            {
                //swap the cached background row
                tileRow = absoluteY >> 3;
                TileMapCache = CacheBackgroundRow(tileRow);
            }
            tileRow = absoluteY >> 3;
            int tilePixelRow = absoluteY & 0x7;

            int absoluteX = SCX;
            for (int i = 0; i < GB_SCREEN_WIDTH; i++)
            {
                absoluteX = SCX + i;
                int tile = absoluteX >> 3;
                var tileBytes = TileMapCache[tile % 0x20];
                int OffsetX = absoluteX & 0x7;
                int OffsetY = absoluteY & 0x7;

                int color = GetTilePixel(OffsetY, OffsetX, tileBytes);

                //convert to the current pallette
                rawPixels[(ScanLine * GB_SCREEN_WIDTH) + i] = BGColor(color);
            }
        }

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
                for (ushort sprite = 0xfe00; sprite < 0xfe9f; sprite += 4)
                {
                    Sprite s = new Sprite();

                    s.Xpos = ppu.ReadByte(sprite + 1);
                    if (s.Xpos <= 0)
                        continue;
                    s.Ypos = ppu.ReadByte(sprite);
                    if ((ScanLine + 16) < s.Ypos || (ScanLine + 16) > (s.Ypos + spriteHeight))
                        continue;
                    
                    s.TileNum = ppu.ReadByte(sprite + 2);
                    s.Flags = ppu.ReadByte(sprite + 3);

                    if (sprites.Count < SPRITE_BUFFER_SIZE)
                    {
                        sprites.Add(s);
                    }
                }

                //count through the sprite buffer backwards,
                //sprites that appear earlier in the buffer should go be on top, thus drawn last.
                for (int i = sprites.Count - 1; i >= 0; i--)
                {
                    int yOffset = ScanLine - (sprites[i].Ypos - 0x10);
                    byte[] tile = new byte[TILE_BYTE_SIZE];
                    Array.Copy(VRAM, sprites[i].TileNum * TILE_BYTE_SIZE, tile, 0, TILE_BYTE_SIZE);

                    //Array.Copy(VRAM, tileId * TILE_BYTE_SIZE, tile, 0, TILE_BYTE_SIZE);
                    //Array.Copy(VRAM, sprites[i].TileNum * TILE_BYTE_SIZE, tile, 0, TILE_BYTE_SIZE);

                    if (yOffset < 8 && yOffset >= 0)
                    {
                        for (int xPos = sprites[i].Xpos; xPos < sprites[i].Xpos + 8; xPos++)
                        {
                            int currentPixelAddress = (ScanLine * GB_SCREEN_WIDTH) + xPos - 8;

                            //if the flip flag is set, iterate through the pixels in the opposite way.
                            int xPixel = (sprites[i].Flags & X_FLIP) == 0 ? xPos - sprites[i].Xpos : 7 - (xPos - sprites[i].Xpos);

                            var color = GetTilePixel(yOffset, xPixel, tile);
                            //if transparent OR if the sprite is to be rendered below back ground.
                            if (color == 0 || ((sprites[i].Flags & SPRITE_PRIORITY) > 0 && rawPixels[currentPixelAddress] != 0))
                            {
                                continue;
                            }
                            rawPixels[currentPixelAddress] = SpriteColor(color, sprites[i].Flags & 0x10);
                        }
                    }
                }
            }
        }

        private void RenderWindow()
        {
            int wxCorrected = WX - 7;
            if (ScanLine < WY || !LCDC.WindowEnable_5 || wxCorrected - 7 >= 160)
            {
                return;
            }

            BGTileOffset = LCDC.WindowTileMap_6 ? 0x9c00 :0x9800;

            int offsetY = ScanLine - WY;
            if (offsetY >> 3 != winTileRow)
            {
                TileMapCache = CacheBackgroundRow(offsetY >> 3);
            }
            int offsetX;

            offsetY = offsetY & 7;//get the sub tile offset.
            for (int i = wxCorrected; i < GB_SCREEN_WIDTH; i++)
            {
                offsetX = i - wxCorrected;
                var tileBytes = TileMapCache[(i - wxCorrected) >> 3];

                //get the sub tile offset
                offsetX = offsetX & 0x7;

                int color = GetTilePixel(offsetY, offsetX, tileBytes);
                rawPixels[(ScanLine * GB_SCREEN_WIDTH) + i] = BGColor(color);
            }
        }
        public byte BP0;
        public byte BP1;
        private int SpriteColor(int raw, int palleteId)
        {
            byte palette;
            if (palleteId == 0)
            {
                palette = BP0;
            }
            else
            {
                palette = BP1;
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
    }
}