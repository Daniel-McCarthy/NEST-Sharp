using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Core = NEST.Classes.Core;
using NEST = NEST.Classes;

namespace NEST
{
    public partial class AssemblyView : Form
    {
        
        public AssemblyView()
        {
            InitializeComponent();

            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += new DrawItemEventHandler(listBox1_DrawItem);
        }

        void updateDisplayValues()
        { 
            accumulatorValueLabel.Text = Core.cpu.getAccumulator().ToString("X2");
            xValueLabel.Text = Core.cpu.getXRegister().ToString("X2");
            yValueLabel.Text = Core.cpu.getYRegister().ToString("X2");
            statusRegisterValueLabel.Text = Core.cpu.getStatus().ToString("X2");

        }
        
        void updatePCValues()
        {
            pcValue.Text = Core.cpu.getProgramCounter().ToString("X4");
        }


        void updateAssemblyDisplay()
        {
            short address = (short)Core.cpu.getProgramCounter();

            listBox1.Items.Clear();

            for (int i = 0; i < 16; i++)
            {
                if (address < short.MaxValue)
                {
                    Tuple<string,int> disassembly = disassembleAddress(address);
                    listBox1.Items.Add(disassembly.Item1);
                    address += (short)disassembly.Item2;
                }
            }


            listBox1.SelectedIndex = 0;
        }

        
        void updateFlagDisplay()
        {
            byte flagRegister = Core.cpu.getStatus();

            byte flagC = (byte)((flagRegister & 0x01));
            byte flagZ = (byte)((flagRegister & 0x02) >> 1);
            byte flagI = (byte)((flagRegister & 0x04) >> 2);
            byte flagD = (byte)((flagRegister & 0x08) >> 3);
            byte flagB = (byte)((flagRegister & 0x10) >> 4);
            byte flagV = (byte)((flagRegister & 0x40) >> 6);
            byte flagS = (byte)((flagRegister & 0x80) >> 7);

            cFlagValue.Text = flagC.ToString();
            zFlagValue.Text = flagZ.ToString();
            iFlagValue.Text = flagI.ToString();
            dFlagValue.Text = flagD.ToString();
            bFlagValue.Text = flagB.ToString();
            vFlagValue.Text = flagV.ToString();
            sFlagValue.Text = flagS.ToString();
        }

        void updateStackDisplay()
        {
            byte stackPointer = Core.cpu.getStackPointer();
            short data = Core.cpu.readCPURam(stackPointer, true);

            spValue.Text = stackPointer.ToString("X2");
            stackValue.Text = data.ToString("X2");
            
        }

        /*
        void updateInterruptDisplay()
        {
            imeValueLabel.Text = (Core.beakCPU.interruptsEnabled) ? 1.ToString("X2") : 0.ToString("X2");
            ieValueLabel.Text = Core.beakMemory.readMemory(0xFFFF).ToString("X2");
            ifValueLabel.Text = Core.beakMemory.readMemory(0xFF0F).ToString("X2");
        }*/

            /*
        void updateLCDValuesDisplay()
        {
            
            lcdcValueLabel.Text = Core.beakGPU.getLCDControl().ToString("X2");
            lcdStatValueLabel.Text = Core.beakGPU.getLCDStatus().ToString("X2");
            lcdLYValueLabel.Text = Core.beakGPU.getLCDLY().ToString("X2");
            lcdModeValueLabel.Text = Core.beakGPU.getLCDMode().ToString("X2");
            scrollXValueLabel.Text = Core.beakGPU.getScrollX().ToString("X2");
            scrollYValueLabel.Text = Core.beakGPU.getScrollY().ToString("X2");
        }*/

        private void stepButton_Click(object sender, EventArgs e)
        {
            
            //Call function in DLL to step emulator
            //NativeMethods.setStep();
            Core.step = true;

            System.Threading.Thread.Sleep(10);

            updateDisplayValues();
            updatePCValues();
            updateFlagDisplay();
            updateAssemblyDisplay();
            updateStackDisplay();
            //updateInterruptDisplay();
            //updateLCDValuesDisplay();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(10);

            updateDisplayValues();
            updatePCValues();
            updateFlagDisplay();
            updateAssemblyDisplay();
            updateStackDisplay();
            //updateInterruptDisplay();
            //updateLCDValuesDisplay();
        }

