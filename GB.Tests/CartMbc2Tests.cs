using GB.Emulator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Tests
{
    class CartMbc2Tests
    {
        CartridgeMBC2 cart;
        [Test]
        public void SetRomBank()
        {
            cart = new CartridgeMBC2(TestCartsPaths.pkmnBlueFile);

            cart.WriteByte(0x0, 0x80 | 0x10);
            Assert.That(cart.Bank, Is.EqualTo(0x10 % 0xf));
            
        }
    }
}
