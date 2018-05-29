using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace NEST.Classes
{
    class PPU
    {
        public SFML.Graphics.Image frame = new SFML.Graphics.Image(256, 240);
        public SFML.Graphics.Image fullWindow = new SFML.Graphics.Image(512, 512);

        private byte[] ppuRam = new byte[0x4000];
        private byte[] oamRam = new byte[0x100];
        private Color[] palette = new Color[0x40] { new Color(0x52, 0x52, 0x52), new Color(0x01, 0x1A, 0x51), new Color(0x0F, 0x0F, 0x65), new Color(0x23, 0x06, 0x63), new Color(0x36, 0x03, 0x4B), new Color(0x40, 0x04, 0x26), new Color(0x3F, 0x09, 0x04), new Color(0x32, 0x13, 0x00), new Color(0x1F, 0x20, 0x00), new Color(0x0B, 0x2A, 0x00), new Color(0x00, 0x2F, 0x00), new Color(0x00, 0x2E, 0x0A), new Color(0x00, 0x26, 0x2D), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xA0, 0xA0, 0xA0), new Color(0x1E, 0x4A, 0x9D), new Color(0x38, 0x37, 0xBC), new Color(0x58, 0x28, 0xB8), new Color(0x75, 0x21, 0x94), new Color(0x84, 0x23, 0x5C), new Color(0x82, 0x2E, 0x24), new Color(0x6F, 0x3F, 0x00), new Color(0x51, 0x52, 0x00), new Color(0x31, 0x63, 0x00), new Color(0x1A, 0x6B, 0x05), new Color(0x0E, 0x69, 0x2E), new Color(0x10, 0x5C, 0x68), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xFE, 0xFF, 0xFF), new Color(0x69, 0x9E, 0xFC), new Color(0x89, 0x87, 0xFF), new Color(0xAE, 0x76, 0xFF), new Color(0xCE, 0x6D, 0xF1), new Color(0xE0, 0x70, 0xB2), new Color(0xDE, 0x7C, 0x70), new Color(0xC8, 0x91, 0x3E), new Color(0xA6, 0xA7, 0x25), new Color(0x81, 0xBA, 0x28), new Color(0x63, 0xC4, 0x46), new Color(0x54, 0xC1, 0x7D), new Color(0x56, 0xB3, 0xC0), new Color(0x3C, 0x3C, 0x3C), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xFE, 0xFF, 0xFF), new Color(0xBE, 0xD6, 0xFD), new Color(0xCC, 0xCC, 0xFF), new Color(0xDD, 0xC4, 0xFF), new Color(0xEA, 0xC0, 0xF9), new Color(0xF2, 0xC1, 0xDF), new Color(0xF1, 0xC7, 0xC2), new Color(0xE8, 0xD0, 0xAA), new Color(0xD9, 0xDA, 0x9D), new Color(0xC9, 0xE2, 0x9E), new Color(0xBC, 0xE6, 0xAE), new Color(0xB4, 0xE5, 0xC7), new Color(0xB5, 0xDF, 0xE4), new Color(0xA9, 0xA9, 0xA9), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00) };

        public bool scrollWrittenOnce       = false;
        public bool ppuAddressWrittenOnce   = false;
        public bool oamAddressWrittenOnce   = false;
        public ushort ppuWriteAddress       = 0;
        public ushort tempPPUWriteAddress   = 0;
        public ushort oamWriteAddress       = 0;
        public ushort tempOAMWriteAddress   = 0;
        public bool pendingNMI              = false;
        public bool spriteZeroHit           = false;
        public bool spriteOverflow          = false;
        public bool isNametableMirrored     = false;
        public bool isHorizNametableMirror = false;

        public byte scrollX     = 0;
        public byte scrollY     = 0;
        public int  ly          = 0;
        public byte ppuState    = 0;
        public uint frameCount  = 0;

        public const byte PPU_STATE_PRERENDER   = 0b00000000;
        public const byte PPU_STATE_DRAWING     = 0b00000001;
        public const byte PPU_STATE_POSTRENDER  = 0b00000010;
        public const byte PPU_STATE_VBLANK      = 0b00000011;


        public byte readPPURamByte(ushort address)
        {
            return ppuRam[address];
        }

        public void writePPURamByte(ushort address, byte value)
        {
            ppuRam[address] = value;
        }

        public ushort adjustAddressForNameTableMirroring(ushort address)
        {
            if (address >= 0x2000 && address <= 0x2FFF)
            {
                if (isHorizNametableMirror)
                {
                    //Adjust address to left name table if it is not already
                    if ((address >= 0x2400 && address < 0x2800) || (address >= 0x2C00 && address >= 0x2FFF))
                    {
                        //Address is in right name table so it is adjusted to the left.
                        return (ushort)(address - 0x400);
                    }
                    else
                    {
                        return address;
                    }
                }
                else
                {
                    //Adjust address to upper name table if it is not already
                    if (address >= 0x2800)
                    {
                        //Writing to up name table so adjust and write to down name table also
                        return (ushort)(address - 0x800);
                    }
                    else
                    {
                        return address;
                    }
                }
            }
            else
            {
                //Address was outside nametable range
                return address;
            }
        }

        public byte readOAMRamByte(ushort address)
        {
            return oamRam[address];
        }

        public void writeOAMRamByte(ushort address, byte value)
        {
            oamRam[address] = value;
        }

        public byte getPPUStatus()
        {
            byte ppuStatus = 0;

            byte PPU_STATE_VBLANK = 0b00000011;
            if (Core.ppu.ppuState == PPU_STATE_VBLANK)
            {
                ppuStatus |= 0x80;
            }

            if (Core.ppu.spriteZeroHit)
            {
                ppuStatus |= 0x40;
            }

            if (Core.ppu.spriteOverflow)
            {
                ppuStatus |= 0x20;
            }

            //TODO: Bitwise Or the least significant bits of last byte written into PPU register

            return ppuStatus;
        }

        private byte getPPURegister()
        {
            return Core.cpu.readCPURam(0x2000, true);
        }

        private byte getPPURegisterNameTableSetting()
        {
            //Base Name Table Address Setting
            //0: 0x2000, 1: 0x2400, 2: 0x2800, 3: 0x2C00
            return (byte)(getPPURegister() & 0b00000011);
        }

        public byte getPPURegisterVRAMIncrement()
        {
            //PPU Write Address Increment per read/write:
            //0: Increment of 1 (left to right)
            //1: Increment of 32 (traverses downward)

            return (byte)(((getPPURegister() & 0b00000100) != 0) ? 32 : 1);
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

        private bool getPPURegisterNMISetting()
        {
            //Generate NMI at V-Blank setting
            //0: False
            //1: True
            return (getPPURegister() & 0b10000000) != 0;
        }

        public void oamDMATransfer(ushort address)
        {
            for (int i = 0; i < 0xFF; i++)
            {
                oamRam[i] = Core.cpu.readCPURam((ushort)(address + i), false);
            }
        }

        public void updatePPU(ref uint ppuClocks)
        {
            //Steps:
            //Pre-render scan line 261 && -1
            //Scan lines 0-239
            //Post-render scan line 240
            //V-Blank 241-260

            //Information:
            //262 scanlines
            //240 screen drawn scan lines
            //341 PPU clock cycles per scan line
            //113 CPU clock cycles per scan line

            //int CLOCKS_PER_SCANLINE = 341;

            if (ppuState == PPU_STATE_PRERENDER)
            {
                if(ppuClocks >= 341)
                {
                    ppuClocks -= 341;

                    if(ly == -1)
                    {
                        //If this was the final pre-render scanline, switch to drawing state
                        ly++;
                        ppuState = PPU_STATE_DRAWING;
                    }
                    else
                    {
                        //If this was pre-render scanline 261, then switch to final pre-render scanline
                        ly = -1;
                    }
                }
            }
            else if (ppuState == PPU_STATE_DRAWING)
            {
                if (ppuClocks >= 341)
                {
                    ppuClocks -= 341;

                    SFML.Graphics.Color[] bgLine = drawBGFrameLine((uint)ly);
                    SFML.Graphics.Color[] spriteLine = Core.ppu.drawSpriteLine((byte)ly);

                    drawLineToFrame(bgLine, spriteLine, (uint)ly);

                    ly++;

                    if(ly == 240)
                    {
                        ppuState = PPU_STATE_POSTRENDER;
                    }
                }

            }
            else if (ppuState == PPU_STATE_POSTRENDER)
            {
                if (ppuClocks >= 341)
                {
                    ppuClocks -= 341;

                    //If this was the final pre-render scanline, switch to drawing state
                    ly++;
                    ppuState = PPU_STATE_VBLANK;

                    if(getPPURegisterNMISetting())
                    {
                        pendingNMI = true;
                    }

                    //Draw frame to screen
                    SFML.Graphics.Texture frameTexture = new SFML.Graphics.Texture(Core.ppu.frame);
                    SFML.Graphics.Sprite frameSprite = new SFML.Graphics.Sprite(frameTexture);
                    //frameSprite.Scale = new SFML.System.Vector2f(1, 1);
                    Core.mainWindow.drawCanvas.drawFrame(frameSprite);
                }
            }
            else if (ppuState == PPU_STATE_VBLANK)
            {
                if (ppuClocks >= 341)
                {
                    ppuClocks -= 341;
                    ly++;
                }

                if (ly == 261)
                {
                    bool isEvenFrame = (frameCount % 2) == 0;
                    if (isEvenFrame)
                    {
                        ppuState = PPU_STATE_PRERENDER;
                    }
                    else
                    {
                        ppuState = PPU_STATE_DRAWING;
                        ly = 0;
                    }

                    frameCount++;
                }
            }

        }

        /*
         * This function accepts a 256px wide background and sprite line and writes them to the frame, with the background below and the sprites on top.
         */
        public void drawLineToFrame(Color[] backGroundLine, Color[]  spriteLine, uint ly)
        {
            if( ly < 240)
            {
                for (uint x = 0; x < 256; x++)
                {
                    if (backGroundLine != null)
                    {
                        Core.ppu.frame.SetPixel(x, ly, backGroundLine[x]);
                    }

                    if (spriteLine != null)
                    {
                        if (spriteLine[x] != null && spriteLine[x].A != 0)
                        {
                            Core.ppu.frame.SetPixel(x, ly, spriteLine[x]);
                        }
                    }
                }
            }
        }

        /*
         * This function draws a full line to the full 512x512 window. It draws from both the left and right name tables at the line specified. 
         * It draws a full line and accounts for name table mirroring. From here the data is drawn to the window to be read in later.
         */
        public void drawFullBGLineToWindow(uint lineNumber)
        {
            bool isLineUpperTable;

            if(isNametableMirrored && !isHorizNametableMirror)
            {
                //If name table is vertically mirrored, then draw the top. The bottom is just a copy.
                isLineUpperTable = true;
            }
            else
            {
                //If line is less in the lower half of 512 lines
                isLineUpperTable = (lineNumber < 256);
            }

            Color[] leftTable = drawBGLineFromNameTable(lineNumber % 256, true, isLineUpperTable);
            Color[] rightTable;

            if (isNametableMirrored && isHorizNametableMirror)
            {
                //If name table is horizontally mirrored, the draw the left table for both tables. The right is just a copy.
                rightTable = leftTable;
            }
            else
            {
                rightTable = drawBGLineFromNameTable(lineNumber % 256, false, isLineUpperTable);
            }
            
            for(int x = 0; x < 256; x++)
            {
                fullWindow.SetPixel((uint)x, lineNumber, leftTable[x]);
                fullWindow.SetPixel((uint)(x + 256), lineNumber, rightTable[x]);
            }
        }

        /*
         * Draws a full 256 wide line from a name table of choice at the line specified.
         * This allows us to read an entire name table line by line.
         */
        public Color[] drawBGLineFromNameTable(uint lineNumber, bool isLeftTable, bool isUpperTable)
        {
            int nameTableSelection = (isUpperTable ? 0 : 2) + (isLeftTable ? 0 : 1); //Top Left: 0, Top Right: 1, Bottom Left: 2, Bottom Right: 3
            Color[] line = new Color[256];

            int tileCount = 256 / 8;

            for (int tileIndex = 0; tileIndex < tileCount; tileIndex++)
            {
                //Determine which tile we are drawing
                int tileYPos = (int)lineNumber / 8;
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
                byte attributeValue = ppuRam[0x23C0 + (nameTableSelection * 0x400) + attributeRegion];
                byte tilePalette = attributeValue;

                tilePalette = (upperCell) ? (byte)(tilePalette & 0x0F) : (byte)((tilePalette & 0xF0) >> 4);
                tilePalette = (!leftCell) ? (byte)((tilePalette & 0b1100) >> 2) : (byte)(tilePalette & 0b0011);

                //Determine the background pattern address
                ushort backgroundPatternTableAddress = getPPURegisterBackgroundPatternTableSetting() ? (ushort)(0x0000) : (ushort)(0x1000);

                //Read tileID from name table
                ushort nameTableAddress = (ushort)(0x2000 + (nameTableSelection * 0x400));
                byte tileID = ppuRam[nameTableAddress + tileNumber];

                //Read current line of tile
                int yLineOffset = (int)lineNumber % 8; //This lets us know which line of the tile we are drawing, so that we can read the correct line data.
                ushort patternAddressTemp = (ushort)(backgroundPatternTableAddress + (tileID * 16) + (yLineOffset * 2));
                byte tileDataRow1 = ppuRam[backgroundPatternTableAddress + (tileID * 16) + (yLineOffset)];
                byte tileDataRow2 = ppuRam[backgroundPatternTableAddress + (tileID * 16) + (yLineOffset) + 8];

                byte[] tileColorIndices = new byte[8];

                for (int i = 0; i < 8; i++)
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
                for (int i = 0; i < 8; i++)
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
                        pixelColor = palette[colorIndex];
                    }
                    else
                    {
                        //Read Default BG Color.
                        pixelColor = palette[ppuRam[0x3F00]];
                    }

                    line[(tileIndex * 8) + i] = pixelColor;
                }
            }

            return line;
        }


        /*
         *  New line drawing function. It can cross name tables and account for scroll. It only draws the tiles needed for this particular line.
         *  Instead of drawing a full 512x1 line then selecting the pixels we need, we can reduce the work heavily by only drawing the tiles that will actually be used.
         */
        public Color[] drawBGFrameLine(uint lineNumber)
        {
            Color[] bgLine = new Color[256];

            bool isRightNametable = (getPPURegisterNameTableSetting() % 2) != 0; //Name table setting can be 0 to 3. 1 and 3 are both odd and both right side tables.
            bool isLowerNametable = (getPPURegisterNameTableSetting() > 1); //Name table setting can be 0 to 3. 2 and 3 are both greater than 1 and bottom tables.
            uint xPosOffset = (uint)(isRightNametable ? 0xFF : 0x00);
            uint yPosOffset = (uint)(isLowerNametable ? 0xFF : 0x00);
            uint actualXScroll = (scrollX + xPosOffset) % 512;
            uint actualYScroll = (scrollY + yPosOffset + lineNumber) % 512;

            uint actualReadLine = (scrollY + yPosOffset + lineNumber) % 512;

            int tileXPos = (int)(actualXScroll / 8);
            int tileYPos = (int)(actualReadLine / 8);

            uint totalPixelsDrawn = 0;

            //Draw pixels from first tile (could be partial tile)
            uint actualX = (uint)((actualXScroll + totalPixelsDrawn) % 512);
            tileXPos = (int)(actualX / 8);
            Color[] firstTile = drawBGTileLineFromNameTable(actualReadLine % 8, actualX < 256, !isLowerNametable, tileXPos % 32, tileYPos);

            for (int x = (int)(actualXScroll % 8); x < 8; x++)
            {
                bgLine[totalPixelsDrawn++] = firstTile[x];
            }

            //Draw middle tiles (guaranteed full 8 pixel tiles)
            for (int x = (int)totalPixelsDrawn; x < 256; x += 8)
            {
                if (totalPixelsDrawn <= (256 - 8))
                {
                    actualX = (uint)((actualXScroll + x) % 512);
                    tileXPos = (int)(actualX / 8);

                    Color[] nextTile = drawBGTileLineFromNameTable(actualReadLine % 8, actualX < 256, !isLowerNametable, tileXPos % 32, tileYPos);
                    ;
                    for (int i = 0; i < 8; i++)
                    {
                        bgLine[totalPixelsDrawn++] = nextTile[i];
                    }
                }
                else
                {
                    //End looping
                    x = 256;
                }
            }

            if (totalPixelsDrawn <= 256)
            {
                //Draw pixels from last tile  (could be partial tile)
                actualX = (uint)((actualXScroll + totalPixelsDrawn) % 512);
                tileXPos = (int)(actualX / 8);
                Color[] lastTile = drawBGTileLineFromNameTable(actualReadLine % 8, actualX < 256, !isLowerNametable, tileXPos % 32, tileYPos);
                uint lastTilePixelsToDraw = 256 - totalPixelsDrawn;

                for (int x = 0; x < lastTilePixelsToDraw; x++)
                {
                    bgLine[totalPixelsDrawn++] = lastTile[x];
                }
            }

            return bgLine;
        }

        /*
         * Draws an 8px wide line from a tile in the specified name table. A tile is 8x8, this draws  an 8x1 line at the line specified.
         * This function is so that one can get pixel data from the line necessary on a tile per tile basis.
         */
        public Color[] drawBGTileLineFromNameTable(uint lineNumber, bool isLeftTable, bool isUpperTable, int tileXPos, int tileYPos)
        {
            int nameTableSelection = (isUpperTable ? 0 : 2) + (isLeftTable ? 0 : 1); //Top Left: 0, Top Right: 1, Bottom Left: 2, Bottom Right: 3
            Color[] line = new Color[8];

            int tileCount = 256 / 8;


            //Determine which tile we are drawing
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
            byte attributeValue = ppuRam[0x23C0 + (nameTableSelection * 0x400) + attributeRegion];
            byte tilePalette = attributeValue;

            tilePalette = (upperCell) ? (byte)(tilePalette & 0x0F) : (byte)((tilePalette & 0xF0) >> 4);
            tilePalette = (!leftCell) ? (byte)((tilePalette & 0b1100) >> 2) : (byte)(tilePalette & 0b0011);

            //Determine the background pattern address
            ushort backgroundPatternTableAddress = getPPURegisterBackgroundPatternTableSetting() ? (ushort)(0x0000) : (ushort)(0x1000);

            //Read tileID from name table
            ushort nameTableAddress = (ushort)(0x2000 + (nameTableSelection * 0x400));
            byte tileID = ppuRam[nameTableAddress + tileNumber];

            //Read current line of tile
            int yLineOffset = (int)lineNumber % 8; //This lets us know which line of the tile we are drawing, so that we can read the correct line data.
            ushort patternAddressTemp = (ushort)(backgroundPatternTableAddress + (tileID * 16) + (yLineOffset * 2));
            byte tileDataRow1 = ppuRam[backgroundPatternTableAddress + (tileID * 16) + (yLineOffset)];
            byte tileDataRow2 = ppuRam[backgroundPatternTableAddress + (tileID * 16) + (yLineOffset) + 8];

            byte[] tileColorIndices = new byte[8];

            for (int i = 0; i < 8; i++)
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
            for (int i = 0; i < 8; i++)
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
                    pixelColor = palette[colorIndex];
                }
                else
                {
                    //Read Default BG Color.
                    pixelColor = palette[ppuRam[0x3F00]];
                }

                line[i] = pixelColor;
            }

            return line;
        }

        public Color[] drawSpriteLine(byte lineNumber)
        {
            Color[] line = new Color[256];

            int spriteCount = oamRam.Length / 4;

            for (int spriteIndex = 0; spriteIndex < spriteCount; spriteIndex++)
            {
                byte spriteYPos = oamRam[spriteIndex * 4];
                byte tileID     = oamRam[(spriteIndex * 4) + 1];
                byte spriteXPos = oamRam[(spriteIndex * 4) + 3];
                byte attributes = oamRam[(spriteIndex * 4) + 2];

                byte spritePalette      = (byte)(4 + (attributes & 0b11));
                byte spriteHeight       = getPPURegisterSpriteSizeSetting() ? (byte)16 : (byte)8;
                bool isBelowBackground  = (attributes & 0b00100000) != 0;
                bool isXFlipped         = (attributes & 0b01000000) != 0;
                bool isYFlipped         = (attributes & 0b10000000) != 0;

                bool isSpriteOnLine = (spriteYPos <= lineNumber) && ((spriteYPos + (spriteHeight)) > lineNumber);

                if(isSpriteOnLine && !isBelowBackground)
                {
                    //Read current line of sprite tile
                    //int yLineOffset = lineNumber % spriteHeight; //This lets us know which line of the tile we are drawing, so that we can read the correct line data.
                    int yLineOffset = lineNumber - spriteYPos;
                    yLineOffset = (isYFlipped) ? ((spriteHeight - 1) - yLineOffset) : yLineOffset;

                    ushort spritePatternTableAddress = (ushort)(getPPURegisterSpritePatternTableSetting() ? 0x1000 : 0x0000);
                    ushort patternAddressTemp = (ushort)((tileID * 16) + (yLineOffset * 2) + spritePatternTableAddress);
                    byte tileDataRow1 = ppuRam[((tileID * 16) + (yLineOffset))];
                    byte tileDataRow2 = ppuRam[((tileID * 16) + (yLineOffset)) + 8];

                    byte[] tileColorIndices = new byte[8];

                    for (int i = 0; i < 8; i++)
                    {
                        //Patterns are defined by 16 bytes that detail an 8x8 pixel pattern. 2 bytes per line.
                        //Each pixel can be one of four colors. 2 bytes are read in. The first bit
                        //in the first byte and the first bit in the second byte define the color of the first pixel.
                        tileColorIndices[i] = (byte)(((tileDataRow1 & 0x01)) | (tileDataRow2 & 0x01) << 1);
                        tileDataRow1 >>= 1;
                        tileDataRow2 >>= 1;
                    }

                    if(!isXFlipped)
                        Array.Reverse(tileColorIndices);

                    //Draw current tile data to line
                    for (int i = 0; i < 8; i++)
                    {
                        Color pixelColor;

                        if (tileColorIndices[i] != 0)
                        {
                            //Use the palette index retrieved from the attribute table to select the proper palette from 0x3F00-0x3F20
                            //Select pixel color index from selected palette using tile color index retrieved from the tile data rows.
                            //Use this final color index to index a color from the NES color palette.
                            //byte colorAddress = (byte)((tilePalette * 4) + tileColorIndices[i]);
                            byte colorAddress = (byte)((spritePalette << 2) | tileColorIndices[i]);
                            byte colorIndex = ppuRam[0x3F00 + colorAddress];
                            pixelColor = palette[colorIndex];

                            if ((spriteXPos + i) < line.Length)
                                line[spriteXPos + i] = pixelColor;
                        }
                    }
                }
            }

            return line;
        }

    }
}
