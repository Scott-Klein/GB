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

        byte SCX;
        byte SCY;

        int tileRow;
        byte[][] TileMapCache;
        int BGTileOffset;
        const int BG_TILE_ADDR_MASK = 0x3ff;
        const int GB_SCREEN_WIDTH = 160;
        const int GB_SCREEN_HEIGHT = 144;
        private int[] rawPixels;
        public int[] Pixels
        {
            get
            {
                //convert rawPixels into 32bit color;
                return rawPixels;
            }
        }
        public Render()
        {
            this.rawPixels = new int[GB_SCREEN_WIDTH * GB_SCREEN_HEIGHT];
        }
        public void RenderLine(int ScanLine)
        {
            //Render background and window
            if (LCDC.BGEnabled_0)
            {
                RenderBackGround(ScanLine);
                RenderWindow(ScanLine);
            }
            else
            {
                //Set the entire scanline white.
            }
            RenderSprite(ScanLine);
        }
        int WinTileOffset;
        int WY;
        int WX;
        private void RenderWindow(int ScanLine)
        {
            WY = mmu.rb(0xff4a);
            WX = mmu.rb(0xff4b) - 7;
            if (ScanLine < WY || !LCDC.WindowEnable_5 || WX >= 160)
            {
                return;
            }

            BGTileOffset = LCDC.WindowTileMap_6 ? 0x9800 : 0x9c00;

            int offsetY = ScanLine - WY;
            int offsetX;

            for (int i = 0; i < GB_SCREEN_WIDTH; i++)
            {
                offsetX = i
            }

        }
        private void RenderSprite(int ScanLine)
        {

        }
        private void RenderBackGround(int ScanLine)
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
                OffsetY = OffsetY << 1;

                byte msb = tileBytes[OffsetY];
                byte lsb = tileBytes[OffsetY + 1];
                int color = (msb & (1 << OffsetX)) << 1 | (lsb & (1 << OffsetX));
                rawPixels[(ScanLine * GB_SCREEN_WIDTH) + i] = BGColor(color);
            }
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

}
