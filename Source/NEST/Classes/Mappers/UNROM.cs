using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class UNROM
    {

        public static bool isMapperWriteAddress(ushort address)
        {
            return address >= 0x8000 && address <= 0xFFFF;
        }


    }
}
