using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator.Cart
{
    class MBC1 : ICartROM
    {
        private int lowBank;
        private int highBank;
        private int ramBank;
        private BankingMode bankMode;
        private readonly ExRam exRam;
        private bool ramEnable;
        private int ramBytes;

        public MBC1(byte[] rom,  RomInfo info)
        {
            ROM = rom;
            this.exRam = info.ExternalRam;
            if (this.exRam != ExRam.None)
            {
                RAM = MemoryMappedFile.CreateFromFile(info.FileName.Remove(info.FileName.IndexOf('.')) + ".sav", FileMode.OpenOrCreate, null, info.ExRamSize).CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);
            }
        }

        public byte[] ROM { get; set; }
        public MemoryMappedViewAccessor RAM { get; set; }

        public void WriteByte(ushort addr, byte value)
        {
            switch (addr)
            {
                //ROM bank register, switch between banks 1-31
                case var a when a <= 0x1fff:
                    ramEnable = ((value & 0xf) & 0xa) == 0xa ? true : false; // check the lower 4 bits for 0xa, if found enable ram, anything else disables it.
                    break;
                case var a when a <= 0x3fff:
                    this.lowBank = (value & 0x1f);
                    if (this.lowBank == 0)
                    {
                        this.lowBank++;
                    }
                    break;
                //Switch the rom bank set, (1-31), (32-
                case var a when a >= 0x4000 && a <= 0x5fff && bankMode == BankingMode.ROM: // rom banking mode.
                    this.highBank = (value & 3) << 5;
                    break;
                case var a when a >= 0x4000 && a <= 0x5fff && bankMode == BankingMode.RAM:
                    this.ramBank = (value & 3);
                    break;
                case var a when a <= 0x7fff:
                    if ((value & 1) == 1)
                    {
                        bankMode = BankingMode.RAM;
                    }
                    else
                    {
                        bankMode = BankingMode.ROM;
                    }
                    break;
            }
        }

        public byte ReadByte(ushort addr)
        {
            return addr switch
            {
                var a when a <= 0x3fff && bankMode == BankingMode.ROM => ROM[a % ROM.Length],
                var a when a <= 0x3fff => ROM[(highBank * 0x4000 + a) % ROM.Length],
                var a when a <= 0x7fff => ROM[((highBank << 5) | lowBank) + (a & 0x3fff)],
                var a when ramEnable && exRam == ExRam.k2 && a > 0xa000 && a < 0xa7ff => RAM.ReadByte(a % 0x7ff), //2k ram
                var a when ramEnable && exRam == ExRam.k8 && a > 0xa000 && a < 0xbfff => RAM.ReadByte(a % 0xbfff), //8k, 1 bank
                var a when ramEnable && exRam == ExRam.k32 && a > 0xa000 && a < 0xbfff => RAM.ReadByte(a * ramBank % 0xbfff)
            };
        }
    }

    class MBC2 : ICartROM
    {
        public byte[] ROM { get; set; }

        public MBC2(byte[] rom)
        {
            ROM = rom;
        }

        public byte ReadByte(ushort addr)
        {
            throw new NotImplementedException();
        }

        public void WriteByte(ushort addr, byte value)
        {
            throw new NotImplementedException();
        }
    }
}
