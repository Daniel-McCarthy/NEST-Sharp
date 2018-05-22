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

            TOTAL_CPU_CLOCKS = 0;
            TOTAL_PPU_CLOCKS = 0;

            run = true;
            paused = false;
            step = false;
        }


        public static MainWindow mainWindow;
        public static PPU ppu;
        public static CPU cpu;
        public static Input input;
        public static Rom rom;

        public static uint TOTAL_CPU_CLOCKS;
        public static uint TOTAL_PPU_CLOCKS;

        public static bool run;
        public static bool paused;
        public static bool step;

        public static bool breakPointEnabled = true;
        public static ConcurrentDictionary<string, ushort> breakpoints = new ConcurrentDictionary<string, ushort>();
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
