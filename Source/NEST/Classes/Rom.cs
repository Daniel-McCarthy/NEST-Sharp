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
        public bool loadSuccessful = false;

        private int programRomSize = 0; // x * 16 KB data size.
        private int programRamSize = 0; // x * 8  KB data size. // If this value is zero, it should be assumed to be 8 KB.
        private int chrRomSize = 0; // X * 8  KB data size. // If this value is zero, then CHR Ram is to be used.
        private byte mapperSetting = 0;

        private bool verticalMirroring = false;
        private bool programRamBattery = false;
        private bool trainerIncluded = false;
        private bool ignoreMirroring = false; // True infers use of 4 screen vram.
        private bool usesProgramRam = false; // INES 2.0 feature.


        public Rom(byte[] fileData)
        {
            romData = fileData;

            readRomHeader();
        }

        void readRomHeader()
        {
            if (checkForINESFormat())
            {
                // Read INES.

                parseINESHeader();

                loadSuccessful = true;
            }
        }

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

        void parseINESHeader()
        {
            if (romData != null && romData.Length >= 16)
            {
                programRomSize = romData[4];
                chrRomSize = romData[5];

                byte flags6 = romData[6];

                verticalMirroring = ((flags6 & 0x01) != 0);
                programRamBattery = ((flags6 & 0x02) != 0);
                trainerIncluded = ((flags6 & 0x04) != 0);
                ignoreMirroring = ((flags6 & 0x08) != 0);

                mapperSetting = (byte)((flags6 & 0xF0) >> 4);

                byte flags7 = romData[7];

                mapperSetting |= (byte)((flags7 & 0xF0));

                programRamSize = romData[8];

                //TODO: Support other Flags 7 data.
                //TODO: Support flags 9 and 10.
            }

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

        public byte readByte(ushort address)
        {
            return romData[address];
        }

        public byte[] readBytes(ushort address, int byteCount)
        {
            if((address + (byteCount - 1)) < (getExactDataLength()))
            {
                byte[] data = new byte[byteCount];

                for(int i = 0; i < byteCount; i++)
                {
                    data[i] = romData[address + i];
                }

                return data;
            }

            return null;
        }

        public byte[] readBytesFromAddressToEnd(ushort address)
        {
            int dataLength = romData.Length;
            
            if(address < dataLength)
            {
                int length = romData.Length - address;

                byte[] data = new byte[length];

                for(int i = 0; i < length; i++)
                {
                    data[i] = romData[address + i];
                }

                return data;
            }

            return null;
        }

        public int getExactDataLength()
        {
            return romData.Length;
        }

        public int getProgramRomSize()
        {
            return programRomSize;
        }

        public int getProgramRamSize()
        {
            return programRamSize;
        }

        public int getCHRRomSize()
        {
            return chrRomSize;
        }

        public int getMapperSetting()
        {
            return mapperSetting;
        }

        public bool getVerticalMirroring()
        {
            return verticalMirroring;
        }

        public bool getProgramRamBattery()
        {
            return programRamBattery;
        }

        public bool getTrainerIncluded()
        {
            return trainerIncluded;
        }

        public bool getIgnoreMirroring()
        {
            return ignoreMirroring;
        }

        public bool getUsesProgramRam()
        {
            return usesProgramRam;
        }
    }
}
