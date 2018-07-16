using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Core = NEST.Classes.Core;
using NEST = NEST.Classes;

namespace NEST
{

    public partial class MemoryView : Form
    {
        private ushort baseAddress = 0;
        private byte memorySourceSetting = CPU_SETTING;

        private const byte CPU_SETTING = 0;
        private const byte PPU_SETTING = 1;
        private const byte OAM_SETTING = 2;
        private const byte ROM_SETTING = 3;

        public MemoryView()
        {
            InitializeComponent();
        }

        private void MemoryView_Load(object sender, EventArgs e)
        {
            memoryViewComboBox.SelectedIndex = 0;
        }

        private void loadMemory()
        {
            hexTextBox.Text = "";

            for(int y = 0; y < 16; y++)
            {
                hexTextBox.Text += (baseAddress + (y * 16)).ToString("X4") + ':';

                for(int x = 0; x < 16; x++)
                {
                    if (x < 16)
                    {
                        hexTextBox.Text += " ";
                    }

                    byte value = 0;

                    if (memorySourceSetting == CPU_SETTING)
                    {
                        if ((baseAddress + (y * 16) + x) < 0x10000)
                        {
                            value = Core.cpu.readCPURam((ushort)(baseAddress + (y * 16) + x), true);
                        }
                    }
                    else if (memorySourceSetting == PPU_SETTING)
                    {
                        if ((baseAddress + (y * 16) + x) < 0x4000)
                        {
                            value = Core.ppu.readPPURamByte((ushort)(baseAddress + (y * 16) + x));
                        }
                    }
                    else if (memorySourceSetting == OAM_SETTING)
                    {
                        if ((baseAddress + (y * 16) + x) < 0x100)
                        {
                            value = Core.ppu.readOAMRamByte((ushort)(baseAddress + (y * 16) + x));
                        }
                    }
                    else if (memorySourceSetting == ROM_SETTING)
                    {
                        if (Core.rom != null)
                        {
                            if ((baseAddress + (y * 16) + x) < Core.rom.getExactDataLength())
                            {
                                value = Core.rom.readByte((ushort)(baseAddress + (y * 16) + x));
                            }
                        }
                    }

                    hexTextBox.Text += value.ToString("X2");

                }

                hexTextBox.Text += '\n';
            }
        }
        
        private void refreshButton_Click(object sender, EventArgs e)
        {
            loadMemory();
        }

        private void thousandthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            baseAddress &= 0x0FFF;
            baseAddress |= (ushort)((int)thousandthNumericUpDown.Value << 12);

            loadMemory();
        }

        private void hundredthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            baseAddress &= 0xF0FF;
            baseAddress |= (ushort)((int)hundredthNumericUpDown.Value << 8);

            loadMemory();
        }

        private void memoryViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            memorySourceSetting = (byte)memoryViewComboBox.SelectedIndex;

            if(memorySourceSetting == CPU_SETTING)
            {
                thousandthNumericUpDown.Enabled = true;
                thousandthNumericUpDown.Maximum = 0xF;
                hundredthNumericUpDown.Enabled = true;
                hundredthNumericUpDown.Maximum = 0xF;
            }
            else if (memorySourceSetting == PPU_SETTING)
            {
                thousandthNumericUpDown.Enabled = true;
                thousandthNumericUpDown.Maximum = 0x3;
                hundredthNumericUpDown.Enabled = true;
                hundredthNumericUpDown.Maximum = 0xF;
            }
            else if (memorySourceSetting == OAM_SETTING)
            {
                thousandthNumericUpDown.Enabled = false;
                hundredthNumericUpDown.Enabled = false;
            }
            else if (memorySourceSetting == ROM_SETTING)
            {
                thousandthNumericUpDown.Enabled = true;
                thousandthNumericUpDown.Maximum = 0xF;
                hundredthNumericUpDown.Enabled = true;
                hundredthNumericUpDown.Maximum = 0xF;
            }

            thousandthNumericUpDown.Value = 0;
            hundredthNumericUpDown.Value = 0;

            loadMemory();
        }
    }
}
