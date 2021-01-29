using GB.Emulator;
using GB.Emulator.Cart;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Tests.Cart
{
    class CartMbc3Tests
    {
        Cartridge cart;

        [Test]
        public void EnableRam()
        {
            cart = new Cartridge(TestCartsPaths.pkmnBlueFile);
            //enable exRam
            cart.WriteByte(0x0000, 0x0a);
            cart.WriteByte(0xa150, 0x1a);
            Assert.That(cart.ReadByte(0xa150) == 0x1a);

            //disable exRam
            cart.WriteByte(0x0000, 0x2);
            Assert.That(cart.ReadByte(0xa150) == 0xff);
        }


        [Test]
        public void WriteBytesToRam()
        {
            cart = new Cartridge(TestCartsPaths.pkmnRedFile);
            //enable exRam
            cart.WriteByte(0x0000, 0x0a); //enable ram


            cart.WriteByte(0x4000, 0x0); // select the first ram bank.

            cart.WriteByte(0xb000, 0x1a);

            cart.WriteByte(0x4000, 0x1);

            cart.WriteByte(0xb000, 0x1b);

            cart.WriteByte(0x4000, 0x0);

            Assert.That(cart.ReadByte(0xb000), Is.EqualTo(0x1a));

            cart.WriteByte(0x4000, 0x1);

            Assert.That(cart.ReadByte(0xb000), Is.EqualTo(0x1b));
           
        }
    }
}
