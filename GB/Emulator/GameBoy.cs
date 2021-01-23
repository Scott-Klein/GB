using GB.Emulator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Emulator
{
    public class GameBoy
    {
        private readonly Cartridge cart;
        public Cartridge Cart { get { return this.cart; }  }

        public GameBoy(string rom)
        {
            this.cart = new Cartridge(rom);
            //build the hardware and load the cartridge/
        }


        public void Run()
        {

        }
    }
}