        private void AssemblyView_Load(object sender, EventArgs e)
        {
            
            updateDisplayValues();
            updatePCValues();
            updateFlagDisplay();
            updateAssemblyDisplay();
            updateStackDisplay();
            //updateInterruptDisplay();
            //updateLCDValuesDisplay();

            listBox1.SelectedIndex = 0;
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            
            Core.paused = true;

            System.Threading.Thread.Sleep(10);

            updateDisplayValues();
            updatePCValues();
            updateFlagDisplay();
            updateAssemblyDisplay();
            updateStackDisplay();
            //updateInterruptDisplay();
            //updateLCDValuesDisplay();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            
            Core.paused = false;
            Core.step = true;

            System.Threading.Thread.Sleep(10);

            updateDisplayValues();
            updatePCValues();
            updateFlagDisplay();
            updateAssemblyDisplay();
            updateStackDisplay();
            //updateInterruptDisplay();
            //updateLCDValuesDisplay();
        }

        
        private Tuple<string, int> disassembleAddress(short address)
        {

            byte opcode = Core.cpu.readCPURam((ushort)address, true);
            string opcodeString = "";
            int bytesRead = 0;


            switch (opcode & 0xF0)
            {
                case 0x00:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    opcodeString = "00 brk";
                                    bytesRead = 1;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "01 " + argumentHex + " ora, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "05 " + argumentHex + " ora, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "06 " + argumentHex + " asl, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "08 " + " php";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "09 " + argumentHex + " ora, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "0A asl";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "0D " + argumentHex + " ora, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "0E " + argumentHex + " asl, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x10:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "10 " + argumentHex + " bpl, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "11 " + argumentHex + " ora, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "15 " + argumentHex + " ora, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "11 " + argumentHex + " asl, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "17 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "18 clc";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "19 " + argumentHex + " ora, " + argumentHex;
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "1A Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "1B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    opcodeString = "1C Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "1D " + argumentHex + " ora, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "1E " + argumentHex + " asl, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "1F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x20:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "20 " + argumentHex + " jsr, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "21 " + argumentHex + " and, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "22 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "23 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "24 " + argumentHex + " bit, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "25 " + argumentHex + " and, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "26 " + argumentHex + " rol, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "27 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "28 plp";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "29 " + argumentHex + " asl, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "2A rol a";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "2B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "2C " + argumentHex + " bit, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "2D " + argumentHex + " and, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "2E " + argumentHex + " rol, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "2F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x30:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "30 " + argumentHex + " bmi, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "31 " + argumentHex + " and, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "32 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "33 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    opcodeString = "34 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "35 " + argumentHex + " and, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "36 " + argumentHex + " rol, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "37 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "38 sec";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "39 " + argumentHex + " and, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "3A Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "3B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    opcodeString = "3C Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "3D " + argumentHex + " and, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "3E " + argumentHex + " rol, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "3F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x40:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    opcodeString = "40 rti";
                                    bytesRead = 1;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "41 " + argumentHex + " eor, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "42 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "43 Illegal Opcode";
                                    bytesRead = 2;
                                    break;
                                }
                            case 4:
                                {
                                    opcodeString = "44 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "45 " + argumentHex + " eor, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "46 " + argumentHex + " lsr, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "47 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "48 pha";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "49 " + argumentHex + " eor, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "4A lsr a";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "4B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "4C " + argumentHex + " jmp, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "4D " + argumentHex + " eor, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "4E " + argumentHex + " lsr, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "4F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x50:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "50 " + argumentHex + " bvc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "51 " + argumentHex + " eor, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "52 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "53 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    opcodeString = "54 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "55 " + argumentHex + " eor, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "56 " + argumentHex + " lsr, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "57 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "58 cli";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "59 " + argumentHex + " eor, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "5A Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "5B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    opcodeString = "5C Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "5D " + argumentHex + " eor, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "5E " + argumentHex + " lsr, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "5F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x60:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    opcodeString = "60 rts";
                                    bytesRead = 1;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "61 " + argumentHex + " adc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "62 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "63 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    opcodeString = "64 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "65 " + argumentHex + " adc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "66 " + argumentHex + " ror, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "67 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "68 pla";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "69 " + argumentHex + " adc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "6A ror a";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "6B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "6C " + argumentHex + " jmp, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "6D " + argumentHex + " adc, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "6E " + argumentHex + " ror, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "6F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x70:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "70 " + argumentHex + " bvs, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "71 " + argumentHex + " adc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "72 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "73 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    opcodeString = "74 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "75 " + argumentHex + " adc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "76 " + argumentHex + " ror, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "77 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "78 sei";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "79 " + argumentHex + " adc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "7A Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "7B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    opcodeString = "7C Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xD:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "7D " + argumentHex + " adc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xE:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "7E " + argumentHex + " ror, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "7F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x80:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    opcodeString = "80 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "81 " + argumentHex + " sta, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "82 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "83 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "84 " + argumentHex + " sty, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "85 " + argumentHex + " sta, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "86 " + argumentHex + " stx, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "87 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "88 dey";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    opcodeString = "89 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "8A txa";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "8B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "8C " + argumentHex + " sty, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "8D " + argumentHex + " sta, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "8E " + argumentHex + " stx, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "8F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0x90:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "90 " + argumentHex + " bcc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "91 " + argumentHex + " sta, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "92 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "93 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "94 " + argumentHex + " sty, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "95 " + argumentHex + " sta, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "96 " + argumentHex + " stx, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "97 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "98 tya";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "99 " + argumentHex + " sta, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "9A txs";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "9B Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    opcodeString = "9C Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "9D " + argumentHex + " sta, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    opcodeString = "9E Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "9F Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0xA0:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "A0 " + argumentHex + " ldy, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "A1 " + argumentHex + " lda, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "A2 " + argumentHex + " ldx, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "A3 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "A4 " + argumentHex + " ldy, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "A5 " + argumentHex + " lda, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "A6 " + argumentHex + " ldx, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "A7 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "A8 tay";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "A9 " + argumentHex + " lda, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "AA tax";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "AB Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "AC " + argumentHex + " ldy, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "AD " + argumentHex + " lda, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "AE " + argumentHex + " ldx, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "AF Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0xB0:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "B0 " + argumentHex + " bcs, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "B1 " + argumentHex + " lda, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "B2 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "B3 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "B4 " + argumentHex + " ldy, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "B5 " + argumentHex + " lda, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "B6 " + argumentHex + " ldx, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "B7 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "B8 clv";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "B9 " + argumentHex + " lda, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "BA tsx";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "BB Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "BC " + argumentHex + " ldy, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "BD " + argumentHex + " lda, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "BE " + argumentHex + " ldx, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "BF Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0xC0:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0x00:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "C0 " + argumentHex + " cpy, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0x01:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "C1 " + argumentHex + " cmp, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0x02:
                                {
                                    opcodeString = "C2 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0x03:
                                {
                                    opcodeString = "C3 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0x04:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "C4 " + argumentHex + " cpy, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0x05:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "C5 " + argumentHex + " cmp, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0x06:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "C6 " + argumentHex + " dec, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0x07:
                                {
                                    opcodeString = "C7 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0x08:
                                {
                                    opcodeString = "C8 iny";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0x09:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "C9 " + argumentHex + " cmp, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0x0A:
                                {
                                    opcodeString = "CA dex";
                                    bytesRead = 3;
                                    break;
                                }
                            case 0x0B:
                                {
                                    opcodeString = "CB Illegal Opcode";
                                    break;
                                }
                            case 0x0C:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "CC " + argumentHex + " cpy, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0x0D:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "CD " + argumentHex + " cmp, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0x0E:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "CE " + argumentHex + " dec, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0x0F:
                                {
                                    opcodeString = "CF Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;

                    }
                case 0xD0:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "D0 " + argumentHex + " bne, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "D1 " + argumentHex + " cmp, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "D2 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "D3 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    opcodeString = "D4 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "D5 " + argumentHex + " cmp, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "D6 " + argumentHex + " dec, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "D7 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    opcodeString = "D8 cld";
                                    bytesRead = 1;
                                    break;
                                }
                            case 9:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "D9 " + argumentHex + " cmp, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "DA Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "DB Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    opcodeString = "DC Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "DD " + argumentHex + " cmp, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "DE " + argumentHex + " dec, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "DF Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0xE0:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "E0 " + argumentHex + " cpx, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "E1 " + argumentHex + " sbc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "E2 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "E3 Illegal Opocde";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "E4 " + argumentHex + " cpx, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "E5 " + argumentHex + " sbc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "E6 " + argumentHex + " inc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "E7 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "E8 " + argumentHex + " inx, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 9:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "E9 " + argumentHex + " sbc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "EA nop";
                                    bytesRead = 2;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "EB Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "EC " + argumentHex + " cpx, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "ED " + argumentHex + " sbc, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "EE " + argumentHex + " inc, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "EF Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }
                case 0xF0:
                    {
                        switch (opcode & 0x0F)
                        {
                            case 0:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "F0 " + argumentHex + " beq, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 1:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "F1 " + argumentHex + " sbc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 2:
                                {
                                    opcodeString = "F2 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 3:
                                {
                                    opcodeString = "F3 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 4:
                                {
                                    opcodeString = "F4 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 5:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "F5 " + argumentHex + " sbc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 6:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "F6 " + argumentHex + " inc, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 7:
                                {
                                    opcodeString = "F7 Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 8:
                                {
                                    byte argument = Core.cpu.readCPURam((ushort)(address + 1), true);
                                    string argumentHex = argument.ToString("X2");
                                    opcodeString = "F8 " + argumentHex + " sed, " + argumentHex;
                                    bytesRead = 2;
                                    break;
                                }
                            case 9:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "F9 " + argumentHex + " sbc, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xA:
                                {
                                    opcodeString = "FA Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xB:
                                {
                                    opcodeString = "FB Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xC:
                                {
                                    opcodeString = "FC Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                            case 0xD:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "FD " + argumentHex + " sbc, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xE:
                                {
                                    ushort argument = (ushort)(Core.cpu.readCPURam((ushort)(address + 1), true) | (Core.cpu.readCPURam((ushort)(address + 2), true) << 8));
                                    string argumentHex = argument.ToString("X4");
                                    opcodeString = "FE " + argumentHex + " inc, " + argumentHex;
                                    bytesRead = 3;
                                    break;
                                }
                            case 0xF:
                                {
                                    opcodeString = "FF Illegal Opcode";
                                    bytesRead = 1;
                                    break;
                                }
                        }
                        break;
                    }


            }

            string addressHex = address.ToString("X4");
            opcodeString = addressHex + ":\t" + opcodeString;

            return new Tuple<string, int>(opcodeString, bytesRead);
        }


        public bool isBreakPointSet(string address)
        {
            return Core.breakpoints.ContainsKey(address);
        }

        public void removeBreakPoint(string address)
        {
            short parsedAddress = short.Parse(address, System.Globalization.NumberStyles.HexNumber);
            Core.breakpoints.TryRemove(address, out parsedAddress);
        }

        public void addBreakPoint(string address)
        {
            if(!isBreakPointSet(address))
            {
                Core.breakpoints.TryAdd(address ,short.Parse(address, System.Globalization.NumberStyles.HexNumber));
            }
        }

        //Double Click List Item // Toggle Breakpoint
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            
            string address = listBox1.SelectedItem.ToString().Substring(0, 4);

            if (isBreakPointSet(address))
            {
                removeBreakPoint(address);
            }
            else
            {
                addBreakPoint(address);
            }

            updateAssemblyDisplay();
        }

        //Draw Breakpoints/Disassembly Text
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            
            if (e.Index >= 0)
            {
                bool isEmpty = listBox1.Items[e.Index].ToString() == "";
                bool isBreakpoint = isBreakPointSet(listBox1.Items[e.Index].ToString().Substring(0, 4));

                if (!isEmpty && isBreakpoint)
                {
                    e.Graphics.FillRectangle(Brushes.Red, e.Bounds);
                }
                else
                {
                    e.DrawBackground();
                }

                using (Brush textBrush = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, textBrush, e.Bounds.Location);
                }
            }
        }

        //Reset Game
        private void resetButton_Click(object sender, EventArgs e)
        {
            /*
            //Needs to reload rom in case a different bank is loaded

            Core.paused = true;
            Core.beakWindow.resetScreens();
            Core.beakMemory.initializeGameBoyValues();
            Core.beakMemory.readRomHeader();
            Core.beakMemory.memoryPointer = 0x100;

            Core.beakCPU.interrupt = false;
            Core.beakCPU.halt = false;
            Core.beakCPU.stop = false;
            Core.repeat = false;
            Core.beakCPU.interruptsEnabled = true;
            Core.beakCPU.tClock = 0;
            Core.beakCPU.mClock = 0;

            //May need to reset more values in memory, like rom bank values, and perhaps some gpu values

            Core.paused = false;*/
        }
        
    }
}
