using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class CNROM
    {


        public static bool isMapperWriteAddress(ushort address)
        {
            return address >= 0x8000 && address <= 0xFFFF;
        }

        public static void writeCNROM(ushort address, byte value)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                //Select CHR Rom Bank to swap to
                loadChrRomBank(ref Core.rom, 0x0000, (byte)(value & 0b11));
            }
        }

    }
}
