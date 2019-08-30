using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using Core = NEST.Classes.Core;

using System.Threading;
using NEST.Classes;
using NEST.Classes.Mappers;
using NEST.Forms;

namespace NEST
{
    public partial class MainWindow : Form
    {
        public Thread emulatorThread;

        private AssemblyView assemblyView;
        private MemoryView memoryView;
        private PaletteView paletteView;

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

            bool alreadyEmulating = Core.run;
            Core.paused = true;
            string filePath = (openFileDialog1.ShowDialog() == DialogResult.OK) ? openFileDialog1.FileName : "Error: No such file found.";

            if(File.Exists(filePath))
            {
                if (alreadyEmulating)
                {
                    global::NEST.Classes.NEST_Main.resetCore();
                }

                Core.rom = new Rom(File.ReadAllBytes(filePath));
                Core.rom.romFilePath = filePath;

                int mapperSetting = Core.rom.getMapperSetting();

                if (mapperSetting == 0)
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
                else if (mapperSetting == 3)
                {
                    CNROM.loadRom(Core.rom);
                }
                else if (mapperSetting == 4)
                {
                    MMC3.loadRom(Core.rom);
                }

                ushort resetAddress = 0;
                resetAddress |= Core.cpu.readCPURam(0xFFFC, true);
                resetAddress |= (ushort)(Core.cpu.readCPURam(0xFFFD, true) << 8);
                Core.cpu.programCounter = resetAddress;

                if (emulatorThread == null)
                {
                    emulatorThread = new Thread(global::NEST.Classes.NEST_Main.startEmulator);
                    emulatorThread.Start();
                }
                else
                {
                    Core.run = true;
                    Core.paused = false;
                }

                resumeToolStripMenuItem.Enabled = true;
                pauseToolStripMenuItem.Enabled = true;
                savesavFileToolStripMenuItem.Enabled = true;

                string saveFilePath = filePath.Substring(0, filePath.LastIndexOf('.')) + ".sav";
                if (File.Exists(saveFilePath))
                {
                    loadSaveFile(saveFilePath);
                }
            }
            else
            {
                MessageBox.Show("File not found at path: " + filePath);
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {

            //Tell the thread to stop when it finishes it's current loop
            Core.run = false;

            int loops = 0;
            while(emulatorThread != null && emulatorThread.IsAlive)
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

        public bool createSaveFile(string fileName, bool overwrite)
        {

            int mapperSetting = Core.rom.getMapperSetting();
            bool romUsesRam = mapperSetting == Mapper.NROM_ID || mapperSetting == Mapper.MMC1_ID || mapperSetting == Mapper.MMC3_ID || mapperSetting == Mapper.MMC5_ID;

            if (romUsesRam)
            {
                byte[] saveData = returnSaveDataFromMemory();

                string savePath = Core.rom.romFilePath.Substring(0, Core.rom.romFilePath.LastIndexOf('.')) + ".sav";
                bool fileExists = File.Exists(savePath);

                if (!fileExists || overwrite)
                {
                    File.WriteAllBytes(savePath, saveData);
                    return true;
                }
            }

            return false;
        }

        public byte[] returnSaveDataFromMemory()
        {
            byte[] memory = new byte[0x2000];

            ushort address = 0x6000;
            for (int i = 0; i <= 0x1FFF; i++)
            {
                memory[i] = Core.cpu.readCPURam((ushort)(address + i), true);
            }

            return memory;
        }

        bool loadSaveFile(string filepath)
        {
            byte[] saveFile = File.ReadAllBytes(filepath);

            if (saveFile != null)
            {
                int fileLength = saveFile.Length;

                if (fileLength >= 0x2000)
                {
                    ushort address = 0x6000;
                    for (ushort i = 0x0; i <= 0x1FFF; i++)
                    {
                        Core.cpu.directCPURamWrite((ushort)(address + i), saveFile[i]);
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Error: Save file does not exist.");
                return false;
            }

            return true;
        }

        private void savesavFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Core.rom.romFilePath != "")
            {
                createSaveFile(Core.rom.romFilePath, true);
            }
        }

		private void paletteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (paletteView == null || paletteView.IsDisposed)
            {
				paletteView = new PaletteView();
				paletteView.Show();
            }
            else
            {
				paletteView.Show();
				paletteView.BringToFront();
            }
		}
    }
}
