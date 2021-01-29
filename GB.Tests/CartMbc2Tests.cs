using GB.Emulator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Tests.Cart
{
    class CartMbc2Tests
    {
        Cartridge cart;
        public const string testRom = TestCartsPaths.kirby_pinball;
        [Test]
        public void SetRomBank()
        {
            cart = new Cartridge(testRom);

            Assert.That(cart.Info.Type, Is.EqualTo(CartridgeType.MBC2_BATTERY));
            
        }
    }
}
