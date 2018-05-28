using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class CNROM
    {

        public static void loadRom(Rom romFile)
        {
            if (romFile.getMapperSetting() == 3)
            {
                loadPrgRomBank(ref romFile, 0x8000);
                loadChrRomBank(ref romFile, 0x0000, 0);

                Core.ppu.isNametableMirrored = true;
                Core.ppu.isHorizNametableMirror = (!romFile.getVerticalMirroring());
            }
        }

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

        public static void loadChrRomBank(ref Rom romFile, ushort address, byte bankNumber)
        {
            //0x0000-0x1FFF Chr Rom Data Bank

            int dataLength = romFile.getExactDataLength();
            byte[] data = null;

            data = romFile.readBytesFromAddressToEnd(16); //16 in order to skip the INES header


            if (data != null)
            {
                uint bankAddress = (uint)(0x2000 * (bankNumber));
                uint prgRomDataAddress = (uint)(0x4000 * romFile.getProgramRomSize()); //Skip trainer if it exists

                for (int i = 0; i < 0x2000; i++)
                {
                    ushort writeAddress = (ushort)(address + i);
                    uint readAddress = (uint)(prgRomDataAddress + bankAddress + i);
                    if (writeAddress >= 0x0000 && writeAddress <= 0x1FFF && readAddress < data.Length)
                    {
                        Core.ppu.writePPURamByte(writeAddress, data[readAddress]);
                    }
                }
            }
        }

        public static void loadPrgRomBank(ref Rom romFile, ushort address)
        {
            //0x8000 16kb - 32 kb non-switchable program rom

            int dataLength = romFile.getExactDataLength();
            byte[] data = null;

            data = romFile.readBytesFromAddressToEnd(16); //16 in order to skip the INES header


            if (data != null)
            {
                uint prgRomDataAddress = (uint)((romFile.getTrainerIncluded()) ? 0x0200 : 0x0000); //Skip trainer if it exists
                uint prgRomSize = (uint)(romFile.getProgramRomSize() * 0x4000);

                for (int i = 0; i < prgRomSize; i++)
                {
                    ushort writeAddress = (ushort)(address + i);
                    uint readAddress = (uint)(prgRomDataAddress + i);
                    if (writeAddress >= 0x8000 && writeAddress <= 0xFFFF && readAddress < data.Length)
                    {
                        Core.cpu.directCPURamWrite(writeAddress, data[readAddress]);
                    }
                }
            }
        }
    }
}
