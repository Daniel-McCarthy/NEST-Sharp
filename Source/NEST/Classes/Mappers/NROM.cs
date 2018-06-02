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

        public static void loadRom(Rom romFile)
        {
            if (romFile.getMapperSetting() == 0)
            {
                loadPrgRom(ref romFile);
                loadChrRom(ref romFile);

                Core.ppu.isNametableMirrored = true;
                Core.ppu.isHorizNametableMirror = (!romFile.getVerticalMirroring());
            }
        }

        public static void loadChrRom(ref Rom romFile)
        {
            int dataLength = romFile.getExactDataLength();
            byte[] data = null;

            data = romFile.readBytesFromAddressToEnd(16); //16 in order to skip the INES header


            if (data != null)
            {
                uint chrDataAddress = (uint)(0x4000 * romFile.getProgramRomSize());
                if (romFile.getCHRRomSize() != 0)
                {
                    for (int i = 0; i < 0x2000; i++)
                    {
                        if ((chrDataAddress + i) < data.Length)
                        {
                            Core.ppu.writePPURamByte((ushort)i, data[chrDataAddress + i]);
                        }
                    }
                }
            }
        }

        public static void loadPrgRom(ref Rom romFile)
        {
            int dataLength = romFile.getExactDataLength();
            byte[] data = null;

            data = romFile.readBytesFromAddressToEnd(16); //16 in order to skip the INES header


            if(data != null)
            {
                int programSize = romFile.getProgramRomSize() * 0x4000;

                for (int i = 0; i < programSize; i++)
                {
                    ushort address = (ushort)(0x8000 + i);
                    if (address >= 0x8000 && address <= 0xFFFF)
                    {
                        Core.cpu.directCPURamWrite(address, data[i]);
                    }
                }

                // If there is only one bank, then mirror
                if(romFile.getProgramRomSize() == 1)
                {
                    for (int i = 0; i < 0x4000; i++)
                    {
                        ushort address = (ushort)(0xC000 + i);
                        if (address >= 0xC000 && address <= 0xFFFF)
                        {
                            Core.cpu.directCPURamWrite(address, data[i]);
                        }
                    }
                }
            }
        }
    }
}
