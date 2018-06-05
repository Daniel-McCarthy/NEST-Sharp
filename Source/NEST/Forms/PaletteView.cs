using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SFML.Graphics;
using Core = NEST.Classes.Core;
using NEST = NEST.Classes;

namespace NEST.Forms
{
    public partial class PaletteView : Form
    {
        public Color[] defaultPalette   = { new Color(0x7C, 0x7C, 0x7C), new Color(0x00, 0x34, 0x9C), new Color(0x00, 0x0B, 0xC3), new Color(0x34, 0x09, 0xC0), new Color(0x81, 0x06, 0x97), new Color(0xAE, 0x01, 0x49), new Color(0xBA, 0x00, 0x00), new Color(0x9D, 0x00, 0x00), new Color(0x50, 0x25, 0x00), new Color(0x00, 0x44, 0x00), new Color(0x00, 0x53, 0x00), new Color(0x00, 0x54, 0x00), new Color(0x00, 0x4B, 0x52), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xD1, 0xD1, 0xD1), new Color(0x00, 0x87, 0xF7), new Color(0x20, 0x5D, 0xFF), new Color(0x84, 0x3D, 0xFF), new Color(0xE8, 0x22, 0xF0), new Color(0xFF, 0x24, 0x98), new Color(0xFF, 0x34, 0x2A), new Color(0xF5, 0x4D, 0x00), new Color(0xA6, 0x71, 0x00), new Color(0x40, 0x8F, 0x00), new Color(0x00, 0xA1, 0x00), new Color(0x00, 0xA5, 0x2E), new Color(0x00, 0x9D, 0xA5), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xFE, 0xFF, 0xFF), new Color(0x36, 0xB5, 0xFF), new Color(0x68, 0x8F, 0xFF), new Color(0xA7, 0x7D, 0xFF), new Color(0xF8, 0x7C, 0xFF), new Color(0xFF, 0x8A, 0xD2), new Color(0xFF, 0x8E, 0x89), new Color(0xFF, 0xA6, 0x40), new Color(0xFF, 0xC9, 0x05), new Color(0xB1, 0xE3, 0x00), new Color(0x5B, 0xF6, 0x35), new Color(0x1E, 0xFD, 0x9C), new Color(0x13, 0xF6, 0xFD), new Color(0x5E, 0x5E, 0x5E), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xFE, 0xFF, 0xFF), new Color(0xAE, 0xE0, 0xFF), new Color(0xBC, 0xCD, 0xFF), new Color(0xD6, 0xC5, 0xFF), new Color(0xFB, 0xCA, 0xFF), new Color(0xFF, 0xCF, 0xED), new Color(0xFF, 0xD1, 0xCF), new Color(0xFF, 0xDD, 0xB9), new Color(0xFF, 0xF2, 0xB0), new Color(0xE7, 0xFA, 0xAE), new Color(0xC6, 0xFC, 0xBD), new Color(0xB3, 0xFE, 0xDC), new Color(0xB2, 0xFE, 0xFF), new Color(0xDA, 0xDA, 0xDA), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00) };
        public List<PictureBox> pictureBoxes;
        public List<SFML.Graphics.Color[]> palettes = new List<SFML.Graphics.Color[]>();
        public List<string> paletteNames = new List<string>();

        public PaletteView()
        {
            InitializeComponent();
            pictureBoxes = new List<PictureBox> { color0PictureBox, color1PictureBox, color2PictureBox, color3PictureBox, color4PictureBox, color5PictureBox, color6PictureBox, color7PictureBox, color8PictureBox, color9PictureBox, color10PictureBox, color11PictureBox, color12PictureBox, color13PictureBox, color14PictureBox, color15PictureBox, color16PictureBox, color17PictureBox, color18PictureBox, color19PictureBox, color20PictureBox, color21PictureBox, color22PictureBox, color23PictureBox, color24PictureBox, color25PictureBox, color26PictureBox, color27PictureBox, color28PictureBox, color29PictureBox, color30PictureBox, color31PictureBox, color32PictureBox, color33PictureBox, color34PictureBox, color35PictureBox, color36PictureBox, color37PictureBox, color38PictureBox, color39PictureBox, color40PictureBox, color41PictureBox, color42PictureBox, color43PictureBox, color44PictureBox, color45PictureBox, color46PictureBox, color47PictureBox, color48PictureBox, color49PictureBox, color50PictureBox, color51PictureBox, color52PictureBox, color53PictureBox, color54PictureBox, color55PictureBox, color56PictureBox, color57PictureBox, color58PictureBox, color59PictureBox, color60PictureBox, color61PictureBox, color62PictureBox, color63PictureBox };
            addPalette("Default Palette", defaultPalette);
        }

        public void displayPalette(int paletteNumber)
        {
            for(int i = 0; i < pictureBoxes.Count; i++)
            {
                SFML.Graphics.Color paletteColor = palettes[paletteNumber][i];
                pictureBoxes[i].BackColor = System.Drawing.Color.FromArgb(paletteColor.R, paletteColor.G, paletteColor.B);
            }
        }

        private void palettesListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if(palettesListBox.SelectedIndex >= 0)
            {
                displayPalette(palettesListBox.SelectedIndex);

                setPaletteButton.Enabled = true;
            }
            else
            {
                foreach(PictureBox colorBox in pictureBoxes)
                {
                    colorBox.BackColor = System.Drawing.Color.FromName("Control");
                }

                setPaletteButton.Enabled = false;
            }
        }

        public void addPalette(string name, SFML.Graphics.Color[] palette)
        {
            palettes.Add(palette);
            paletteNames.Add(name);
            palettesListBox.Items.Add(paletteNames[paletteNames.Count - 1]);
        }

        private void setPaletteButton_Click(object sender, EventArgs e)
        {
            int selectedPaletteNumber = palettesListBox.SelectedIndex;

            int length = palettes[selectedPaletteNumber].Length;
            for(int i = 0; i < length; i++)
            {
                Core.ppu.palette[i] = palettes[selectedPaletteNumber][i];
            }
        }
    }
}
