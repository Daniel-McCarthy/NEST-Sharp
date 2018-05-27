using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using Core = NEST.Classes.Core;

using System.Threading;
using NEST.Classes;
using NEST.Classes.Mappers;

namespace NEST
{
    public partial class MainWindow : Form
    {
        private Thread emulatorThread;

        private AssemblyView assemblyView;
        private MemoryView memoryView;

        public Classes.Canvas drawCanvas = new Classes.Canvas();
        public SFML.Graphics.Sprite frame;

        public MainWindow()
        {
            InitializeComponent();

            //Add drawing canvas to screen
            Controls.Add(drawCanvas);
            drawCanvas.Location = new Point(0, 27);

        }

        
        /*
         *  Load Rom File 
        */
        
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            Core.paused = true;
            string filePath = (openFileDialog1.ShowDialog() == DialogResult.OK) ? openFileDialog1.FileName : "Error: No such file found.";

            if(File.Exists(filePath))
            {
                Core.rom = new Rom(File.ReadAllBytes(filePath));

                int mapperSetting = Core.rom.getMapperSetting();

                if(mapperSetting == 0)
                {
                    NROM.loadRom(Core.rom);
                }
                else if (mapperSetting == 1)
                {
                    MMC1.loadRom(Core.rom);
                }
                else if (mapperSetting == 2)
                {
                    UNROM.loadRom(Core.rom);
                }

                ushort resetAddress = 0;
                resetAddress |= Core.cpu.readCPURam(0xFFFC, true);
                resetAddress |= (ushort)(Core.cpu.readCPURam(0xFFFD, true) << 8);
                Core.cpu.programCounter = resetAddress;

                //Core.beakMemory.initializeGameBoyValues();
                //Core.beakMemory.readRomHeader();

                if (emulatorThread == null)
                {
                    emulatorThread = new Thread(global::NEST.Classes.NEST_Main.startEmulator);
                    emulatorThread.Start();
                }

                resumeToolStripMenuItem.Enabled = true;
                pauseToolStripMenuItem.Enabled = true;

            }
            else
            {
                MessageBox.Show(filePath);
                //Core.beakMemory.romFilePath = "";
            }
        }
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {

            //Tell the thread to stop when it finishes it's current loop
            Core.run = false;

            int loops = 0;
            while(emulatorThread.IsAlive)
            {
                //Attempt to wait for the thread to be ready to be stopped.
                if(emulatorThread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    Thread.Sleep(800);
                    emulatorThread.Abort();
                }

                //If the thread is not exiting after several attempts to check, just end it.
                if (loops++ > 20)
                {
                    emulatorThread.Abort();
                }

                //Wait before trying again
                Thread.Sleep(100);

            }
        }

        private void assemblyViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (assemblyView == null || assemblyView.IsDisposed)
            {
                assemblyView = new AssemblyView();
                assemblyView.Show();
            }
            else
            {
                assemblyView.Show();
                assemblyView.BringToFront();
            }
        }

        private void memoryViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (memoryView == null || memoryView.IsDisposed)
            {
                memoryView = new MemoryView();
                memoryView.Show();
            }
            else
            {
                memoryView.Show();
                memoryView.BringToFront();
            }
        }

        private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Core.paused = false;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Core.paused = true;
        }
    }

}
