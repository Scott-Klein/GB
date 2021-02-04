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
        private readonly Clock clock;
        public bool PowerSwitch { get; set; }
        public Cartridge Cart { get { return this.cart; }  }
        public int[] Pixels { get { return this.ppu.Renderer.Pixels;  } }

        public GameBoy(string rom)
        {
            this.cart = new Cartridge(rom);
            //build the hardware and load the cartridge/

            this.clock = new Clock();
            this.ppu = new PPU(clock);
            this.memory = new MMU(this.cart, this.ppu, this.clock);
            this.cpu = new CPU(this.memory, clock);
            this.PowerSwitch = true;
        }

        public void Run()
        {
            while (!ppu.V_BLANK)
            {
                this.cpu.Tick();
            }
            while (ppu.V_BLANK)
            {
                this.cpu.Tick();
            }
        }
    }
}
