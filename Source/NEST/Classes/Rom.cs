using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEST.Classes
{

    class Rom
    {
        private byte[] romData;
        public Rom(byte[] fileData)
        {
            romData = fileData;

        bool checkForINESFormat()
        {
            // Checks first for file bytes for "NES" and MS-DOS eof signature.

            if (romData != null && romData.Length >= 4)
            {
                return ((romData[0] << 24) | (romData[1] << 16) | (romData[2] << 8) | romData[3]) == 0x4E45531A;
            }

            return false;
        }

        }
    }
}
