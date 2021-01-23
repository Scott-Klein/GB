using Emulator;
using System;

namespace GB
{
    class Program
    {
        static void Main(string[] args)
        {
            var gb = new GameBoy();

            gb.Run();
        }
    }
}
