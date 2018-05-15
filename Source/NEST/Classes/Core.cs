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
            ppu = new PPU();
            cpu = new CPU();
            mainWindow = new MainWindow();
            input = new Input();
            clocks = 0;

            run = true;
            paused = false;
            step = false;
        }


        public static MainWindow mainWindow;
        public static PPU ppu;
        public static CPU cpu;
        public static Input input;
        public static Rom rom;

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

                    Core.cpu.fetchAndExecute();
                }
            }
        }
    }
}
