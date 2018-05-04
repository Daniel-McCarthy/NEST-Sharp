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

        bool checkForINES2Format()
        {
            // Checks for NES 2.0 flags being set in byte 8 of 16.

            if (romData != null && romData.Length >= 8)
            {
                // If bits 0000xx00 contain the value 2, then this header is NES 2.0 format.

                return ((romData[7] & 0x0C) == 2);
            }

            return false;
        }


        public bool loadRom(string path)
        {
            if (File.Exists(path))
            {
                byte[] newRomData = File.ReadAllBytes(path);

                if (newRomData != null && newRomData.Length > 0)
                {
                    romData = newRomData;
                    return true;
                }
                else
                {
                    MessageBox.Show("Error: Rom was unable to be opened or contains no data.");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Error: Rom does not exist.");
                return false;
            }
        }
    }
}
