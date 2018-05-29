using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class UNROM
    {
        public static void loadRom(Rom romFile)
        {
            if (romFile.getMapperSetting() == 2)
            {
                loadPrgRomBank(ref romFile, 0x8000, 0);
                loadPrgRomBank(ref romFile, 0xC000, (byte)(romFile.getProgramRomSize() - 1));

                if (romFile.getCHRRomSize() > 0)
                {
                    loadChrRomBank(ref romFile);
                }

            }
        }

        public static bool isMapperWriteAddress(ushort address)
        {
            return address >= 0x8000 && address <= 0xFFFF;
        }

        public static void writeUNROM(ushort address, byte value)
        {
            bool writeToUNROMRomPageRegister = address >= 0x8000 && address <= 0xFFFF;

            if (writeToUNROMRomPageRegister)
            {
                byte bankNumber = (byte)(value & 0x0F);
                loadPrgRomBank(ref Core.rom, 0x8000, bankNumber);
            }
        }

        public static void loadPrgRomBank(ref Rom romFile, ushort address, byte bankNumber)
        {
            //0x8000-0xBFFF, starts with first rom bank
            //0xC000-0xFFFF, fixed to last rom bank

            int dataLength = romFile.getExactDataLength();
            byte[] data = null;

            data = romFile.readBytesFromAddressToEnd(16); //16 in order to skip the INES header


            if (data != null)
            {
                uint bankAddress = (uint)(0x4000 * (bankNumber));
                uint prgRomDataAddress = (uint)((romFile.getTrainerIncluded()) ? 0x0200 : 0x0000); //Skip trainer if it exists

                for (int i = 0; i < 0x4000; i++)
                {
                    ushort writeAddress = (ushort)(address + i);
                    uint readAddress = (uint)(prgRomDataAddress + bankAddress + i);
                    if (writeAddress >= 0x8000 && writeAddress <= 0xFFFF && readAddress < data.Length)
                    {
                        Core.cpu.directCPURamWrite(writeAddress, data[readAddress]);
                    }
                }
            }
        }

        public static void loadChrRomBank(ref Rom romFile)
        {
            //0x0000-0x0FFF Chr Rom Data Bank 1
            //0x1000-0x1FFF Chr Rom Data Bank 2

            int dataLength = romFile.getExactDataLength();
            byte[] data = null;

            data = romFile.readBytesFromAddressToEnd(16); //16 in order to skip the INES header

            if (data != null)
            {
                uint chrDataAddress = (uint)(0x4000 * romFile.getProgramRomSize());

                for (int i = 0; i < 0x4000; i++)
                {
                    if ((chrDataAddress + i) < data.Length)
                    {
                        Core.ppu.writePPURamByte((ushort)i, data[chrDataAddress + i]);
                    }
                }
            }
        }

    }
}
