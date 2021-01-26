using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.Emulator
{
    public class Clock
    {
        public int M { get; set; }
        public int T { get; set; }
        public Clock()
        {
            M = 0;
            T = 0;
        }
    }
}
