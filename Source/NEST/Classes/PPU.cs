using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes
{
    class PPU
    {
        private byte[] ppuRam = new byte[0x4000];
        private byte[] oamRam = new byte[0x100];

        private byte getPPURegister()
        {
            return Core.cpu.readCPURam(0x2000, true);
        }

        private byte getPPURegisterTableSetting()
        {
            //Base Name Table Address Setting
            //0: 0x2000, 1: 0x2400, 2: 0x2800, 3: 0x2C00
            return (byte)(getPPURegister() & 0b00000111);
        }

        private bool getPPURegisterSpritePatternTableSetting()
        {
            //Pattern Table address for 8x8 sprites
            //0: 0x0000, 1: 0x1000
            return (getPPURegister() & 0b0001000) != 0;
        }

        private bool getPPURegisterBackgroundPatternTableSetting()
        {
            //Pattern Table address for background
            //0: 0x0000, 1: 0x1000
            return (getPPURegister() & 0b00100000) != 0;
        }

        private bool getPPURegisterSpriteSizeSetting()
        {
            //Sprite size Setting
            //0: 8x8 Sprites, 1: 8x16 Sprites
            return (getPPURegister() & 0b01000000) != 0;
        }


        public Color[] drawBGLineFromNameTable1(byte lineNumber)
        {
            Color[] line = new Color[256];

            int tileCount = 256 / 8;
            int cellCount = 256 / 16;

            for (int tileIndex = 0; tileIndex < tileCount; tileIndex++)
            {
                //Determine which tile we are drawing
                int tileYPos = lineNumber / 8;
                int tileXPos = tileIndex;
                int tileNumber = tileXPos + (tileYPos * 32);

                //Determine which name table cell the current tile is in
                int cellYPos = (tileYPos / 2);
                int cellXPos = (tileXPos / 2);
                int cellNumber = (cellYPos * 16) + cellXPos;
                bool leftTile = (tileXPos % 2) == 0;
                bool upperTile = (tileYPos % 2) == 0;
                bool leftCell = (cellXPos % 2) == 0;
                bool upperCell = (cellYPos % 2) == 0;


                //Determine the attribute byte for this cell

                //A Cell is made up of 2 rows of 2 tiles in a square.
                //One attribute byte defines the palettes for a 2x2 square of cells.
                //The bottom right cell is cell 1, bottom left is cell 2, upper right is cell 3, and upper left is cell 4.
                //The attribute data is 8 bits. 2 bits for each cell. These bits are the palette number.
                //Then we retrieve the 2 relevant bits that define the palette number for this tile.
                //The attribute value contains the palettes in the order of 0bAABBCCDD. AA is cell 4. BB is cell 3. CC is cell 2. DD is cell 1.

                int attributeRegion = ((cellYPos / 2) * 8) + (cellXPos / 2);
                byte attributeValue = ppuRam[0x23C0 + attributeRegion];
                byte tilePalette = attributeValue;

                tilePalette = (upperCell) ? (byte)(tilePalette & 0x0F) : (byte)((tilePalette & 0xF0) >> 4);
                tilePalette = (!leftCell) ? (byte)((tilePalette & 0b1100) >> 2) : (byte)(tilePalette & 0b0011);

                //Determine the background pattern address
                ushort backgroundPatternTableAddress = getPPURegisterBackgroundPatternTableSetting() ? (ushort)(0x0000) : (ushort)(0x1000);

                //Read tileID from name table
                byte tileID = ppuRam[0x2000 + tileNumber];

                //Read current line of tile
                int yLineOffset = lineNumber % 8; //This lets us know which line of the tile we are drawing, so that we can read the correct line data.
                ushort patternAddressTemp = (ushort)(backgroundPatternTableAddress + (tileID * 16) + (yLineOffset * 2));
                byte tileDataRow1 = ppuRam[backgroundPatternTableAddress + (tileID * 16) + (yLineOffset)];
                byte tileDataRow2 = ppuRam[backgroundPatternTableAddress + (tileID * 16) + (yLineOffset) + 8];

                byte[] tileColorIndices = new byte[8];
                
                for(int i = 0; i < 8; i++)
                {
                    //Patterns are defined by 16 bytes that detail an 8x8 pixel pattern. 2 bytes per line.
                    //Each pixel can be one of four colors. 2 bytes are read in. The first bit
                    //in the first byte and the first bit in the second byte define the color of the first pixel.
                    tileColorIndices[i] = (byte)(((tileDataRow1 & 0x01)) | (tileDataRow2 & 0x01) << 1);
                    tileDataRow1 >>= 1;
                    tileDataRow2 >>= 1;
                }

                Array.Reverse(tileColorIndices);

                //Draw current tile data to line
                for(int i = 0; i < 8; i++)
                {
                    Color pixelColor;

                    if (tileColorIndices[i] != 0)
                    {
                        //Use the palette index retrieved from the attribute table to select the proper palette from 0x3F00-0x3F20
                        //Select pixel color index from selected palette using tile color index retrieved from the tile data rows.
                        //Use this final color index to index a color from the NES color palette.
                        //byte colorAddress = (byte)((tilePalette * 4) + tileColorIndices[i]);
                        byte colorAddress = (byte)((tilePalette << 2) | tileColorIndices[i]);
                        byte colorIndex = ppuRam[0x3F00 + colorAddress];
                        pixelColor = tempPalette[colorIndex];
                    }
                    else
                    {
                        //Read Default BG Color.
                        pixelColor = tempPalette[ppuRam[0x3F00]];
                    }

                    line[(tileIndex * 8) + i] = pixelColor;
                }
            }

            return line;
        }

    }
}
