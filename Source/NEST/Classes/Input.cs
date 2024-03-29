﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes
{
    class Input
    {

        //NES keys:
        //[Up][Left][Right][Down][A][B][Start][Select]

        //Mapped to standard keyboard keys:
        //[Up][Left][Right][Down][Z][X][Enter][RShift]

        int joy1NextInputRead = 0; //Reading the input status several times will return the next key. This is how we keep track of which key to show next.
        bool joy1Strobing = false;
        bool joy1PreviousStrobeValue = false;
        bool joy1Connected = true;
	    bool joy1KeyUp = false;
        bool joy1KeyDown = false;
        bool joy1KeyLeft = false;
        bool joy1KeyRight = false;
        bool joy1KeyStart = false;
        bool joy1KeySelect = false;
        bool joy1KeyA = false;
        bool joy1KeyB = false;

        int joy2NextInputRead = 0; //Reading the input status several times will return the next key. This is how we keep track of which key to show next.
        bool joy2Strobing = false;
        bool joy2PreviousStrobeValue = false;
        bool joy2Connected = false;
        bool joy2KeyUp = false;
        bool joy2KeyDown = false;
        bool joy2KeyLeft = false;
        bool joy2KeyRight = false;
        bool joy2KeyStart = false;
        bool joy2KeySelect = false;
        bool joy2KeyA = false;
        bool joy2KeyB = false;

        public void joyPadRegisterWrite(byte newValue, bool isJoyPad2 = false)
        {
            bool newStrobeValue = (newValue != 0);

            if(!isJoyPad2)
            {
                joy1PreviousStrobeValue = joy1Strobing;
                joy1Strobing = newStrobeValue;

                if(joy1PreviousStrobeValue && !joy1Strobing)
                {
                    //Reset joy input reading
                    joy1NextInputRead = 0;
                }
            }
            else
            {
                joy2PreviousStrobeValue = joy2Strobing;
                joy2Strobing = newStrobeValue;

                if (joy2PreviousStrobeValue && !joy2Strobing)
                {
                    //Reset joy input reading
                    joy2NextInputRead = 0;
                }
            }
        }

        public byte joyPadRegisterRead(bool isJoyPad2 = false)
        {
            byte status = 0;

            if (!isJoyPad2)
            {
                if(joy1Strobing)
                {
                    //Reset input cycle
                    joy1NextInputRead = 0;
                }

                if (joy1Connected)
                {
                    switch(joy1NextInputRead)
                    {
                        case 0:
                            {
                                status = (byte)(joy1KeyA ? 1 : 0);
                                break;
                            }
                        case 1:
                            {
                                status = (byte)(joy1KeyB ? 1 : 0);
                                break;
                            }
                        case 2:
                            {
                                status = (byte)(joy1KeySelect ? 1 : 0);
                                break;
                            }
                        case 3:
                            {
                                status = (byte)(joy1KeyStart ? 1 : 0);
                                break;
                            }
                        case 4:
                            {
                                status = (byte)(joy1KeyUp ? 1 : 0);
                                break;
                            }
                        case 5:
                            {
                                status = (byte)(joy1KeyDown ? 1 : 0);
                                break;
                            }
                        case 6:
                            {
                                status = (byte)(joy1KeyLeft ? 1 : 0);
                                break;
                            }
                        case 7:
                            {
                                status = (byte)(joy1KeyRight ? 1 : 0);
                                break;
                            }
                    }

                    joy1NextInputRead = (joy1NextInputRead + 1) % 8;

                    return (byte)(status | 0x40);
                }

            }
            else
            {
                if (joy2Strobing)
                {
                    //Reset input cycle
                    joy2NextInputRead = 0;
                }

                if (joy2Connected)
                {
                    switch (joy2NextInputRead)
                    {
                        case 0:
                            {
                                status = (byte)(joy2KeyA ? 1 : 0);
                                break;
                            }
                        case 1:
                            {
                                status = (byte)(joy2KeyB ? 1 : 0);
                                break;
                            }
                        case 2:
                            {
                                status = (byte)(joy2KeySelect ? 1 : 0);
                                break;
                            }
                        case 3:
                            {
                                status = (byte)(joy2KeyStart ? 1 : 0);
                                break;
                            }
                        case 4:
                            {
                                status = (byte)(joy2KeyUp ? 1 : 0);
                                break;
                            }
                        case 5:
                            {
                                status = (byte)(joy2KeyDown ? 1 : 0);
                                break;
                            }
                        case 6:
                            {
                                status = (byte)(joy2KeyLeft ? 1 : 0);
                                break;
                            }
                        case 7:
                            {
                                status = (byte)(joy2KeyRight ? 1 : 0);
                                break;
                            }
                    }

                    joy2NextInputRead = (joy2NextInputRead + 1) % 8;

                    return (byte)(status | 0x40);
                }
            }

            return 0x01;
        }

        public void setJoy1KeyInput(int keyCode, bool enabled)
        {

            switch (keyCode)
            {
                case 0:
                    {
                        joy1KeyUp = enabled;
                        break;
                    }
                case 1:
                    {
                        joy1KeyDown = enabled;
                        break;
                    }
                case 2:
                    {
                        joy1KeyLeft = enabled;
                        break;
                    }
                case 3:
                    {
                        joy1KeyRight = enabled;
                        break;
                    }
                case 4:
                    {
                        joy1KeyStart = enabled;
                        break;
                    }
                case 5:
                    {
                        joy1KeySelect = enabled;
                        break;
                    }
                case 6:
                    {
                        joy1KeyA = enabled;
                        break;
                    }
                case 7:
                    {
                        joy1KeyB = enabled;
                        break;
                    }
            }
        }

        public void resetInput()
        {
            joy1NextInputRead = 0;
            joy1Strobing = false;
            joy1PreviousStrobeValue = false;
            joy1Connected = true;
            joy1KeyUp = false;
            joy1KeyDown = false;
            joy1KeyLeft = false;
            joy1KeyRight = false;
            joy1KeyStart = false;
            joy1KeySelect = false;
            joy1KeyA = false;
            joy1KeyB = false;

            joy2NextInputRead = 0;
            joy2Strobing = false;
            joy2PreviousStrobeValue = false;
            joy2Connected = true;
            joy2KeyUp = false;
            joy2KeyDown = false;
            joy2KeyLeft = false;
            joy2KeyRight = false;
            joy2KeyStart = false;
            joy2KeySelect = false;
            joy2KeyA = false;
            joy2KeyB = false;
        }
    }
}
