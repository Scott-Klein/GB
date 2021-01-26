using GB.Emulator;
using GB.Emulator.Cart;
using NUnit.Framework;

namespace GB.Tests
{
    public class CartridgeTests
    {
        private const string testcart = TestCartsPaths.tetris;
        Cartridge cart;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void OpensRomFile()
        {
            cart = new Cartridge(testcart);
            Assert.That(string.IsNullOrEmpty(cart.Info.Name), Is.False);
        }

        [Test]
        public void ReadsRomTitle()
        {
            cart = new Cartridge(testcart);
            Assert.That(cart.Info.Name, Is.EqualTo("POKEMON RED"));

        }

        [Test]
        public void ReadsCorrectRomSize_Returns1mb()
        {
            cart = new Cartridge(testcart);
            Assert.That(cart.Info.Size, Is.EqualTo(RomSize.m1));
        }

        [Test]
        public void ReadBytes_ReadsCorrectBytes()
        {
            cart = new Cartridge(testcart);
            var firstByte = cart.ReadByte(AddressHelper.NINTENDO_LOGO);
            Assert.That(firstByte, Is.EqualTo(0xce));
        }

        [Test]
        public void ReadsCartridgeType()
        {
            cart = new Cartridge(testcart);

            Assert.That(cart.Info.Type, Is.EqualTo(CartridgeType.MBC3_RAM_BATTERY));
        }

        [Test]
        public void ReadsInternational()
        {
            cart = new Cartridge(testcart);
            Assert.That(cart.Info.Destination, Is.EqualTo(Destination.Not_Japan));
        }
    }
}