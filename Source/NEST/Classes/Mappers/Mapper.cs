using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class Mapper
    {

        public static bool isMapperWriteAddress(ushort address)
        {
            if (Core.rom != null)
            {
                int mapperSetting = Core.rom.getMapperSetting();

                if (mapperSetting == 1)
                {
                    return MMC1.isMapperWriteAddress(address);
                }
            }

            return false;
        }

    }
}
