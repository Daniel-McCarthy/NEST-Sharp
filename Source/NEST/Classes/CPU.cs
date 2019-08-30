using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes
{
    class CPU
    {
        //NEST CPU Registers
        private byte accumulator = 0;                                               //Contains results of arithmetic functions
        private byte xAddress = 0;                                                  //X Index Value
        private byte yAddress = 0;                                                  //Y Index Value
        public ushort programCounter = 0x8000;                                      //Tracks position in the program
        private byte stackPointer = 0xFD;                                           //Tracks position in the stack
        private byte statusRegister = 0 | Empty_Flag | Interrupt_Disable_Flag;      //Tracks what flags are set in the CPU

        private const byte Carry_Flag               = 0x1;          //C
        private const byte Zero_Flag                = 0x1 << 1;     //Z
        private const byte Interrupt_Disable_Flag   = 0x1 << 2;     //I
        private const byte Decimal_Mode_Flag        = 0x1 << 3;     //D
        private const byte Breakpoint_Flag          = 0x1 << 4;     //B
        private const byte Empty_Flag               = 0x1 << 5;     //-
        private const byte Overflow_Flag            = 0x1 << 6;     //V
        private const byte Negative_Flag            = 0x1 << 7;     //S

        private const ushort PPU_CONTROL_REGISTER       = 0x2000;
        private const ushort PPU_MASK_REGISTER          = 0x2001;
        private const ushort PPU_STATUS_REGISTER        = 0x2002;
        private const ushort OAM_DATA_ADDRESS_REGISTER  = 0x2003;
        private const ushort OAM_DATA_REGISTER          = 0x2004;
        private const ushort PPU_SCROLL_REGISTER        = 0x2005;
        private const ushort PPU_DATA_ADDRESS_REGISTER  = 0x2006;
        private const ushort PPU_DATA_REGISTER          = 0x2007;
        private const ushort OAM_DMA_REGISTER           = 0x4014;
        private const ushort JOYPAD1_REGISTER           = 0x4016;
        private const ushort JOYPAD2_REGISTER           = 0x4017;


        //CPU Ram Map:
        //0x0000-0x07FF: Internal Ram
        //0x0800-0x1FFF: Mirroring of Internal Ram
        //0x2000-0x2007: PPU Registers
        //0x2008-0x3FFF: Mirroring of PPU Registers
        //0x4000-0x4017: APU & I/O Registers
        //0x4018-0x401F: APU & I/O Functionality
        //0x4020-0xFFFF: Cartridge Space
        private byte[] cpuRam = new byte[0x10000];

        public bool pendingInterrupt = false;

        public uint tClock = 0;
        public uint mClock = 0;

        private byte readImmediateByte()
        {
            return readCPURam(programCounter++);
        }

        private ushort readImmediateUShort()
        {
            return (ushort)(readImmediateByte() | (readImmediateByte() << 8));
        }

        private byte zeroPageIndexed(byte argument, byte index, byte offset = 0)
        {
            //d, x  //d, y
            return readCPURam((ushort)((argument + index + offset) % 256));
        }

        private byte absolute(ushort address, byte offset = 0)
        {
            return readCPURam((ushort)(address + offset));
        }

        private byte absoluteIndexed(byte argument, byte index)
        {
            //a, x //a, y
            return readCPURam((ushort)(argument + index));
        }

        private byte indexedIndirect(byte argument)
        {
            //(d, x)
            ushort addressLower = readCPURam((ushort)((argument + xAddress) % 256));
            ushort addressUpper = (ushort)(readCPURam((ushort)((argument + xAddress + 1) % 256)) << 8);
            return readCPURam((ushort)(addressLower | addressUpper));

            //May need extra logic for if argument is 0xFF, and the next byte would be at 00?
        }

        private void writeIndexedIndirect(byte address, byte data)
        {
            //(d, x)
            ushort addressLower = readCPURam((ushort)((address + xAddress) % 256));
            ushort addressUpper = (ushort)(readCPURam((ushort)((address + xAddress + 1) % 256)) << 8);
            writeCPURam((ushort)(addressLower | addressUpper), data);

            //May need extra logic for if argument is 0xFF, and the next byte would be at 00?
        }


        private byte indirectIndexed(byte argument)
        {
            //(d), y

            ushort a = readCPURam(argument);
            ushort b = (ushort)(zeroPageIndexed(argument, 0, 1) << 8);
            return readCPURam((ushort)((a | b) + yAddress));
        }

        private void writeIndirectIndexed(byte address, byte data)
        {
            ushort a = readCPURam(address);
            ushort b = (ushort)(zeroPageIndexed(address, 0, 1) << 8);
            writeCPURam((ushort)((a | b) + yAddress), data);
        }

        public byte readCPURam(ushort address, bool ignoreCycles = false)
        {
            if(!ignoreCycles)
            {
                mClock += 1;
                tClock += 4;
            }

            if (address == PPU_STATUS_REGISTER)
            {
                return Core.ppu.getPPUStatus();
            }
            else if (address == PPU_DATA_REGISTER)
            {
                ushort ppuAddress = Core.ppu.ppuWriteAddress;

                if(ppuAddress > 0x3FFF)
                {
                    ppuAddress %= 0x3FFF;
                }

                byte value = Core.ppu.readPPURamByte(ppuAddress);
                Core.ppu.ppuWriteAddress += (ushort)(Core.ppu.getPPURegisterVRAMIncrement());
                return value;
            }
            else if (address == JOYPAD1_REGISTER)
            {
                return Core.input.joyPadRegisterRead();
            }
            else
            {
                return cpuRam[address];
            }
        }

        public void writeCPURam(ushort address, byte value, bool ignoreCycles = false)
        {
            if (!ignoreCycles)
            {
                mClock += 1;
                tClock += 4;
            }

            if(Mappers.Mapper.isMapperWriteAddress(address))
            {
                Mappers.Mapper.writeToCurrentMapper(address, value);
            }
            else if (address == OAM_DMA_REGISTER)
            {
                //Initiate DMA tranfer from (XX00 to XXFF) to OAM Ram
                ushort oamAddress = (ushort)(value << 8);
                Core.ppu.oamDMATransfer(oamAddress);
            }
            else if (address == PPU_DATA_REGISTER)
            {
                Core.ppu.writePPURamByte(Core.ppu.ppuWriteAddress, value);
                Core.ppu.ppuWriteAddress += (ushort)(Core.ppu.getPPURegisterVRAMIncrement());
            }
            else if(address == PPU_DATA_ADDRESS_REGISTER)
            {
                //Set byte of PPU Write Address
                if(!Core.ppu.ppuAddressWrittenOnce)
                {
                    value &= 0x3F; //0x3F so that the address can not exceed PPU Ram size
                    Core.ppu.tempPPUWriteAddress &= 0x00FF;
                    Core.ppu.tempPPUWriteAddress |= (ushort)(value << 8);
                    Core.ppu.ppuAddressWrittenOnce = true;
                }
                else
                {
                    Core.ppu.tempPPUWriteAddress &= 0xFF00;
                    Core.ppu.tempPPUWriteAddress |= value;
                    Core.ppu.ppuAddressWrittenOnce = false;
                    Core.ppu.ppuWriteAddress = Core.ppu.tempPPUWriteAddress;
                }
            }
            else if (address == OAM_DATA_REGISTER)
            {
                Core.ppu.writeOAMRamByte(Core.ppu.oamWriteAddress, value);
            }
            else if(address == OAM_DATA_ADDRESS_REGISTER)
            {
                //Set byte of OAM Write Address
                if(!Core.ppu.oamAddressWrittenOnce)
                {
                    Core.ppu.tempOAMWriteAddress &= 0x00FF;
                    Core.ppu.tempOAMWriteAddress |= (ushort)(value << 8);
                    Core.ppu.oamAddressWrittenOnce = true;
                }
                else
                {
                    Core.ppu.tempOAMWriteAddress &= 0xFF00;
                    Core.ppu.tempOAMWriteAddress |= value;
                    Core.ppu.oamWriteAddress = Core.ppu.tempOAMWriteAddress;
                    Core.ppu.oamAddressWrittenOnce = false;
                }
            }
            else if (address == PPU_SCROLL_REGISTER)
            {
                if (!Core.ppu.scrollWrittenOnce)
                {
                    Core.ppu.scrollX = value;
                    Core.ppu.scrollWrittenOnce = true;
                }
                else
                {
                    Core.ppu.scrollY = value;
                    Core.ppu.scrollWrittenOnce = false;
                }
            }
            else if (address == JOYPAD1_REGISTER)
            {
                Core.input.joyPadRegisterWrite(value);
            }
            else
            {
                cpuRam[address] = value;
            }
        }

        public void directCPURamWrite(ushort address, byte value)
        {
            cpuRam[address] = value;
        }

        public void pushStackU8(byte value)
        {
            writeCPURam((ushort)(0x100 | stackPointer), value);
            stackPointer--;
        }

        public void pushStackU16(ushort value)
        {
            pushStackU8((byte)((value >> 8) & 0xFF));
            pushStackU8((byte)(value & 0xFF));
        }

        public byte popStackU8()
        {
            stackPointer++;
            return readCPURam((ushort)(0x100 | stackPointer));
        }

        public ushort popStackU16()
        {
            return (ushort)(popStackU8() | (popStackU8() << 8));
        }

        private bool detectADCOverflow(int value, int addition, int sum)
        {
            return (!(((value ^ addition) & 0x80) > 0)) && (((value ^ sum) & 0x80) > 0);
        }

        private bool detectSBCOverflow(int value, int addition, int sum)
        {
            return ((((value ^ sum) & 0x80) > 0)) && (((value ^ addition) & 0x80) > 0);
        }

        private void opcode00()
        {
            //BRK: Force Interrupt

            readImmediateByte(); // Empty padding byte.
            setFlagTo(Breakpoint_Flag, true);
            pushStackU16(programCounter);
            pushStackU8(statusRegister);

            ushort address = readCPURam(0xFFFE);
            address |= (ushort)(readCPURam(0xFFFF) << 8);

            programCounter = --address;

            //7 Cycles
        }
        
        private void opcode01()
        {
            //Bitwise OR A Indexed Indirect X

            byte value = indexedIndirect(readImmediateByte());
            accumulator = ((byte)(accumulator | value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode04()
        {
            //Unofficial Opcode: NOP with zero page read
            readCPURam(readImmediateByte());

            // 3 cycles total. Read opcode byte, operand byte, and read value from address.
        }

        private void opcode05()
        {
            //Bitwise OR A Zero Page

            accumulator = ((byte)(accumulator | readCPURam(readImmediateByte())));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode06()
        {
            //Bitwise Left Shift of Zero Page Value

            ushort address = readImmediateByte();
            byte value = readCPURam(address);

            setFlagTo(Carry_Flag, (value & 0x80) == 0x80);          //Set carry flag to old bit 7

            value <<= 1;
            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode08()
        {
            //PHP: Pushes value in status onto stack

            pushStackU8((byte)(statusRegister | Empty_Flag | Breakpoint_Flag));

            //3 cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode09()
        {
            //Bitwise OR A Immediate Byte

            accumulator = ((byte)(accumulator | readImmediateByte()));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode10()
        {
            //BPL: Branch if Negative Flag disabled

            if (!getFlagStatus(Negative_Flag))
            {
                sbyte signedByte = (sbyte)readImmediateByte();
                programCounter = (ushort)(programCounter + signedByte);
            }
            else
            {
                // Skip operand byte.
                programCounter++;
            }

            //TODO: Add Cycle if branched to a new page
            //2 cycles. +1 cycle if branch successful. +2 cycles if branched to a new page.

            mClock += 1;
            tClock += 4;
        }

        private void opcode0A()
        {
            //Bitwise Left Shift of Accumulator

            //Set carry flag to old bit 7
            setFlagTo(Carry_Flag, (accumulator & 0x80) == 0x80);
            accumulator <<= 1;

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode0C()
        {
            //Unofficial Opcode: NOP with absolute address read
            readCPURam(readImmediateUShort());

            // 4 cycles total. Read opcode byte, 2 operand bytes, and read value from address.
        }

        private void opcode0D()
        {
            //Bitwise OR A Absolute 16 Bit Address

            accumulator = (byte)(accumulator | absolute(readImmediateUShort()));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode0E()
        {
            //Bitwise Left Shift of value at absolute address

            ushort address = readImmediateUShort();
            byte value = readCPURam(address);

            setFlagTo(Carry_Flag, (value & 0x80) == 0x80);          //Set carry flag to old bit 7

            value <<= 1;
            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode11()
        {
            //Bitwise OR A Indirect Indexed Y

            byte value = indirectIndexed(readCPURam(programCounter++));
            accumulator = ((byte)(accumulator | value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode14()
        {
            //Unofficial Opcode: NOP with zero page + X read
            programCounter++;

            mClock += 3;
            tClock += 12;

            // 4 cycles total. Read opcode byte, operand byte, and read value from address, and index of X address.
        }

        private void opcode15()
        {
            //Bitwise OR A Zero Page X

            accumulator = ((byte)(accumulator | zeroPageIndexed(readImmediateByte(), xAddress)));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode16()
        {
            //Bitwise Left Shift of value at Zero Page X address

            ushort address = (ushort)(readImmediateByte() + xAddress);
            byte value = readCPURam(address);

            setFlagTo(Carry_Flag, (value & 0x80) == 0x80);          //Set carry flag to old bit 7

            value <<= 1;
            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode18()
        {
            //CLC: Set carry flag to disabled

            setFlagTo(Carry_Flag, false);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode19()
        {
            //Bitwise OR A Absolute Y Index 16 Bit Address

            accumulator = (byte)(accumulator | absolute(readImmediateUShort(), yAddress));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode1D()
        {
            //Bitwise OR A Absolute X Index 16 Bit Address

            accumulator = (byte)(accumulator | absolute(readImmediateUShort(), xAddress));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode1E()
        {
            //Bitwise Left Shift of value at absolute X address

            ushort address = readImmediateUShort();
            address += xAddress;
            byte value = readCPURam(address);

            setFlagTo(Carry_Flag, (value & 0x80) == 0x80);          //Set carry flag to old bit 7

            value <<= 1;
            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode20()
        {
            //Jump to subroutine at absolute address

            ushort newPC = readImmediateUShort();
            pushStackU16((ushort)(programCounter - 1));
            programCounter = newPC;

            //6 cycles
            mClock += 2;
            tClock += 8;
        }

        private void opcode21()
        {
            //Bitwise And A with Indexed Indirect X

            byte value = indexedIndirect(readImmediateByte());
            accumulator = ((byte)(accumulator & value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode24()
        {
            //Bitwise Test of Zero Page value with Bit Mask in Accumulator

            ushort address = readImmediateByte();

            //Store bit 7 and 6 in Negative and Overflow flags respectively.
            byte value = readCPURam(address);
            setFlagTo(Negative_Flag, (value & 0x80) != 0);
            setFlagTo(Overflow_Flag, (value & 0x40) != 0);

            value &= accumulator;
            setFlagTo(Zero_Flag, (value == 0));

            // 3 cycles total. Read opcode byte, operand byte, and read value from address.
        }

        private void opcode25()
        {
            //Bitwise And A with Zero Page Immediate Byte

            accumulator = ((byte)(accumulator & readCPURam(readImmediateByte())));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode26()
        {
            //Bitwise Left Rotate of Zero Page Value

            ushort address = readImmediateByte();
            byte value = readCPURam(address);

            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (value & 0x80) == 0x80);          //Set carry flag to old bit 7

            value <<= 1;

            if(oldCarry)
            {
                value |= 0x1;
            }

            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode28()
        {
            //PLP: Pops value from stack into status register

            statusRegister = (byte)(popStackU8() | Empty_Flag);

            //4 cycles

            mClock += 2;
            tClock += 8;
        }

        private void opcode29()
        {
            //Bitwise And A with Immediate Byte

            accumulator = ((byte)(accumulator & readImmediateByte()));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode2A()
        {
            //Bitwise Left Rotate of Accumulator

            //Set carry flag to old bit 7
            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (accumulator & 0x80) == 0x80);
            accumulator <<= 1;
            
            //Set new bit 0 to previous carry flag
            if(oldCarry)
            {
                accumulator |= 0x1;
            }

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode2C()
        {
            //Bitwise Test of value at absolute address with Bit Mask in Accumulator

            ushort address = readImmediateUShort();

            //Store bit 7 and 6 in Negative and Overflow flags respectively.
            byte value = readCPURam(address);
            setFlagTo(Negative_Flag, (value & 0x80) != 0);
            setFlagTo(Overflow_Flag, (value & 0x40) != 0);

            //Mask value with accumulator value and set Zero flag
            value &= accumulator;
            setFlagTo(Zero_Flag, (value == 0));
            

            // 4 cycles total. Read opcode byte, 2 operand bytes, and read value from address.
        }

        private void opcode2D()
        {
            //Bitwise And A with absolute 16 bit Address

            accumulator = ((byte)(accumulator & absolute(readImmediateUShort())));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode2E()
        {
            //Bitwise Left Rotate of value at absolute address

            ushort address = readImmediateUShort();
            byte value = readCPURam(address);

            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (value & 0x80) == 0x80);          //Set carry flag to old bit 7

            value <<= 1;

            if (oldCarry)
            {
                value |= 0x01;
            }

            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode30()
        {
            //BMI: Branch if Negative Flag enabled

            if (getFlagStatus(Negative_Flag))
            {
                sbyte signedByte = (sbyte)readImmediateByte();
                programCounter = (ushort)(programCounter + signedByte);
            }
            else
            {
                // Skip operand byte.
                programCounter++;
            }

            //TODO: Add Cycle if branched to a new page
            //2 cycles. +1 cycle if branch successful. +2 cycles if branched to a new page.

            mClock += 1;
            tClock += 4;
        }

        private void opcode31()
        {
            //Bitwise And A Indirect Indexed Y

            byte value = indirectIndexed(readCPURam(programCounter++));
            accumulator = ((byte)(accumulator & value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode34()
        {
            //Unofficial Opcode: NOP with zero page + X read
            programCounter++;

            mClock += 3;
            tClock += 12;

            // 4 cycles total. Read opcode byte, operand byte, and read value from address, and index of X address.
        }

        private void opcode35()
        {
            //Bitwise And A with Zero Page X

            byte value = zeroPageIndexed(readImmediateByte(), xAddress);
            accumulator = ((byte)(accumulator & value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode36()
        {
            //Bitwise Left Rotate of value at Zero Page X address

            ushort address = (ushort)(readImmediateByte() + xAddress);
            byte value = readCPURam(address);

            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (value & 0x80) == 0x80);          //Set carry flag to old bit 7

            value <<= 1;

            if(oldCarry)
            {
                value |= 0x01;
            }

            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode38()
        {
            //SEC: Set carry flag to enabled

            setFlagTo(Carry_Flag, true);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode39()
        {
            //Bitwise And A with Absolute + Y Address

            byte value = absolute(readImmediateUShort(), yAddress);
            accumulator = ((byte)(accumulator & value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode3D()
        {
            //Bitwise And A with Absolute + X Address

            byte value = absolute(readImmediateUShort(), xAddress);
            accumulator = ((byte)(accumulator & value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode3E()
        {
            //Bitwise Left Rotate of value at absolute X address

            ushort address = (ushort)(readImmediateUShort() + xAddress);
            byte value = readCPURam(address);

            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (value & 0x80) == 0x80);          //Set carry flag to old bit 7

            value <<= 1;

            if(oldCarry)
            {
                value |= 0x01;
            }

            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode40()
        {
            //RTI: Return from interrupt

            statusRegister = popStackU8();
            statusRegister |= Empty_Flag;
            programCounter = (ushort)(popStackU16());

            //6 cycles
            mClock += 2;
            tClock += 8;
        }

        private void opcode41()
        {
            //Bitwise XOR A Indexed Indirect X

            byte value = indexedIndirect(readCPURam(programCounter++));
            accumulator = ((byte)(accumulator ^ value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode44()
        {
            //Unofficial Opcode: NOP with zero page read
            readCPURam(readImmediateByte());

            // 3 cycles total. Read opcode byte, operand byte, and read value from address.
        }

        private void opcode45()
        {
            //Bitwise XOR A with Zero Page address

            accumulator = ((byte)(accumulator ^ zeroPageIndexed(readImmediateByte(),0)));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode46()
        {
            //Bitwise Right Shift of Zero Page Value

            ushort address = readImmediateByte();
            byte value = readCPURam(address);

            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value >>= 1;
            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode48()
        {
            //PHA: Pushes value in accumulator onto stack

            pushStackU8(accumulator);

            //3 cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode49()
        {
            //Bitwise XOR A with Immediate byte

            accumulator = (byte)(accumulator ^ readImmediateByte());

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode4A()
        {
            //Bitwise Right Shift of Accumulator

            //Set carry flag to old bit 7
            setFlagTo(Carry_Flag, (accumulator & 0x01) == 0x01);
            accumulator >>= 1;

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode4C()
        {
            //Jump to absolute address

            programCounter = readImmediateUShort();

            //3 cycles
        }

        private void opcode4E()
        {
            //Bitwise Right Shift of value at absolute address

            ushort address = readImmediateUShort();
            byte value = readCPURam(address);

            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value >>= 1;
            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode4D()
        {
            //Bitwise XOR A with absolute address

            accumulator = (byte)(accumulator ^ readCPURam(readImmediateUShort()));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode50()
        {
            //BVC: Branch if Overflow Flag disabled

            if (!getFlagStatus(Overflow_Flag))
            {
                sbyte signedByte = (sbyte)readImmediateByte();
                programCounter = (ushort)(programCounter + signedByte);
            }
            else
            {
                // Skip operand byte.
                programCounter++;
            }

            //TODO: Add Cycle if branched to a new page
            //2 cycles. +1 cycle if branch successful. +2 cycles if branched to a new page.

            mClock += 1;
            tClock += 4;
        }

        private void opcode51()
        {
            //Bitwise XOR A Indirect Indexed Y

            byte value = indirectIndexed(readCPURam(programCounter++));
            accumulator = ((byte)(accumulator ^ value));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode54()
        {
            //Unofficial Opcode: NOP with zero page + X read
            programCounter++;

            mClock += 3;
            tClock += 12;

            // 4 cycles total. Read opcode byte, operand byte, and read value from address, and index of X address.
        }

        private void opcode55()
        {
            //Bitwise XOR A with Zero Page X address

            accumulator = ((byte)(accumulator ^ zeroPageIndexed(readImmediateByte(), xAddress)));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode56()
        {
            //Bitwise Right Shift of value at Zero Page X address

            ushort address = (ushort)(readImmediateByte() + xAddress);
            byte value = readCPURam(address);

            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value >>= 1;
            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode58()
        {
            //CLI: Set interrupt disable flag to disabled

            setFlagTo(Interrupt_Disable_Flag, false);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode59()
        {
            //Bitwise XOR A with Absolute Y address

            accumulator = (byte)(accumulator ^ absolute(readImmediateUShort(), yAddress));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode5D()
        {
            //Bitwise XOR A with Absolute X address

            accumulator = (byte)(accumulator ^ absolute(readImmediateUShort(), xAddress));

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 1;
            tClock += 4;
        }

        private void opcode5E()
        {
            //Bitwise Right Shift of value at absolute X address

            ushort address = readImmediateUShort();
            address += xAddress;
            byte value = readCPURam(address);

            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value >>= 1;
            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode60()
        {
            //Return from subroutine

            programCounter = (ushort)(popStackU16() + 1);

            //6 cycles
            mClock += 2;
            tClock += 8;
        }

        private void opcode61()
        {
            //ADC: Add Byte at Indexed Indirect address + Carry Flag and copy it to Accumulator

            int originalValue = accumulator;
            int additionByte = indexedIndirect(readImmediateByte());
            int carryAmount = getFlagStatus(Carry_Flag) ? 1 : 0;
            int sum = originalValue + additionByte + carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectADCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, sum > 0xFF);
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcode64()
        {
            //Unofficial Opcode: NOP with zero page read
            readCPURam(readImmediateByte());

            // 3 cycles total. Read opcode byte, operand byte, and read value from address.
        }

        private void opcode65()
        {
            //ADC: Add Zero Page Byte + Carry Flag and copy it to Accumulator

            int originalValue = accumulator;
            int additionByte = readCPURam(readImmediateByte());
            int carryAmount = getFlagStatus(Carry_Flag) ? 1 : 0;
            int sum = originalValue + additionByte + carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectADCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, sum > 0xFF);
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcode66()
        {
            //Bitwise Right Rotate of Zero Page Value

            ushort address = readImmediateByte();
            byte value = readCPURam(address);

            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value >>= 1;

            if (oldCarry)
            {
                value |= 0x80;
            }

            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode68()
        {
            //PLA: Pops value from stack into accumulator

            accumulator = popStackU8();

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            //4 cycles

            mClock += 2;
            tClock += 8;
        }

        private void opcode69()
        {
            //ADC: Add Immediate Byte + Carry Flag and copy it to Accumulator

            int originalValue = accumulator;
            int additionByte = readImmediateByte();
            int carryAmount = getFlagStatus(Carry_Flag) ? 1 : 0;
            int sum = originalValue + additionByte + carryAmount;   
            
            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectADCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, sum > 0xFF);
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcode6A()
        {
            //Bitwise Right Rotate of Accumulator

            //Set carry flag to old bit 0
            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (accumulator & 0x01) == 0x01);
            accumulator >>= 1;

            //Set new bit 7 to previous carry flag
            if (oldCarry)
            {
                accumulator |= 0x80;
            }

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode6C()
        {
            //Jump to indirect address

            ushort addressLocation = readImmediateUShort();
            ushort address = 0;
            bool jumpBug = (addressLocation & 0xFF) == 0xFF;

            //6502 Bug:
            if (jumpBug)
            {
                address |= readCPURam(addressLocation);
                address |= (ushort)(readCPURam((ushort)(addressLocation & 0xFF00)) << 8);
            }
            else
            {
                address |= readCPURam(addressLocation);
                address |= (ushort)(readCPURam((ushort)(addressLocation + 1)) << 8);
            }

            programCounter = address;

            //3 cycles
        }

        private void opcode6D()
        {
            //ADC: Add Byte at absolute address + Carry Flag and copy it to Accumulator

            int originalValue = accumulator;
            int additionByte = readCPURam(readImmediateUShort());
            int carryAmount = getFlagStatus(Carry_Flag) ? 1 : 0;
            int sum = originalValue + additionByte + carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectADCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, sum > 0xFF);
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcode6E()
        {
            //Bitwise Right Rotate of value at absolute address

            ushort address = readImmediateUShort();
            byte value = readCPURam(address);

            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value >>= 1;

            if(oldCarry)
            {
                value |= 0x80;
            }

            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode70()
        {
            //BVS: Branch if Overflow Flag enabled

            if (getFlagStatus(Overflow_Flag))
            {
                sbyte signedByte = (sbyte)readImmediateByte();
                programCounter = (ushort)(programCounter + signedByte);
            }
            else
            {
                // Skip operand byte.
                programCounter++;
            }

            //TODO: Add Cycle if branched to a new page
            //2 cycles. +1 cycle if branch successful. +2 cycles if branched to a new page.

            mClock += 1;
            tClock += 4;
        }

        private void opcode71()
        {
            //ADC: Add Byte at Indirect Indexed address + Carry Flag and copy it to Accumulator

            int originalValue = accumulator;
            int additionByte = indirectIndexed(readImmediateByte());
            int carryAmount = getFlagStatus(Carry_Flag) ? 1 : 0;
            int sum = originalValue + additionByte + carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectADCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, sum > 0xFF);
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcode74()
        {
            //Unofficial Opcode: NOP with zero page + X read
            programCounter++;

            mClock += 3;
            tClock += 12;

            // 4 cycles total. Read opcode byte, operand byte, and read value from address, and index of X address.
        }

        private void opcode75()
        {
            //ADC: Add Zero Page + X Byte + Carry Flag and copy it to Accumulator

            int originalValue = accumulator;
            int additionByte = zeroPageIndexed(readImmediateByte(), xAddress);
            int carryAmount = getFlagStatus(Carry_Flag) ? 1 : 0;
            int sum = originalValue + additionByte + carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectADCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, sum > 0xFF);
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcode76()
        {
            //Bitwise Right Rotate of value at Zero Page X address

            ushort address = (ushort)(readImmediateByte() + xAddress);
            byte value = readCPURam(address);

            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value >>= 1;

            if (oldCarry)
            {
                value |= 0x80;
            }

            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode78()
        {
            //SEI: Set interrupt disable flag to enabled

            setFlagTo(Interrupt_Disable_Flag, true);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode79()
        {
            //ADC: Add Byte at absolute + Y address + Carry Flag and copy it to Accumulator

            int originalValue = accumulator;
            ushort address = readImmediateUShort();
            address += yAddress;
            int additionByte = readCPURam(address);
            int carryAmount = getFlagStatus(Carry_Flag) ? 1 : 0;
            int sum = originalValue + additionByte + carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectADCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, sum > 0xFF);
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcode7D()
        {
            //ADC: Add Byte at absolute + X address + Carry Flag and copy it to Accumulator

            int originalValue = accumulator;
            ushort address = readImmediateUShort();
            address += xAddress;
            int additionByte = readCPURam(address);
            int carryAmount = getFlagStatus(Carry_Flag) ? 1 : 0;
            int sum = originalValue + additionByte + carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectADCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, sum > 0xFF);
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcode7E()
        {
            //Bitwise Right Rotate of value at absolute X address

            ushort address = (ushort)(readImmediateUShort() + xAddress);
            byte value = readCPURam(address);

            bool oldCarry = getFlagStatus(Carry_Flag);
            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value >>= 1;

            if (oldCarry)
            {
                value |= 0x80;
            }

            writeCPURam(address, (byte)(value));

            setFlagTo(Zero_Flag, (value == 0));
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            mClock += 2;
            tClock += 8;
        }

        private void opcode80()
        {
            //Unofficial Opcode: NOP with immediate read
            programCounter++;

            mClock += 1;
            tClock += 4;

            // 2 cycles total. Read opcode byte, and operand byte.
        }

        private void opcode81()
        {
            //STA: Copy value in accumulator to Indexed Indirect Address

            writeIndexedIndirect(readImmediateByte(), accumulator);

            //6 cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode82()
        {
            //Unofficial Opcode: NOP with immediate read
            programCounter++;

            mClock += 1;
            tClock += 4;

            // 2 cycles total. Read opcode byte, and operand byte.
        }

        private void opcode84()
        {
            //STY: Copy value in y register to zero page addres

            writeCPURam(readImmediateByte(), yAddress);

            //3 Cycles
        }

        private void opcode85()
        {
            //STA: Copy value in accumulator to Zero Page Address

            writeCPURam(readImmediateByte(), accumulator);

            //3 cycles
        }

        private void opcode86()
        {
            //STX: Copy value in x register to zero page addres

            writeCPURam(readImmediateByte(), xAddress);

            //3 Cycles
        }

        private void opcode88()
        {
            //Decrement Y Register

            --yAddress;

            setFlagTo(Zero_Flag, yAddress == 0);
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);

            //2 Cycles
            mClock += 1;
            tClock += 4;

        }

        private void opcode89()
        {
            //Unofficial Opcode: NOP with immediate read
            programCounter++;

            mClock += 1;
            tClock += 4;

            // 2 cycles total. Read opcode byte, and operand byte.
        }

        private void opcode8A()
        {
            //TXA: Copy value in x register to accumulator

            accumulator = xAddress;

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode8C()
        {
            //STY: Copy value in y register to absolute address

            writeCPURam(readImmediateUShort(), yAddress);

            //4 Cycles
        }

        private void opcode8D()
        {
            //STA: Copy value in accumulator to Absolute Address

            writeCPURam(readImmediateUShort(), accumulator);

            //4 cycles
        }

        private void opcode8E()
        {
            //STX: Copy value in x register to absolute address

            writeCPURam(readImmediateUShort(), xAddress);

            //4 Cycles
        }

        private void opcode90()
        {
            //BCC: Branch if Carry Flag disabled

            if(!getFlagStatus(Carry_Flag))
            {
                sbyte signedByte = (sbyte)readImmediateByte();
                programCounter = (ushort)(programCounter + signedByte);
            }
            else
            {
                // Skip operand byte.
                programCounter++;
            }

            //TODO: Add Cycle if branched to a new page
            //2 cycles. +1 cycle if branch successful. +2 cycles if branched to a new page.

            mClock += 1;
            tClock += 4;
        }

        private void opcode91()
        {
            //STA: Copy value in accumulator to Indirect Indexed Address

            writeIndirectIndexed(readImmediateByte(), accumulator);

            //6 cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode94()
        {
            //STY: Copy value in y register to zero page + x address

            byte address = readImmediateByte();
            address += xAddress;
            writeCPURam(address, yAddress);

            //4 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode95()
        {
            //STA: Copy value in accumulator to Zero Page + X Address

            ushort address = readImmediateByte();
            address += xAddress;
            writeCPURam(address, accumulator);

            //4 cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode96()
        {
            //STX: Copy value in x register to zero page + y address

            byte address = readImmediateByte();
            address += yAddress;
            writeCPURam(address, xAddress);

            //4 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode98()
        {
            //TYA: Copy value in y register to accumulator

            accumulator = yAddress;

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode99()
        {
            //STA: Copy value in accumulator to Absolute + Y Address

            ushort address = readImmediateUShort();
            address += yAddress;
            writeCPURam(address, accumulator);

            //5 cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode9A()
        {
            //TXS: Copy value in x register to stack pointer

            stackPointer = xAddress;

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcode9D()
        {
            //STA: Copy value in accumulator to Absolute + X Address

            ushort address = readImmediateUShort();
            address += xAddress;
            writeCPURam(address, accumulator);

            //5 cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcodeA0()
        {
            //Load immediate byte into Y Register

            yAddress = readImmediateByte();

            setFlagTo(Zero_Flag, (yAddress == 0));
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);
        }

        private void opcodeA1()
        {
            //Load value at indexedIndirect address into accumulator

            accumulator = indexedIndirect(readImmediateByte());

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

        }

        private void opcodeA2()
        {
            //Load immediate byte into X Register

            xAddress = readImmediateByte();

            setFlagTo(Zero_Flag, (xAddress == 0));
            setFlagTo(Negative_Flag, (xAddress & 0x80) != 0);
        }

        private void opcodeA4()
        {
            //Load zero page value into Y Register

            yAddress = readCPURam(readImmediateByte());

            setFlagTo(Zero_Flag, (yAddress == 0));
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);
        }

        private void opcodeA5()
        {
            //Load zero page value into accumulator

            accumulator = readCPURam(readImmediateByte());

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);
        }

        private void opcodeA6()
        {
            //Load zero page value into X Register

            xAddress = readCPURam(readImmediateByte());

            setFlagTo(Zero_Flag, (xAddress == 0));
            setFlagTo(Negative_Flag, (xAddress & 0x80) != 0);
        }

        private void opcodeA8()
        {
            //TAY: Copy value in accumulator to Y Register

            yAddress = accumulator;

            setFlagTo(Zero_Flag, (yAddress == 0));
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcodeA9()
        {
            //Load immediate byte into accumulator

            accumulator = readImmediateByte();

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);
        }

        private void opcodeAA()
        {
            //TAX: Copy value in accumulator to X Register

            xAddress = accumulator;

            setFlagTo(Zero_Flag, (xAddress == 0));
            setFlagTo(Negative_Flag, (xAddress & 0x80) != 0);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcodeAC()
        {
            //Load value at absolute address into Y Register

            yAddress = absolute(readImmediateUShort());

            setFlagTo(Zero_Flag, (yAddress == 0));
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);
        }

        private void opcodeAD()
        {
            //Load value at absolute address into accumulator

            accumulator = absolute(readImmediateUShort());

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);
        }

        private void opcodeAE()
        {
            //Load value at absolute address into X Register

            xAddress = absolute(readImmediateUShort());

            setFlagTo(Zero_Flag, (xAddress == 0));
            setFlagTo(Negative_Flag, (xAddress & 0x80) != 0);
        }

        private void opcodeB0()
        {
            //BCS: Branch if Carry Flag enabled

            if (getFlagStatus(Carry_Flag))
            {
                sbyte signedByte = (sbyte)readImmediateByte();
                programCounter = (ushort)(programCounter + signedByte);
            }
            else
            {
                // Skip operand byte.
                programCounter++;
            }

            //TODO: Add Cycle if branched to a new page
            //2 cycles. +1 cycle if branch successful. +2 cycles if branched to a new page.

            mClock += 1;
            tClock += 4;
        }

        private void opcodeB1()
        {
            //Load value at indirectIndexed address into accumulator

            accumulator = indirectIndexed(readImmediateByte());

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

        }

        private void opcodeB4()
        {
            //Load zero page + x value into Y Register

            yAddress = zeroPageIndexed(readImmediateByte(), xAddress);

            setFlagTo(Zero_Flag, (yAddress == 0));
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);

            //Add cycle to account for retrieving X address
            mClock += 1;
            tClock += 4;
        }

        private void opcodeB5()
        {
            //Load zero page + x value into accumulator

            accumulator = zeroPageIndexed(readImmediateByte(), xAddress);

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            //Add cycle to account for retrieving X address
            mClock += 1;
            tClock += 4;
        }

        private void opcodeB6()
        {
            //Load zero page + y value into X Register

            xAddress = zeroPageIndexed(readImmediateByte(), yAddress);

            setFlagTo(Zero_Flag, (xAddress == 0));
            setFlagTo(Negative_Flag, (xAddress & 0x80) != 0);

            //Add cycle to account for retrieving X address
            mClock += 1;
            tClock += 4;
        }

        private void opcodeB8()
        {
            //CLV: Set overflow flag to disabled

            setFlagTo(Overflow_Flag, false);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcodeB9()
        {
            //Load value at absolute + Y address into accumulator

            accumulator = absolute(readImmediateUShort(), yAddress);

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            //Add cycle to account for retrieving y address
            mClock += 1;
            tClock += 4;
        }

        private void opcodeBA()
        {
            //TSX: Copy value in stack pointer to X Register

            xAddress = stackPointer;

            setFlagTo(Zero_Flag, (xAddress == 0));
            setFlagTo(Negative_Flag, (xAddress & 0x80) != 0);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcodeBC()
        {
            //Load value at absolute + X address into Y Register

            yAddress = absolute(readImmediateUShort(), xAddress);

            setFlagTo(Zero_Flag, (yAddress == 0));
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);

            //Add cycle to account for retrieving X address
            mClock += 1;
            tClock += 4;
        }

        private void opcodeBD()
        {
            //Load value at absolute + X address into accumulator

            accumulator = absolute(readImmediateUShort(), xAddress);

            setFlagTo(Zero_Flag, (accumulator == 0));
            setFlagTo(Negative_Flag, (accumulator & 0x80) != 0);

            //Add cycle to account for retrieving X address
            mClock += 1;
            tClock += 4;
        }

        private void opcodeBE()
        {
            //Load value at absolute + Y address into X Register

            xAddress = absolute(readImmediateUShort(), yAddress);

            setFlagTo(Zero_Flag, (xAddress == 0));
            setFlagTo(Negative_Flag, (xAddress & 0x80) != 0);

            //Add cycle to account for retrieving y address
            mClock += 1;
            tClock += 4;
        }

        private void opcodeC0()
        {
            //CPY: Compare value of Y register with Immediate byte

            int value = yAddress;
            value -= readImmediateByte();

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //2 Cycles. 1 cycle reading opcode byte and 1 cycle reading opcode argument byte.
        }

        private void opcodeC1()
        {
            //Compare value of accumulator with value at indexedIndirect address

            int value = accumulator;
            value -= indexedIndirect(readImmediateByte());

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //6 Cycles. 1 cycle for opcode byte. 1 cycles for immediate byte. 1 cycle for getting xAddress. 3 for indexed Indirect addressing.
        }

        private void opcodeC2()
        {
            //Unofficial Opcode: NOP with immediate read
            programCounter++;

            mClock += 1;
            tClock += 4;

            // 2 cycles total. Read opcode byte, and operand byte.
        }

        private void opcodeC4()
        {
            //CPY: Compare value of Y Register with value at Zero Page Address

            int value = yAddress;
            value -= readCPURam(readImmediateByte());

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //3 Cycles. 1 cycle reading opcode byte and 1 cycle reading opcode argument address byte, 1 cycle reading value from Zero Page.
        }

        private void opcodeC5()
        {
            //Compare value of accumulator with value at Zero Page Address

            int value = accumulator;
            value -= readCPURam(readImmediateByte());

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //3 Cycles. 1 cycle reading opcode byte and 1 cycle reading opcode argument address byte, 1 cycle reading value from Zero Page.
        }

        private void opcodeC6()
        {
            //Decrement value at Zero Page address

            byte address = readImmediateByte();
            byte value = readCPURam(address);
            --value;

            writeCPURam(address, value);

            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            //5 Cycles
            mClock += 1;
            tClock += 4;

        }

        private void opcodeC8()
        {
            //Increment Y address value

            ++yAddress;

            setFlagTo(Zero_Flag, yAddress == 0);
            setFlagTo(Negative_Flag, (yAddress & 0x80) == 0x80);

            mClock += 1;
            tClock += 4;

            //2 cycles
        }

        private void opcodeC9()
        {
            //Compare value of accumulator with Immediate byte

            int value = accumulator;
            value -= readImmediateByte();

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //2 Cycles. 1 cycle reading opcode byte and 1 cycle reading opcode argument byte.
        }

        private void opcodeCA()
        {
            //Decrement X Register

            --xAddress;

            setFlagTo(Zero_Flag, xAddress == 0);
            setFlagTo(Negative_Flag, (xAddress & 0x80) != 0);

            //2 Cycles
            mClock += 1;
            tClock += 4;

        }

        private void opcodeCC()
        {
            //CPY: Compare value of Y Register with value at absolute Address

            int value = yAddress;
            value -= absolute(readImmediateUShort());

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //4 Cycles. 1 cycle for opcode byte. 2 cycles for immediate ushort. 1 cycle for reading from Zero Page.
        }

        private void opcodeCD()
        {
            //Compare value of accumulator with value at absolute Address

            int value = accumulator;
            value -= absolute(readImmediateUShort());

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //4 Cycles. 1 cycle for opcode byte. 2 cycles for immediate ushort. 1 cycle for reading from Zero Page.
        }

        private void opcodeCE()
        {
            //Decrement value at absolute address

            ushort address = readImmediateUShort();
            byte value = readCPURam(address);
            --value;

            writeCPURam(address, value);

            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            //6 Cycles
            mClock += 1;
            tClock += 4;

        }

        private void opcodeD0()
        {
            //BNE: Branch if Zero Flag disabled

            if (!getFlagStatus(Zero_Flag))
            {
                sbyte signedByte = (sbyte)readImmediateByte();
                programCounter = (ushort)(programCounter + signedByte);
            }
            else
            {
                // Skip operand byte.
                programCounter++;
            }

            //TODO: Add Cycle if branched to a new page
            //2 cycles. +1 cycle if branch successful. +2 cycles if branched to a new page.

            mClock += 1;
            tClock += 4;
        }

        private void opcodeD1()
        {
            //Compare value of accumulator with value at indirectIndexed address

            int value = accumulator;
            value -= indirectIndexed(readImmediateByte());

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //6 Cycles. 1 cycle for opcode byte. 1 cycles for immediate byte. 1 cycle for getting xAddress. 3 for indirect Indexed addressing.
        }

        private void opcodeD4()
        {
            //Unofficial Opcode: NOP with zero page + X read
            programCounter++;

            mClock += 3;
            tClock += 12;

            // 4 cycles total. Read opcode byte, operand byte, and read value from address, and index of X address.
        }

        private void opcodeD5()
        {
            //Compare value of accumulator with value at Zero Page X Address

            int value = accumulator;
            value -= zeroPageIndexed(readImmediateByte(), xAddress);

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //4 Cycles. 1 cycle for opcode byte. 1 cycle for immediate byte. 1 cycle for getting xAddress. 1 cycle for reading from Zero Page.
        }

        private void opcodeD6()
        {
            //Decrement value at Zero Page + X address

            byte address = readImmediateByte();
            address += xAddress;
            byte value = readCPURam(address);
            --value;

            writeCPURam(address, value);

            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            //6 Cycles
            mClock += 2;
            tClock += 8;

        }

        private void opcodeD8()
        {
            //CLD: Set decimal flag to disabled

            setFlagTo(Decimal_Mode_Flag, false);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcodeD9()
        {
            //Compare value of accumulator with value at absolute + Y Address

            int value = accumulator;
            value -= absolute(readImmediateUShort(), yAddress);

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //5 Cycles. 1 cycle for opcode byte. 2 cycles for immediate ushort. 1 cycle for getting yAddress. 1 cycle for reading from Zero Page.
        }

        private void opcodeDD()
        {
            //Compare value of accumulator with value at absolute + X Address

            int value = accumulator;
            value -= absolute(readImmediateUShort(), xAddress);

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //5 Cycles. 1 cycle for opcode byte. 2 cycles for immediate ushort. 1 cycle for getting xAddress. 1 cycle for reading from Zero Page.
        }

        private void opcodeDE()
        {
            //Decrement value at absolute + X address

            ushort address = readImmediateUShort();
            address += xAddress;
            byte value = readCPURam(address);
            --value;

            writeCPURam(address, value);

            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) != 0);

            //7 Cycles
            mClock += 2;
            tClock += 8;

        }

        private void opcodeE0()
        {
            //CPX: Compare value of X register with Immediate byte

            int value = xAddress;
            value -= readImmediateByte();

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //2 Cycles. 1 cycle reading opcode byte and 1 cycle reading opcode argument byte.
        }

        private void opcodeE1()
        {
            //SBC: Subtract Byte at Indexed Indirect address and Carry Flag value from Accumulator

            int originalValue = accumulator;
            int additionByte = indexedIndirect(readImmediateByte());
            int carryAmount = getFlagStatus(Carry_Flag) ? 0 : 1;
            int sum = originalValue - additionByte - carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectSBCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, (originalValue >= (additionByte + carryAmount)));
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcodeE2()
        {
            //Unofficial Opcode: NOP with immediate read
            programCounter++;

            mClock += 1;
            tClock += 4;

            // 2 cycles total. Read opcode byte, and operand byte.
        }

        private void opcodeE4()
        {
            //CPX: Compare value of X Register with value at Zero Page Address

            int value = xAddress;
            value -= readCPURam(readImmediateByte());

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //3 Cycles. 1 cycle reading opcode byte and 1 cycle reading opcode argument address byte, 1 cycle reading value from Zero Page.
        }

        private void opcodeE5()
        {
            //SBC: Subtract Zero Page Byte + Carry Flag value from Accumulator

            int originalValue = accumulator;
            int additionByte = readCPURam(readImmediateByte());
            int carryAmount = getFlagStatus(Carry_Flag) ? 0 : 1;
            int sum = originalValue - additionByte - carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectSBCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, (originalValue >= (additionByte + carryAmount)));
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcodeE6()
        {
            //Increment data at Zero page address

            byte address = readImmediateByte();
            byte value = readCPURam(address);
            ++value;

            writeCPURam(address, value);

            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            mClock += 2;
            tClock += 8;

            //5 cycles
        }

        private void opcodeEC()
        {
            //CPX: Compare value of X Register with value at absolute Address

            int value = xAddress;
            value -= absolute(readImmediateUShort());

            setFlagTo(Carry_Flag, value >= 0);
            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            //4 Cycles. 1 cycle for opcode byte. 2 cycles for immediate ushort. 1 cycle for reading from Zero Page.
        }

        private void opcodeED()
        {
            //SBC: Subtract Byte at absolute address and Carry Flag value from Accumulator

            int originalValue = accumulator;
            int additionByte = readCPURam(readImmediateUShort());
            int carryAmount = getFlagStatus(Carry_Flag) ? 0 : 1;
            int sum = originalValue - additionByte - carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectSBCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, (originalValue >= (additionByte + carryAmount)));
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcodeE8()
        {
            //Increment data at Zero page address

            ++xAddress;

            setFlagTo(Zero_Flag, xAddress == 0);
            setFlagTo(Negative_Flag, (xAddress & 0x80) == 0x80);

            mClock += 1;
            tClock += 4;

            //2 cycles
        }

        private void opcodeE9()
        {
            //SBC: Subtract Immedate byte and Carry Flag value from A

            int originalValue = accumulator;
            int additionByte = readImmediateByte();
            int carryAmount = getFlagStatus(Carry_Flag) ? 0 : 1;
            int sum = originalValue - additionByte - carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectSBCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, (originalValue >= (additionByte + carryAmount)));
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcodeEA()
        {
            //NOP

            mClock += 1;
            tClock += 4;

            //2 cycles
        }

        private void opcodeEE()
        {
            //Increment data at absolute address

            ushort address = readImmediateUShort();
            byte value = readCPURam(address);
            ++value;

            writeCPURam(address, value);

            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            mClock += 2;
            tClock += 8;

            //6 cycles
        }

        private void opcodeF0()
        {
            //BEQ: Branch if Zero Flag enabled

            if (getFlagStatus(Zero_Flag))
            {
                sbyte signedByte = (sbyte)readImmediateByte();
                programCounter = (ushort)(programCounter + signedByte);
            }
            else
            {
                // Skip operand byte.
                programCounter++;
            }

            //TODO: Add Cycle if branched to a new page
            //2 cycles. +1 cycle if branch successful. +2 cycles if branched to a new page.

            mClock += 1;
            tClock += 4;
        }

        private void opcodeF1()
        {
            //SBC: Subtract Byte at Indirect Indexed address and Carry Flag value from Accumulator

            int originalValue = accumulator;
            int additionByte = indirectIndexed(readImmediateByte());
            int carryAmount = getFlagStatus(Carry_Flag) ? 0 : 1;
            int sum = originalValue - additionByte - carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectSBCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, (originalValue >= (additionByte + carryAmount)));
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcodeF4()
        {
            //Unofficial Opcode: NOP with zero page + X read
            programCounter++;

            mClock += 3;
            tClock += 12;

            // 4 cycles total. Read opcode byte, operand byte, and read value from address, and index of X address.
        }

        private void opcodeF5()
        {
            //SBC: Subtract (Zero Page + X Byte) address value and Carry Flag value from Accumulator

            int originalValue = accumulator;
            int additionByte = zeroPageIndexed(readImmediateByte(), xAddress);
            int carryAmount = getFlagStatus(Carry_Flag) ? 0 : 1;
            int sum = originalValue - additionByte - carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectSBCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, (originalValue >= (additionByte + carryAmount)));
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcodeF6()
        {
            //Increment data at Zero page + X address

            byte address = readImmediateByte();
            address += xAddress;
            byte value = readCPURam(address);
            ++value;

            writeCPURam(address, value);

            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            mClock += 3;
            tClock += 12;

            //6 cycles
        }

        private void opcodeF8()
        {
            //SED: Set decimal flag to enabled

            setFlagTo(Decimal_Mode_Flag, true);

            //2 Cycles

            mClock += 1;
            tClock += 4;
        }

        private void opcodeF9()
        {
            //SBC: Subtract Byte at (absolute + Y) address and Carry Flag value from Accumulator

            int originalValue = accumulator;
            ushort address = readImmediateUShort();
            address += yAddress;
            int additionByte = readCPURam(address);
            int carryAmount = getFlagStatus(Carry_Flag) ? 0 : 1;
            int sum = originalValue - additionByte - carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectSBCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, (originalValue >= (additionByte + carryAmount)));
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        private void opcodeFE()
        {
            //Increment data at absolute + X address

            ushort address = readImmediateUShort();
            address += xAddress;
            byte value = readCPURam(address);
            ++value;

            writeCPURam(address, value);

            setFlagTo(Zero_Flag, value == 0);
            setFlagTo(Negative_Flag, (value & 0x80) == 0x80);

            mClock += 3;
            tClock += 12;

            //7 cycles
        }

        private  void opcodeFD()
        {
            //SBC: Subtract Byte at (absolute + X) address and Carry Flag value from Accumulator

            int originalValue = accumulator;
            ushort address = readImmediateUShort();
            address += xAddress;
            int additionByte = readCPURam(address);
            int carryAmount = getFlagStatus(Carry_Flag) ? 0 : 1;
            int sum = originalValue - additionByte - carryAmount;

            accumulator = (byte)(sum & 0xFF);

            setFlagTo(Overflow_Flag, detectSBCOverflow(originalValue, additionByte, sum));
            setFlagTo(Carry_Flag, (originalValue >= (additionByte + carryAmount)));
            setFlagTo(Zero_Flag, accumulator == 0);
            setFlagTo(Negative_Flag, (accumulator & 0x80) == 0x80);
        }

        /*
         * @Name: setFlagTo
         * @Params: byte flag: This contains the bits representing the flag to modify status with.
         * @Params: bool enable: This tells whether to enable or disable this particular flag.
         * @Purpose: Allows enabling and disabling specific flags in the CPU status register.
         */
        private void setFlagTo(byte flag, bool enable)
        {
            if (enable)
            {
                statusRegister |= flag;
            }
            else
            {
                statusRegister &= (byte)(~flag);
            }
        }

        /*
         * @Name: getFlagStatus
         * @Params: byte flag: This contains the bits to check in the status register.
         * @Purpose: Allows checking if a specific flag bit is enabled or disabled in the status register.
         */
        private bool getFlagStatus(byte flag)
        {
            return (statusRegister & flag) == flag;
        }

        public void fetchAndExecute()
        {
            byte opcode = readImmediateByte();
            switch (opcode)
            {
                case 0x00:
                    opcode00();
                    break;
                case 0x01:
                    opcode01();
                    break;
                    // No 02
                    // No 03
                case 0x04:
                    opcode04(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x05:
                    opcode05();
                    break;
                case 0x06:
                    opcode06();
                    break;
                    // No 07
                case 0x08:
                    opcode08();
                    break;
                case 0x09:
                    opcode09();
                    break;
                case 0x0A:
                    opcode0A();
                    break;
                    // No 0B
                case 0x0C:
                    opcode0C(); // Unofficial Opcode: 3 byte NOP.
                    break;
                case 0x0D:
                    opcode0D();
                    break;
                case 0x0E:
                    opcode0E();
                    break;
                    // No 0F
                case 0x10:
                    opcode10();
                    break;
                case 0x11:
                    opcode11();
                    break;
                    // No 12
                    // No 13
                case 0x14:
                    opcode14();  // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x15:
                    opcode15();
                    break;
                case 0x16:
                    opcode16();
                    break;
                    // No 17
                case 0x18:
                    opcode18();
                    break;
                case 0x19:
                    opcode19();
                    break;
                    // No 1A
                    // No 1B
                    // No 1C
                case 0x1D:
                    opcode1D();
                    break;
                case 0x1E:
                    opcode1E();
                    break;
                    // No 1F
                case 0x20:
                    opcode20();
                    break;
                case 0x21:
                    opcode21();
                    break;
                    // No 22
                    // No 23
                case 0x24:
                    opcode24();
                    break;
                case 0x25:
                    opcode25();
                    break;
                case 0x26:
                    opcode26();
                    break;
                    // No 27
                case 0x28:
                    opcode28();
                    break;
                case 0x29:
                    opcode29();
                    break;
                case 0x2A:
                    opcode2A();
                    break;
                    // No 2B
                case 0x2C:
                    opcode2C();
                    break;
                case 0x2D:
                    opcode2D();
                    break;
                case 0x2E:
                    opcode2E();
                    break;
                    // No 2F
                case 0x30:
                    opcode30();
                    break;
                case 0x31:
                    opcode31();
                    break;
                    // No 32
                    // No 33
                case 0x34:
                    opcode34(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x35:
                    opcode35();
                    break;
                case 0x36:
                    opcode36();
                    break;
                    // No 37
                case 0x38:
                    opcode38();
                    break;
                case 0x39:
                    opcode39();
                    break;
                    // No 3A
                    // No 3B
                    // No 3C
                case 0x3D:
                    opcode3D();
                    break;
                case 0x3E:
                    opcode3E();
                    break;
                    // No 3F
                case 0x40:
                    opcode40();
                    break;
                case 0x41:
                    opcode41();
                    break;
                    // No 42
                    // No 43
                case 0x44:
                    opcode44(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x45:
                    opcode45();
                    break;
                case 0x46:
                    opcode46();
                    break;
                    // No 47
                case 0x48:
                    opcode48();
                    break;
                case 0x49:
                    opcode49();
                    break;
                case 0x4A:
                    opcode4A();
                    break;
                    // No 4B
                case 0x4C:
                    opcode4C();
                    break;
                case 0x4D:
                    opcode4D();
                    break;
                case 0x4E:
                    opcode4E();
                    break;
                    // No 4F
                case 0x50:
                    opcode50();
                    break;
                case 0x51:
                    opcode51();
                    break;
                    // No 52
                    // No 53
                case 0x54:
                    opcode54(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x55:
                    opcode55();
                    break;
                case 0x56:
                    opcode56();
                    break;
                    // No 57
                case 0x58:
                    opcode58();
                    break;
                case 0x59:
                    opcode59();
                    break;
                    // No 5A
                    // No 5B
                    // No 5C
                case 0x5D:
                    opcode5D();
                    break;
                case 0x5E:
                    opcode5E();
                    break;
                    // No 5F
                case 0x60:
                    opcode60();
                    break;
                case 0x61:
                    opcode61();
                    break;
                    // No 62
                    // No 63
                case 0x64:
                    opcode64(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x65:
                    opcode65();
                    break;
                case 0x66:
                    opcode66();
                    break;
                    // No 67
                case 0x68:
                    opcode68();
                    break;
                case 0x69:
                    opcode69();
                    break;
                case 0x6A:
                    opcode6A();
                    break;
                    // No 6B
                case 0x6C:
                    opcode6C();
                    break;
                case 0x6D:
                    opcode6D();
                    break;
                case 0x6E:
                    opcode6E();
                    break;
                    // No 6F
                case 0x70:
                    opcode70();
                    break;
                case 0x71:
                    opcode71();
                    break;
                    // No 72
                    // No 73
                case 0x74:
                    opcode74(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x75:
                    opcode75();
                    break;
                case 0x76:
                    opcode76();
                    break;
                    // No 77
                case 0x78:
                    opcode78();
                    break;
                case 0x79:
                    opcode79();
                    break;
                    // No 7A
                    // No 7B
                    // No 7C
                case 0x7D:
                    opcode7D();
                    break;
                case 0x7E:
                    opcode7E();
                    break;
                    // No 7F
                case 0x80:
                    opcode80(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x81:
                    opcode81();
                    break;
                case 0x82:
                    opcode82(); // Unofficial Opcode: 2 byte NOP.
                    break;
                    // No 83
                case 0x84:
                    opcode84();
                    break;
                case 0x85:
                    opcode85();
                    break;
                case 0x86:
                    opcode86();
                    break;
                    // No 87
                case 0x88:
                    opcode88();
                    break;
                case 0x89:
                    opcode89(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0x8A:
                    opcode8A();
                    break;
                    // No 8B
                case 0x8C:
                    opcode8C();
                    break;
                case 0x8D:
                    opcode8D();
                    break;
                case 0x8E:
                    opcode8E();
                    break;
                    // No 8F
                case 0x90:
                    opcode90();
                    break;
                case 0x91:
                    opcode91();
                    break;
                    // No 92
                    // No 93
                case 0x94:
                    opcode94();
                    break;
                case 0x95:
                    opcode95();
                    break;
                case 0x96:
                    opcode96();
                    break;
                    // No 97
                case 0x98:
                    opcode98();
                    break;
                case 0x99:
                    opcode99();
                    break;
                case 0x9A:
                    opcode9A();
                    break;
                    // No 9B
                    // No 9C
                case 0x9D:
                    opcode9D();
                    break;
                    // No 9E
                    // No 9F
                case 0xA0:
                    opcodeA0();
                    break;
                case 0xA1:
                    opcodeA1();
                    break;
                case 0xA2:
                    opcodeA2();
                    break;
                    // No A3
                case 0xA4:
                    opcodeA4();
                    break;
                case 0xA5:
                    opcodeA5();
                    break;
                case 0xA6:
                    opcodeA6();
                    break;
                    // No A7
                case 0xA8:
                    opcodeA8();
                    break;
                case 0xA9:
                    opcodeA9();
                    break;
                case 0xAA:
                    opcodeAA();
                    break;
                    // No AB
                case 0xAC:
                    opcodeAC();
                    break;
                case 0xAD:
                    opcodeAD();
                    break;
                case 0xAE:
                    opcodeAE();
                    break;
                    // No AF
                case 0xB0:
                    opcodeB0();
                    break;
                case 0xB1:
                    opcodeB1();
                    break;
                    // No B2
                    // No B3
                case 0xB4:
                    opcodeB4();
                    break;
                case 0xB5:
                    opcodeB5();
                    break;
                case 0xB6:
                    opcodeB6();
                    break;
                    // No B7
                case 0xB8:
                    opcodeB8();
                    break;
                case 0xB9:
                    opcodeB9();
                    break;
                case 0xBA:
                    opcodeBA();
                    break;
                    // No BB
                case 0xBC:
                    opcodeBC();
                    break;
                case 0xBD:
                    opcodeBD();
                    break;
                case 0xBE:
                    opcodeBE();
                    break;
                    // No BF
                case 0xC0:
                    opcodeC0();
                    break;
                case 0xC1:
                    opcodeC1();
                    break;
                case 0xC2:
                    opcodeC2(); // Unofficial Opcode: 2 byte NOP.
                    break;
                    // No C3
                case 0xC4:
                    opcodeC4();
                    break;
                case 0xC5:
                    opcodeC5();
                    break;
                case 0xC6:
                    opcodeC6();
                    break;
                    // No C7
                case 0xC8:
                    opcodeC8();
                    break;
                case 0xC9:
                    opcodeC9();
                    break;
                case 0xCA:
                    opcodeCA();
                    break;
                    // No CB
                case 0xCC:
                    opcodeCC();
                    break;
                case 0xCD:
                    opcodeCD();
                    break;
                case 0xCE:
                    opcodeCE();
                    break;
                    // No CF
                case 0xD0:
                    opcodeD0();
                    break;
                case 0xD1:
                    opcodeD1();
                    break;
                    // No D2
                    // No D3
                case 0xD4:
                    opcodeD4(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0xD5:
                    opcodeD5();
                    break;
                case 0xD6:
                    opcodeD6();
                    break;
                    // No D7
                case 0xD8:
                    opcodeD8();
                    break;
                case 0xD9:
                    opcodeD9();
                    break;
                    // No DA
                    // No DB
                    // No DC
                case 0xDD:
                    opcodeDD();
                    break;
                case 0xDE:
                    opcodeDE();
                    break;
                    // No DF
                case 0xE0:
                    opcodeE0();
                    break;
                case 0xE1:
                    opcodeE1();
                    break;
                case 0xE2:
                    opcodeE2(); // Unofficial Opcode: 2 byte NOP.
                    break;
                    // No E3
                case 0xE4:
                    opcodeE4();
                    break;
                case 0xE5:
                    opcodeE5();
                    break;
                case 0xE6:
                    opcodeE6();
                    break;
                    // No E7
                case 0xE8:
                    opcodeE8();
                    break;
                case 0xE9:
                    opcodeE9();
                    break;
                case 0xEA:
                    opcodeEA();
                    break;
                    // No EB
                case 0xEC:
                    opcodeEC();
                    break;
                case 0xED:
                    opcodeED();
                    break;
                case 0xEE:
                    opcodeEE();
                    break;
                    // No EF
                case 0xF0:
                    opcodeF0();
                    break;
                case 0xF1:
                    opcodeF1();
                    break;
                    // No F2
                    // No F3
                case 0xF4:
                    opcodeF4(); // Unofficial Opcode: 2 byte NOP.
                    break;
                case 0xF5:
                    opcodeF5();
                    break;
                case 0xF6:
                    opcodeF6();
                    break;
                    // No F7
                case 0xF8:
                    opcodeF8();
                    break;
                case 0xF9:
                    opcodeF9();
                    break;
                    // No FA
                    // No FB
                    // No FC
                case 0xFD:
                    opcodeFD();
                    break;
                case 0xFE:
                    opcodeFE();
                    break;
                    // No FF
            }
        }

        public byte getAccumulator()
        {
            return accumulator;
        }

        public byte getStackPointer()
        {
            return stackPointer;
        }

        public byte getStatus()
        {
            return statusRegister;
        }

        public ushort getProgramCounter()
        {
            return programCounter;
        }

        public byte getXRegister()
        {
            return xAddress;
        }

        public byte getYRegister()
        {
            return yAddress;
        }

        public void setStackPointer(byte newSP)
        {
            stackPointer = newSP;
        }

        public void serviceInterrupt()
        {
            if(!getFlagStatus(Interrupt_Disable_Flag) && pendingInterrupt)
            {
                pushStackU16(programCounter);
                pushStackU8(statusRegister);

                programCounter = (ushort)(readCPURam(0xFFFE) | (readCPURam(0xFFFF) << 8));
                setFlagTo(Interrupt_Disable_Flag, true);
            }
        }

        public void serviceNonMaskableInterrupt()
        {
            if(Core.ppu.pendingNMI)
            {
                pushStackU16(programCounter);
                pushStackU8(statusRegister);

                programCounter = (ushort)(readCPURam(0xFFFA) | (readCPURam(0xFFFB) << 8));
                Core.ppu.pendingNMI = false;
            }
        }

        public void resetCPU() {
            accumulator = 0;
            xAddress = 0;
            yAddress = 0;
            programCounter = 0x8000;
            stackPointer = 0xFD;
            statusRegister = 0 | Empty_Flag | Interrupt_Disable_Flag;

            cpuRam = new byte[0x10000];

            pendingInterrupt = false;

            tClock = 0;
            mClock = 0;
        }
    }
}