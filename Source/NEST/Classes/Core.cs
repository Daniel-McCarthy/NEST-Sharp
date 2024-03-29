﻿using System;
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

            run = false;
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

            while (Core.run)
            {

                if (Core.breakPointEnabled)
                { 
                    
                    if(Core.breakpoints.ContainsKey(Core.cpu.programCounter.ToString("X4")))
                    {
                        Core.paused = true;
                    }
                    
                }

                if (!Core.paused || Core.step)
                {


                    if ( Core.ppu.pendingNMI ) { Core.cpu.serviceNonMaskableInterrupt(); }
                    else { Core.cpu.serviceInterrupt(); }

                    Core.cpu.fetchAndExecute();

                    uint cpuClocks = Core.cpu.mClock;
                    uint ppuClocks = cpuClocks * 3;

                    Core.TOTAL_CPU_CLOCKS += cpuClocks;
                    Core.TOTAL_PPU_CLOCKS += ppuClocks;

                    Core.ppu.updatePPU(ref Core.TOTAL_PPU_CLOCKS);


                    Core.step = false;

                    Core.cpu.mClock = 0;
                    Core.cpu.tClock = 0;

                }
            }
        }


        public static void resetCore()
        {
            Core.TOTAL_CPU_CLOCKS = 0;
            Core.TOTAL_PPU_CLOCKS = 0;

            Core.paused = false;
            Core.step = false;

            Core.breakPointEnabled = true;
            Core.breakpoints = new ConcurrentDictionary<string, ushort>();

            // Don't reset rom since the rom class will be created again on new rom load.
            Core.cpu.resetCPU();
            Core.ppu.resetPPU();
            Core.input.resetInput();

            Mappers.MMC1.resetMMC1();
            Mappers.MMC3.resetMMC3();
        }
    }
}
