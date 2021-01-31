using GB.Emulator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Emulator
{
    public class GameBoy
    {
        private readonly Cartridge cart;
        private readonly PPU ppu;
        private readonly CPU cpu;
        private readonly MMU memory;

        public bool PowerSwitch { get; set; }
        public Cartridge Cart { get { return this.cart; }  }
        public int[] Pixels { get { return this.ppu.Renderer.Pixels;  } }

        public GameBoy(string rom)
        {
            this.cart = new Cartridge(rom);
            //build the hardware and load the cartridge/
            this.ppu = new PPU();
            this.memory = new MMU(this.cart, this.ppu);
            this.cpu = new CPU(this.memory);
            this.PowerSwitch = true;
        }


        public void Run()
        {
            for (int i = 0; i < 10000; i++)
            {
                this.cpu.Tick();
            }
        }
    }
}
