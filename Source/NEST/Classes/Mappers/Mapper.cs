using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class Mapper
    {
        public const int NROM_ID   = 0x00;
        public const int MMC1_ID   = 0x01;
        public const int UxROM_ID  = 0x02;
        public const int CNROM_ID  = 0x03;
        public const int MMC3_ID   = 0x04;
        public const int MMC5_ID   = 0x05;

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
                else if (mapperSetting == 4)
                {
                    return MMC3.isMapperWriteAddress(address);
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
                else if (mapperSetting == 4)
                {
                    MMC3.writeMMC3(address, value);
                }

            }

        }

    }
}
