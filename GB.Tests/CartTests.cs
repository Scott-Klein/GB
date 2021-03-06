using GB.Emulator;
using GB.Emulator.Cart;
using NUnit.Framework;

namespace GB.Tests.Cart
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
            Assert.That(cart.Info.Name, Is.EqualTo("TETRIS"));

        }

        [Test]
        public void ReadsCorrectRomSize_Returns1mb()
        {
            cart = new Cartridge(testcart);
            Assert.That(cart.Info.Size, Is.EqualTo(RomSize.k32));
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

            Assert.That(cart.Info.Type, Is.EqualTo(CartridgeType.ROM_ONLY));
        }

        [Test]
        public void ReadsInternational()
        {
            cart = new Cartridge(testcart);
            Assert.That(cart.Info.Destination, Is.EqualTo(Destination.Japan));
        }
    }
}