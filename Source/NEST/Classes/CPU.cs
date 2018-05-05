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
        private byte accumulator = 0;
        private byte xAddress = 0;
        private byte yAddress = 0;
        public ushort programCounter = 0;
        private byte stackPointer = 0;
        private byte statusRegister = 0 | Empty_Flag;


        private const byte Carry_Flag           = 0x1;          //C
        private const byte Zero_Flag            = 0x1 << 1;     //Z
        private const byte Interrupt_Flag       = 0x1 << 2;     //I
        private const byte Decimal_Mode_Flag    = 0x1 << 3;     //D
        private const byte Breakpoint_Flag      = 0x1 << 4;     //B
        private const byte Empty_Flag           = 0x1 << 5;     //-
        private const byte Overflow_Flag        = 0x1 << 6;     //V
        private const byte Negative_Flag        = 0x1 << 7;     //S

        public int tClock = 0;
        public int mClock = 0;

        private byte readImmediateByte()
        {
            mClock += 1;
            tClock += 4;

            return Core.beakMemory.readMemory(programCounter++);
        }

    }
}
