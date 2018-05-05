using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes
{
    static class Core
    {
        static Core()
        {
            clocks = 0;

            run = true;
            paused = false;
            step = false;
        }

        public static int clocks;

        public static bool run;
        public static bool paused;
        public static bool step;
    }

    public static class NEST_Main
    {
        public static void startEmulator()
        {

            while(Core.run)
            {
                if (!Core.paused || Core.step)
                {

                }
            }
        }
    }
}
