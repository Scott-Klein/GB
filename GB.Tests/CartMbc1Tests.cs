using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GB.Emulator;
using NUnit.Framework;
namespace GB.Tests.Cart
{
    class CartMbc1Tests
    {
        Cartridge cart;
        private const string testCartFile = TestCartsPaths.tetris_attack; // a path to an MBC1 Rom, this one has 512k rom size without ram.
        private const string testCartFileRam = TestCartsPaths.tetris_plus; //path to MBC1 rom with ram.

        [Test]
        public void CorrectCartTypeRead()
        {
            cart = new Cartridge(testCartFile);
            Assert.That(cart.Info.Type, Is.EqualTo(CartridgeType.MBC1));

            cart = new Cartridge(testCartFileRam);
            Assert.That(cart.Info.Type, Is.EqualTo(CartridgeType.MBC1_RAM_BATTERY));
        }
    }
}
