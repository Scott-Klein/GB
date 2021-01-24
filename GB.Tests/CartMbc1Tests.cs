using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GB.Emulator;
using NUnit.Framework;
namespace GB.Tests
{
    class CartMbc1Tests
    {
        CartridgeMBC1 cart;
        [Test]
        public void AddressingExternalRamDoesNothing()
        {
            cart = new CartridgeMBC1(TestCartsPaths.pkmnBlueFile);
            var bank = cart.Bank;
            var bankMode = cart.BankMode;

            cart.WriteByte(0x1000, 0x0a);
            Assert.That(bank, Is.EqualTo(cart.Bank));
            Assert.That(bankMode, Is.EqualTo(cart.BankMode));
        }

        [Test]
        public void SelectsRomBank()
        {
            cart = new CartridgeMBC1(TestCartsPaths.pkmnBlueFile);

            cart.WriteByte(0x3000, 0x10);
            Assert.That(cart.Bank, Is.EqualTo(0x10));
        }

        [Test]
        public void SelectsRomSet()
        {
            cart = new CartridgeMBC1(TestCartsPaths.pkmnBlueFile);


            cart.WriteByte(0x3000, 0x0); //set rombank
            Assert.That(cart.Bank, Is.EqualTo(1));

            cart.WriteByte(0x3000, 0x1);
            Assert.That(cart.Bank, Is.EqualTo(1));

            cart.WriteByte(0x3000, 0x2);
            Assert.That(cart.Bank, Is.EqualTo(2));

            cart.WriteByte(0x5000, 0x1);
            Assert.That(cart.Bank, Is.EqualTo(0x22));
        }

        [Test]
        public void SelectsBankMode()
        {
            cart = new CartridgeMBC1(TestCartsPaths.pkmnBlueFile);

            cart.WriteByte(0x7000, 0);
            Assert.That(cart.BankMode, Is.EqualTo(BankingMode.ROM));

            cart.WriteByte(0x7000, 1);
            Assert.That(cart.BankMode, Is.EqualTo(BankingMode.RAM));
        }
    }
}
