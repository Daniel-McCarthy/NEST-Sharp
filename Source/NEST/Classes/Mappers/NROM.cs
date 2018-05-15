using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class NROM
    {
        const int mapperINESNumber = 0;

        //NROM is INES mapper 0.
        //It supports 16 KiB and 32 KiB ROMs
        //It supports 2 KiB and 4 KiB RAM
        //It does not support rom or ram bank swapping.
        // CPU RAM 0x8000-0xBFFF contains the first 16 KB of the ROM.
        // CPU RAM 0xC000-0xFFFF contains the last 16 KB of the ROM.

    }
}
