using GB.Emulator;
using GB.Emulator.Cart;
using NUnit.Framework;

namespace GB.Tests
{
    public class CartridgeTests
    {
        private const string pkmnRedFile = @"c:\roms\pkmnRed.gb";
        private const string pkmnBlueFile = @"c:\roms\pkmnBlue.gb";
        Cartridge cart;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void OpensRomFile()
        {
            cart = new Cartridge(pkmnRedFile);
            Assert.That(string.IsNullOrEmpty(cart.Info.Name), Is.False);
        }

        [Test]
        public void ReadsRomTitle()
        {
            cart = new Cartridge(pkmnRedFile);
            Assert.That(cart.Info.Name, Is.EqualTo("POKEMON RED"));

            cart = new Cartridge(pkmnBlueFile);
            Assert.That(cart.Info.Name, Is.EqualTo("POKEMON BLUE"));
        }

        [Test]
        public void ReadsCorrectRomSize_Returns1mb()
        {
            cart = new Cartridge(pkmnRedFile);
            Assert.That(cart.Info.Size, Is.EqualTo(RomSize.m1));
        }

        [Test]
        public void ReadBytes_ReadsCorrectBytes()
        {
            cart = new Cartridge(pkmnRedFile);
            var firstByte = cart.ReadByte(AddressHelper.NINTENDO_LOGO);
            Assert.That(firstByte, Is.EqualTo(0xce));
        }

        [Test]
        public void ReadsCartridgeType_PokemonRed()
        {
            cart = new Cartridge(pkmnRedFile);

            Assert.That(cart.Info.Type, Is.EqualTo(CartridgeType.MBC3_RAM_BATTERY));
        }

        [Test]
        public void ReadsInternation_PokemonRed()
        {
            cart = new Cartridge(pkmnRedFile);
            Assert.That(cart.Info.Destination, Is.EqualTo(Destination.Not_Japan));
        }

        [Test]
        public void ReadsCartridgeRam_PokemonRed()
        {
            cart = new Cartridge(pkmnRedFile);
            Assert.That(cart.Info.ExternalRam, Is.EqualTo(ExRam.k32));
        }
    }
}