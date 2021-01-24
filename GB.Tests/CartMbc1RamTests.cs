using GB.Emulator;
using GB.Emulator.Cart;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Tests
{
    class CartMbc1RamTests
    {

        CartridgeMBC1_RAM cart;

        [Test]
        public void CanReadWriteToRam()
        {
            cart = new CartridgeMBC1_RAM(TestCartsPaths.pkmnBlueFile);
            Assert.That(cart.CanReadRam);
            Assert.That(cart.CanWriteRam);
        }

        [Test]
        public void EnableRam()
        {
            cart = new CartridgeMBC1_RAM(TestCartsPaths.pkmnBlueFile);
            //enable exRam
            cart.WriteByte(0x0000, 0x0a);
            Assert.That(cart.RamEnable);
        }
        [Test]
        public void EnableRamBankingMode()
        {
            cart = new CartridgeMBC1_RAM(TestCartsPaths.pkmnBlueFile);

            cart.WriteByte(0x7000, (byte)BankingMode.RAM);
            Assert.That(cart.BankMode, Is.EqualTo(BankingMode.RAM));
        }

        [Test]
        public void EnableRomBankingMode()
        {
            cart = new CartridgeMBC1_RAM(TestCartsPaths.pkmnBlueFile);

            cart.WriteByte(AddressHelper.BANK_MODE_START, (byte)BankingMode.ROM);
            Assert.That(cart.BankMode, Is.EqualTo(BankingMode.ROM));
        }

        [Test]
        public void WriteBytesToRam()
        {
            cart = new CartridgeMBC1_RAM(TestCartsPaths.pkmnBlueFile);
            //enable exRam
            cart.WriteByte(0x0000, 0x0a); //enable ram
            Assert.That(cart.RamEnable);

            cart.WriteByte(0x4000, 0x0); // select the first ram bank.
            Assert.That(cart.RAMbank, Is.EqualTo(0));

            cart.WriteByte(0x7000, (byte)BankingMode.RAM);// select ram banking mode.
            Assert.That(cart.BankMode, Is.EqualTo(BankingMode.RAM));

            cart.WriteByte(0xa001, 0xff);
            Assert.That(cart.ReadByte(0xa001), Is.EqualTo(0xff));
        }

        [Test]
        public void RamBanksAreIsolated()
        {
            cart = new CartridgeMBC1_RAM(TestCartsPaths.pkmnBlueFile);

            cart.WriteByte(0x0001, 0x0a);//enable ram

            cart.WriteByte(0x4000, 0x0);
            cart.WriteByte(0x7000, (byte)BankingMode.RAM);

            cart.WriteByte(0xa001, 0xff);
            cart.WriteByte(0x4000, 0x1);//change ram banks

            //Assert that we don't find the data we just wrote in the wrong bank.
            Assert.That(cart.ReadByte(0xa001), Is.Not.EqualTo(0xff));
        }

        [Test]
        public void RamDisable()
        {
            cart = new CartridgeMBC1_RAM(TestCartsPaths.pkmnBlueFile);

            cart.WriteByte(AddressHelper.SRAM_ENABLE_START, 0x0a);
            cart.WriteByte(AddressHelper.SRAM_ENABLE_START, 0x01);
            Assert.That(!cart.RamEnable);
            Assert.That(cart.ReadByte(AddressHelper.SRAM_START), Is.EqualTo(0xff));
        }
    }
}
