using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes.Mappers
{
    class MMC1
    {
        public static byte writeRegisterShift = 0;
        public static byte writeRegisterValue = 0;

        public static byte controlRegisterValue = 0;
        public static byte ramPage1RegisterValue = 0;
        public static byte ramPage2RegisterValue = 0;
        public static byte romPageRegisterValue = 0;

        public static byte prgRomBankSwitchingMode = 3;
        public static bool chrRom8kbBankSwitchingMode = false;

        public static void loadRom(Rom romFile)
        {
            if (romFile.getMapperSetting() == 1)
            {
                loadPrgRomBank(ref romFile, 0x8000, 0);
                loadPrgRomBank(ref romFile, 0xC000, (byte)(romFile.getProgramRomSize() - 1));
                loadChrRomBank(ref romFile, 0x0000, 0);
                loadChrRomBank(ref romFile, 0x1000, 1);

                Core.ppu.isNametableMirrored = false;
            }
        }

        public static bool isMapperWriteAddress(ushort address)
        {
            return address >= 0x8000 && address <= 0xFFFF;
        }

        public static void writeMMC1(ushort address, byte value)
        {
            
            bool writeToMMC1ControlRegister     = address >= 0x8000 && address <= 0x9FFF;
            bool writeToMMC1RamPage1Register    = address >= 0xA000 && address <= 0xBFFF;
            bool writeToMMC1RamPage2Register    = address >= 0xC000 && address <= 0xDFFF;
            bool writeToMMC1RomPageRegister     = address >= 0xE000 && address <= 0xFFFF;

            if(address >= 0x8000 && address <= 0xFFFF)
            {
                if ((value & 0x80) == 0x80)
                {
                    //Reset Shift if bit 7 is set
                    writeRegisterShift = 0;
                    writeRegisterValue = 0;
                    controlRegisterValue |= 0x0C;

                    //0x0C write locks 0xC000 to last bank of Prg Rom
                    loadPrgRomBank(ref Core.rom, 0xC000, (byte)(Core.rom.getProgramRamSize() - 1));
                }
                else
                {
                    if (writeRegisterShift < 5)
                    {
                        writeRegisterValue |= (byte)((value & 0x01) << writeRegisterShift);
                        writeRegisterShift++;
                    }

                    if (writeRegisterShift == 5)
                    {
                        if (writeToMMC1ControlRegister)
                        {
                            controlRegisterValue = writeRegisterValue;

                            byte nameTableSetting = (byte)(controlRegisterValue & 0b11);

                            if (nameTableSetting == 0)
                            {
                                Core.ppu.isNametableMirrored = false;
                                //TODO: Implement one-screen lower bank
                            }
                            else if (nameTableSetting == 1)
                            {
                                Core.ppu.isNametableMirrored = false;
                                //TODO: Implement one-screen upper bank
                            }
                            else if (nameTableSetting == 2)
                            {
                                //Set to Vertical Name Table Mirroring Mode
                                Core.ppu.isNametableMirrored = true;
                                Core.ppu.isVertNametableMirror = true;
                                Core.ppu.isHorizNametableMirror = false;
                            }
                            else if (nameTableSetting == 3)
                            {
                                //Set to Horizontal Name Table Mirroring Mode
                                Core.ppu.isNametableMirrored = true;
                                Core.ppu.isVertNametableMirror = false;
                                Core.ppu.isHorizNametableMirror = true;

                            }

                            //Set prgRom Banking Mode
                            prgRomBankSwitchingMode = (byte)((controlRegisterValue & 0b1100) >> 2);

                            //Set chrRom Banking Mode
                            chrRom8kbBankSwitchingMode = (controlRegisterValue & 0b10000) == 0;

                        }
                        else if (writeToMMC1RamPage1Register)
                        {
                            //Initiate CHR Rom Bank Swap
                            ramPage1RegisterValue = writeRegisterValue;

                            if (chrRom8kbBankSwitchingMode)
                            {
                                //Even number for 8kb banks
                                ramPage1RegisterValue &= 0b000011110;
                            }
                            else
                            {
                                //Full number to select 4kb banks
                                ramPage1RegisterValue &= 0b000011111;
                            }

                            int numBanks = (Core.rom.getCHRRomSize()) * ((chrRom8kbBankSwitchingMode) ? 1 : 2);
                            if (numBanks > 0 && ramPage1RegisterValue <= numBanks)
                            {
                                loadChrRomBank(ref Core.rom, 0x0000, ramPage1RegisterValue);
                            }
                        }
                        else if (writeToMMC1RamPage2Register)
                        {
                            //Initiate CHR Rom Bank Swap
                            ramPage2RegisterValue = writeRegisterValue;
                            if (!chrRom8kbBankSwitchingMode)
                            {
                                int numBanks = (Core.rom.getCHRRomSize()) * ((chrRom8kbBankSwitchingMode) ? 1 : 2);
                                if (numBanks > 0 && ramPage2RegisterValue <= numBanks)
                                {
                                    //Ignore swap request if in 8kb swap mode
                                    if (!chrRom8kbBankSwitchingMode)
                                    {
                                        loadChrRomBank(ref Core.rom, 0x1000, ramPage2RegisterValue);
                                    }
                                }
                            }
                        }
                        else if (writeToMMC1RomPageRegister)
                        {
                            //Initiate PRG Rom Bank Swap
                            romPageRegisterValue = writeRegisterValue;

                            bool ramEnabled = (romPageRegisterValue & 0b10000) == 0b10000;

                            //Supposedly we're supposed to ignore bit 0 in 32 kb mode
                            if (prgRomBankSwitchingMode < 2)
                            {
                                romPageRegisterValue &= 0b00001110;
                            }
                            else
                            {
                                ramPage1RegisterValue &= 0b00001111;
                            }

                            if (prgRomBankSwitchingMode < 2)
                            {
                                //32kb Prg Rom Banking
                                loadPrgRomBank(ref Core.rom, 0x8000, romPageRegisterValue);
                            }
                            else if (prgRomBankSwitchingMode == 2)
                            {
                                loadPrgRomBank(ref Core.rom, 0x8000, 0);
                                loadPrgRomBank(ref Core.rom, 0xC000, romPageRegisterValue);
                            }
                            else if (prgRomBankSwitchingMode == 3)
                            {
                                loadPrgRomBank(ref Core.rom, 0x8000, romPageRegisterValue);
                                loadPrgRomBank(ref Core.rom, 0xC000, (byte)(Core.rom.getProgramRamSize() - 1));
                            }
                        }

                        //Reset shift register
                        writeRegisterShift = 0;
                        writeRegisterValue = 0;
                    }
                }
            }
        }

        public static void loadChrRomBank(ref Rom romFile, ushort address, byte bankNumber)
        {
            //0x0000-0x0FFF Chr Rom Data Bank 1
            //0x1000-0x1FFF Chr Rom Data Bank 2

            int bankSize = chrRom8kbBankSwitchingMode ? 0x2000 : 0x1000;
            uint bankAddress = (uint)(0x1000 * bankNumber);
            uint prgRomDataAddress = (uint)(0x4000 * romFile.getProgramRomSize()); //Skip trainer if it exists

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
            //0x8000-0xBFFF, starts with first rom bank
            //0xC000-0xFFFF, starts with last rom bank

            int bankSize = (prgRomBankSwitchingMode < 2) ? 0x8000 : 0x4000;
            uint bankAddress = (uint)(0x4000 * (bankNumber));
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

        public static void resetMMC1()
        {
            writeRegisterShift = 0;
            writeRegisterValue = 0;

            controlRegisterValue = 0;
            ramPage1RegisterValue = 0;
            ramPage2RegisterValue = 0;
            romPageRegisterValue = 0;

            prgRomBankSwitchingMode = 3;
            chrRom8kbBankSwitchingMode = false;
        }
    }
}
