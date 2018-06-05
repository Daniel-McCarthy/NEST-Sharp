using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class MMC3
    {
        public static byte bankRegister;
        public static bool prgRomBankingMode;
        public static bool chrRomBankOrder;

        public static byte irqLatch;
        public static byte irqCounter;
        public static bool irqEnabled;
        public static bool irqPending;

        public static void loadRom(Rom romFile)
        {
            if (romFile.getMapperSetting() == 4)
            {
                loadChrRomBank(ref Core.rom, 0x0000, 0x0800, 0);
                loadChrRomBank(ref Core.rom, 0x0800, 0x0800, 0);
                loadChrRomBank(ref Core.rom, 0x1000, 0x0800, 0);
                loadChrRomBank(ref Core.rom, 0x1800, 0x0800, 0);

                loadPrgRomBank(ref Core.rom, 0x8000, 0);
                loadPrgRomBank(ref Core.rom, 0xA000, 0);
                loadPrgRomBank(ref Core.rom, 0xC000, (byte)((Core.rom.getProgramRomSize() * 2) - 2));
                loadPrgRomBank(ref Core.rom, 0xE000, (byte)((Core.rom.getProgramRomSize() * 2) - 1));

                Core.ppu.isNametableMirrored = false;
            }
        }

        public static bool isMapperWriteAddress(ushort address)
        {
            return address >= 0x8000 && address <= 0xFFFF;
        }

        public static void writeMMC3(ushort address, byte value)
        {
            bool isAddressEven = (address % 2) == 0;
            bool bankSelect = (address >= 0x8000 && address <= 0x9FFF) && isAddressEven;
            bool bankSwapNumber = (address >= 0x8000 && address <= 0x9FFF) && !isAddressEven;
            bool mirroringSelection = (address >= 0xA000 && address <= 0xBFFF) && isAddressEven;
            bool ramSetting = (address >= 0xA000 && address <= 0xBFFF) && !isAddressEven;
            bool irqLatchSetting = (address >= 0xC000 && address <= 0xDFFF) && isAddressEven;
            bool irqReset = (address >= 0xC000 && address <= 0xDFFF) && !isAddressEven;
            bool irqDisable = (address >= 0xE000 && address <= 0xFFFF) && isAddressEven;
            bool irqEnable = (address >= 0xE000 && address <= 0xFFFF) && !isAddressEven;

            if (address >= 0x8000 && address <= 0xFFFF)
            {
                if (bankSelect)
                {
                    bankRegister = (byte)(value & 0b111);
                    prgRomBankingMode = (value & 0b01000000) != 0;
                    chrRomBankOrder = (value & 0b10000000) != 0;
                }
                else if (bankSwapNumber)
                {
                    byte bankNumber = value;

                    if (bankRegister == 0)
                    {
                        //Swap 0x0000-0x07FF / 0x1000-0x17FF 2kb CHR Rom Bank
                        ushort ramAddress = (ushort)(chrRomBankOrder ? 0x1000 : 0x0000);
                        ushort bankSize = 0x0800;

                        loadChrRomBank(ref Core.rom, ramAddress, bankSize, bankNumber);
                    }
                    else if (bankRegister == 1)
                    {
                        //Swap 0x0800-0x0FFF / 0x1800-0x1FFF 2kb CHR Rom Bank
                        ushort ramAddress = (ushort)(chrRomBankOrder ? 0x1800 : 0x0800);
                        ushort bankSize = 0x0800;

                        loadChrRomBank(ref Core.rom, ramAddress, bankSize, bankNumber);
                    }
                    else if (bankRegister == 2)
                    {
                        //Swap 0x1000-0x13FF / 0x0000-0x03FF 1kb CHR Rom Bank
                        ushort ramAddress = (ushort)(chrRomBankOrder ? 0x0000 : 0x1000);
                        ushort bankSize = 0x0400;

                        loadChrRomBank(ref Core.rom, ramAddress, bankSize, bankNumber);
                    }
                    else if (bankRegister == 3)
                    {
                        //Swap 0x1400-0x17FF / 0x0400-0x07FF 1kb CHR Rom Bank
                        ushort ramAddress = (ushort)(chrRomBankOrder ? 0x0400 : 0x1400);
                        ushort bankSize = 0x0400;

                        loadChrRomBank(ref Core.rom, ramAddress, bankSize, bankNumber);
                    }
                    else if (bankRegister == 4)
                    {
                        //Swap 0x1800-0x1BFF / 0x0800-0x0BFF 1kb CHR Rom Bank
                        ushort ramAddress = (ushort)(chrRomBankOrder ? 0x0800 : 0x1800);
                        ushort bankSize = 0x0400;

                        loadChrRomBank(ref Core.rom, ramAddress, bankSize, bankNumber);
                    }
                    else if (bankRegister == 5)
                    {
                        //Swap 0x1C00-0x1FFF / 0x0C00-0x0FFF 1kb CHR Rom Bank
                        ushort ramAddress = (ushort)(chrRomBankOrder ? 0x0C00 : 0x1C00);
                        ushort bankSize = 0x0400;

                        loadChrRomBank(ref Core.rom, ramAddress, bankSize, bankNumber);
                    }
                    else if (bankRegister == 6)
                    {
                        //Swap 0x8000-0x9FFF / 0xC000-0xDFFF 8kb PRG Rom Bank
                        ushort ramAddress = (ushort)(prgRomBankingMode ? 0xC000 : 0x8000);
                        loadPrgRomBank(ref Core.rom, ramAddress, (byte)(bankNumber));

                        //Fix bank to second to last rom bank
                        ushort fixedBankRamAddress = (ushort)(prgRomBankingMode ? 0x8000 : 0xC000);
                        loadPrgRomBank(ref Core.rom, fixedBankRamAddress, (byte)((Core.rom.getProgramRomSize() * 2) - 2));
                    }
                    else if (bankRegister == 7)
                    {
                        //Swap 0xA000-0xBFFF 8kb PRG Rom Bank
                        loadPrgRomBank(ref Core.rom, 0xA000, bankNumber);
                    }
                }
                else if (mirroringSelection)
                {
                    Core.ppu.isNametableMirrored = true;
                    if ((value & 0x01) != 0)
                    {
                        //Set Nametable Mirroring to horizontal
                        Core.ppu.isHorizNametableMirror = true;
                        Core.ppu.isVertNametableMirror = false;
                    }
                    else
                    {
                        //Set Nametable Mirroring to vertical
                        Core.ppu.isHorizNametableMirror = false;
                        Core.ppu.isVertNametableMirror = true;
                    }
                }
                else if (ramSetting)
                {
                    bool ramWritesDisabled = (value & 0b01000000) != 0;
                    bool ramEnabled = (value & 0b10000000) != 0;
                    //TODO: Implement these settings
                }
                else if (irqLatchSetting)
                {
                    irqLatch = value;
                }
                else if (irqReset)
                {
                    irqCounter = irqLatch;
                }
                else if (irqDisable)
                {
                    irqEnabled = false;
                    //TODO: Register pending interrupts
                }
                else if (irqEnable)
                {
                    irqEnabled = true;
                }
            }
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

            uint bankAddress = (uint)(0x0400 * bankNumber);
            uint prgRomDataAddress = (uint)(0x2000 * (Core.rom.getProgramRomSize() * 2)); //Skip trainer if it exists

            for (int i = 0; i < bankSize; i++)
            {
                ushort writeAddress = (ushort)(address + i);
                uint readAddress = (uint)(prgRomDataAddress + bankAddress + i);
                if (writeAddress >= 0x0000 && writeAddress < (address + bankSize) && readAddress < (Core.rom.getExactDataLength() - 16)) //16 to skip the INES header.
                {
                    Core.ppu.writePPURamByte(writeAddress, Core.rom.readByte(16 + readAddress));
                }
            }
        }

        public static void loadPrgRomBank(ref Rom romFile, ushort address, byte bankNumber)
        {
            //0x8000-0x9FFF, switchable rom bank
            //0xA000-0xBFFF, switchable rom bank

            //0xC000-0xDFFF, fixed to second to last rom bank
            //0xE000-0xFFFF, fixed to last rom bank

            //The order of the swappable and fixed banks can be switched.


            int bankSize = 0x2000;
            uint bankAddress = (uint)(bankSize * (bankNumber));
            uint prgRomDataAddress = (uint)((romFile.getTrainerIncluded()) ? 0x0200 : 0x0000); //Skip trainer if it exists

            for (int i = 0; i < bankSize; i++)
            {
                ushort writeAddress = (ushort)(address + i);
                uint readAddress = (uint)(prgRomDataAddress + bankAddress + i);

                if (writeAddress >= 0x8000 && writeAddress <= 0xFFFF && readAddress < (Core.rom.getExactDataLength() - 16)) //16 to skip the INES header.
                {
                    Core.cpu.directCPURamWrite(writeAddress, Core.rom.readByte(16 + readAddress));
                }
            }
        }
    }
}
