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
        private byte accumulator = 0;                   //Contains results of arithmetic functions
        private byte xAddress = 0;                      //X Index Value
        private byte yAddress = 0;                      //Y Index Value
        public ushort programCounter = 0;               //Tracks position in the program
        private byte stackPointer = 0;                  //Tracks position in the stack
        private byte statusRegister = 0 | Empty_Flag;   //Tracks what flags are set in the CPU

        private const byte Carry_Flag           = 0x1;          //C
        private const byte Zero_Flag            = 0x1 << 1;     //Z
        private const byte Interrupt_Flag       = 0x1 << 2;     //I
        private const byte Decimal_Mode_Flag    = 0x1 << 3;     //D
        private const byte Breakpoint_Flag      = 0x1 << 4;     //B
        private const byte Empty_Flag           = 0x1 << 5;     //-
        private const byte Overflow_Flag        = 0x1 << 6;     //V
        private const byte Negative_Flag        = 0x1 << 7;     //S

        private byte[] cpuRam = new byte[0x10000];

        public int tClock = 0;
        public int mClock = 0;

        private byte readImmediateByte()
        {
            mClock += 1;
            tClock += 4;

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
            mClock += 1;
            tClock += 4;

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
            ushort a =readCPURam(argument);
            ushort b = (ushort)(zeroPageIndexed(argument, 0, 1) << 8);
            return readCPURam((ushort)((a | b) + yAddress));
        }


        public byte readCPURam(ushort address)
        {
            return cpuRam[address];
        }

        private void opcode01()
        {
            //Bitwise OR A Indexed Indirect X

            byte value = indexedIndirect(readCPURam(programCounter++));
            accumulator = ((byte)(accumulator | value));

            //TODO: Flags

            mClock += 2;
            tClock += 8;
        }

        private void opcode05()
        {
            //Bitwise OR A Zero Page

            accumulator = ((byte)(accumulator | readCPURam(readImmediateByte())));

            //TODO: Flags

            mClock += 1;
            tClock += 4;
        }

        private void opcode09()
        {
            //Bitwise OR A Immediate Byte

            accumulator = ((byte)(accumulator | readImmediateByte()));

            //TODO: Flags

            mClock += 1;
            tClock += 4;
        }


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



    }
}
