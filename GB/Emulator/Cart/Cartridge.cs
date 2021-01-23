using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GB.Emulator
{
    public class Cartridge
    {
        private readonly byte[] rom;

        private RomInfo info;

        public RomInfo Info { get { return this.info; } }

        public Cartridge(string romFile)
        {
            this.rom = File.ReadAllBytes(romFile);

        }

        public byte ReadByte(ushort addr)
        {
            return rom[addr]; //dummy
        }


    }
}
