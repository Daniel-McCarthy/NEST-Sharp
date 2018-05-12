using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes
{
    class PPU
    {
        private byte[] ppuRam = new byte[0x4000];

        private byte getPPURegister()
        {
            return Core.cpu.readCPURam(0x2000, true);
        }

    }
}
