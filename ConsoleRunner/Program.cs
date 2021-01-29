using System;
using Emulator;
namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var gb = new GameBoy(@"c:\roms\pkmnBlue.gb");
            gb.Run();
        }
    }
}
