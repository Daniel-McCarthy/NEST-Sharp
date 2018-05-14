﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes
{
    class CPU
    {
        //NEST CPU Registers
        private byte accumulator = 0;                   //Contains results of arithmetic functions
        private byte xAddress = 0;                      //X Index Value
        private byte yAddress = 0;                      //Y Index Value
        public ushort programCounter = 0x8000;          //Tracks position in the program
        private byte stackPointer = 0;                  //Tracks position in the stack
        private byte statusRegister = 0 | Empty_Flag;   //Tracks what flags are set in the CPU

        private const byte Carry_Flag               = 0x1;          //C
        private const byte Zero_Flag                = 0x1 << 1;     //Z
        private const byte Interrupt_Disable_Flag   = 0x1 << 2;     //I
        private const byte Decimal_Mode_Flag        = 0x1 << 3;     //D
        private const byte Breakpoint_Flag          = 0x1 << 4;     //B
        private const byte Empty_Flag               = 0x1 << 5;     //-
        private const byte Overflow_Flag            = 0x1 << 6;     //V
        private const byte Negative_Flag            = 0x1 << 7;     //S

        //CPU Ram Map:
        //0x0000-0x07FF: Internal Ram
        //0x0800-0x1FFF: Mirroring of Internal Ram
        //0x2000-0x2007: PPU Registers
        //0x2008-0x3FFF: Mirroring of PPU Registers
        //0x4000-0x4017: APU & I/O Registers
        //0x4018-0x401F: APU & I/O Functionality
        //0x4020-0xFFFF: Cartridge Space
        private byte[] cpuRam = new byte[0x10000];

        public int tClock = 0;
        public int mClock = 0;

        private byte readImmediateByte()
        {
            return readCPURam(programCounter++);
        }

        private ushort readImmediateUShort()
        {
            return (ushort)((readImmediateByte() << 8) | readImmediateByte());
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
            ushort addressLower =readCPURam(zeroPageIndexed(argument, xAddress));
            ushort addressUpper = (ushort)(readCPURam(zeroPageIndexed(argument, xAddress, 1)) << 8);
            return readCPURam((ushort)(addressLower | addressUpper));
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

            return cpuRam[address];
        }

        public void writeCPURam(ushort address, byte value, bool ignoreCycles = false)
        {
            if (!ignoreCycles)
            {
                mClock += 1;
                tClock += 4;
            }

            cpuRam[address] = value;
        }

        public void pushStackU8(byte value)
        {
            writeCPURam((ushort)(0x100 | stackPointer), value);
            stackPointer--;
        }

        public void pushStackU16(ushort value)
        {
            pushStackU8((byte)(value & 0xFF));
            pushStackU8((byte)((value >> 8) & 0xFF));
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
                programCounter += readImmediateByte();
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
            byte value = absolute(address, xAddress);

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

            pushStackU16((ushort)(programCounter - 1));
            programCounter = readImmediateUShort();

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

            statusRegister = popStackU8();

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
                programCounter += readImmediateByte();
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
                programCounter += readImmediateByte();
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
            byte value = absolute(address, xAddress);

            setFlagTo(Carry_Flag, (value & 0x01) == 0x01);          //Set carry flag to old bit 7

            value <<= 1;
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
            address |= readCPURam(addressLocation);
            address |= (ushort)(readCPURam(addressLocation) << 8);

            programCounter = address;

            //3 cycles
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
                programCounter += readImmediateByte();
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

        private void opcode84()
        {
            //STY: Copy value in y register to zero page addres

            writeCPURam(readImmediateByte(), yAddress);

            //3 Cycles
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
                programCounter += readImmediateByte();
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

        private void opcode9A()
        {
            //TXS: Copy value in x register to stack pointer

            stackPointer = xAddress;

            setFlagTo(Zero_Flag, (stackPointer == 0));
            setFlagTo(Negative_Flag, (stackPointer & 0x80) != 0);

            //2 Cycles

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
                programCounter += readImmediateByte();
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

            writeCPURam(address, --value);

            setFlagTo(Zero_Flag, yAddress == 0);
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);

            //5 Cycles
            mClock += 1;
            tClock += 4;

        }

        private void opcodeC8()
        {
            //Increment data at Zero page address

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

            writeCPURam(address, --value);

            setFlagTo(Zero_Flag, yAddress == 0);
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);

            //6 Cycles
            mClock += 1;
            tClock += 4;

        }

        private void opcodeD0()
        {
            //BNE: Branch if Zero Flag disabled

            if (!getFlagStatus(Zero_Flag))
            {
                programCounter += readImmediateByte();
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

            writeCPURam(address, --value);

            setFlagTo(Zero_Flag, yAddress == 0);
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);

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

            writeCPURam(address, --value);

            setFlagTo(Zero_Flag, yAddress == 0);
            setFlagTo(Negative_Flag, (yAddress & 0x80) != 0);

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
                programCounter += readImmediateByte();
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



    }
}
