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

        private byte absolute(ushort address)
        {
            mClock += 1;
            tClock += 4;

            return readCPURam(address);
        }

        private byte absoluteIndexed(byte argument, byte index)
        {
            //a, x //a, y
            return readCPURam((ushort)(argument + index));
        }


        public byte readCPURam(ushort address)
        {
            return cpuRam[address];
        }



    }
}
