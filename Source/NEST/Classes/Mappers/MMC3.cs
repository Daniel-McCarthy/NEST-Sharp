using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class MMC3
    {

        public static bool isMapperWriteAddress(ushort address)
        {
            return address >= 0x8000 && address <= 0xFFFF;
        }


        public static void loadChrRomBank(ref Rom romFile, ushort address, ushort bankSize, byte bankNumber)
        {
            //0x0000-0x07FF Chr Rom, 2k switchable bank
            //0x0800-0x0FFF Chr Rom, 2k switchable bank
            //0x1000-0x13FF Chr Rom, 1k switchable bank
            //0x1400-0x17FF Chr Rom, 1k switchable bank
            //0x1800-0x1BFF Chr Rom, 1k switchable bank
            //0x1C00-0x1FFF Chr Rom, 1k switchable bank

            //Order of 1k and 2k rom banks are switchable

            int dataLength = romFile.getExactDataLength();
            byte[] data = null;

            data = romFile.readBytesFromAddressToEnd(16); //16 in order to skip the INES header


            if (data != null)
            {
                uint bankAddress = (uint)(0x0400 * bankNumber);
                uint prgRomDataAddress = (uint)(0x2000 * (Core.rom.getProgramRomSize() * 2)); //Skip trainer if it exists

                for (int i = 0; i < bankSize; i++)
                {
                    ushort writeAddress = (ushort)(address + i);
                    uint readAddress = (uint)(prgRomDataAddress + bankAddress + i);
                    if (writeAddress >= 0x0000 && writeAddress < (address + bankSize) && readAddress < data.Length)
                    {
                        Core.ppu.writePPURamByte(writeAddress, data[readAddress]);
                    }
                }
            }
        }

        public static void loadPrgRomBank(ref Rom romFile, ushort address, byte bankNumber)
        {
            //0x8000-0x9FFF, switchable rom bank
            //0xA000-0xBFFF, switchable rom bank

            //0xC000-0xDFFF, fixed to second to last rom bank
            //0xE000-0xFFFF, fixed to last rom bank

            //The order of the swappable and fixed banks can be swithed.

            int dataLength = romFile.getExactDataLength();
            byte[] data = null;

            data = romFile.readBytesFromAddressToEnd(16); //16 in order to skip the INES header


            if (data != null)
            {
                int bankSize = 0x2000;
                uint bankAddress = (uint)(bankSize * (bankNumber));
                uint prgRomDataAddress = (uint)((romFile.getTrainerIncluded()) ? 0x0200 : 0x0000); //Skip trainer if it exists

                for (int i = 0; i < bankSize; i++)
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
    }
}
