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
        private byte accumulator = 0;
        private byte xAddress = 0;
        private byte yAddress = 0;
        public ushort programCounter = 0;
        private byte stackPointer = 0;
        private byte statusRegister = 0;

        public int tClock = 0;
        public int mClock = 0;

    }
}
