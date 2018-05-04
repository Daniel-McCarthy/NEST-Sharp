using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes
{
    static class Binary
    {

        static public byte rotateLeft(byte number)
        {
            byte lostBit = (byte)((number & 0x80) >> 7);
            number <<= 1;
            number |= lostBit;

            return number;
        }

        static public byte rotateLeft(byte number, int shiftAmount)
        {
            for (int i = 0; i < shiftAmount; i++)
            {
                number = rotateLeft(number);
            }

            return number;
        }

        static public byte rotateRight(byte number)
        {
            byte lostBit = (byte)((number & 0x01) << 7);
            number >>= 1;
            number |= lostBit;

            return number;
        }

        static public byte rotateRight(byte number, int shiftAmount)
        {
            for (int i = 0; i < shiftAmount; i++)
            {
                number = rotateRight(number);
            }

            return number;
        }

    }
}
