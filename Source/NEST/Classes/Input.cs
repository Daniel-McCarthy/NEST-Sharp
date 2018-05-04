using System;
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


    }
}
