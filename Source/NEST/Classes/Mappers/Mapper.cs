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
                else if (mapperSetting == 2)
                {
                    return UNROM.isMapperWriteAddress(address);
                }
                else if (mapperSetting == 3)
                {
                    return CNROM.isMapperWriteAddress(address);
                }
            }

            return false;
        }

        public static void writeToCurrentMapper(ushort address, byte value)
        {
            if (Core.rom != null)
            {
                int mapperSetting = Core.rom.getMapperSetting();

                if (mapperSetting == 1)
                {
                    MMC1.writeMMC1(address, value);
                }
                else if (mapperSetting == 2)
                {
                    UNROM.writeUNROM(address, value);
                }
                else if (mapperSetting == 3)
                {
                    CNROM.writeCNROM(address, value);
                }

            }

        }

    }
}
