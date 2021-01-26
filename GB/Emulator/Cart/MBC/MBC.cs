using System.IO;
using System.IO.MemoryMappedFiles;

namespace GB.Emulator.Cart.MBC
{
    public abstract class MBC
    {
        protected int lowBank;
        protected int highBank;
        protected int ramBank;
        protected BankingMode bankMode;
        protected readonly ExRam exRam;
        protected bool ramEnable;

        public byte[] ROM { get; set; }
        public MemoryMappedViewAccessor RAM { get; set; }

        public MBC(byte[] rom, RomInfo info)
        {
            ROM = rom;
            this.exRam = info.ExternalRam;
            if (this.exRam != ExRam.None)
            {
                RAM = MemoryMappedFile.CreateFromFile(info.FileName.Remove(info.FileName.IndexOf('.')) + ".sav", FileMode.OpenOrCreate, null, info.ExRamSize).CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite);
            }
        }
    }
}