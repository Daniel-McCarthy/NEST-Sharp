using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        public List<PictureBox> pictureBoxes;
        public List<SFML.Graphics.Color[]> palettes = new List<SFML.Graphics.Color[]>();
        public List<string> paletteNames = new List<string>();

        public PaletteView()
        {
            InitializeComponent();
            pictureBoxes = new List<PictureBox> { color0PictureBox, color1PictureBox, color2PictureBox, color3PictureBox, color4PictureBox, color5PictureBox, color6PictureBox, color7PictureBox, color8PictureBox, color9PictureBox, color10PictureBox, color11PictureBox, color12PictureBox, color13PictureBox, color14PictureBox, color15PictureBox, color16PictureBox, color17PictureBox, color18PictureBox, color19PictureBox, color20PictureBox, color21PictureBox, color22PictureBox, color23PictureBox, color24PictureBox, color25PictureBox, color26PictureBox, color27PictureBox, color28PictureBox, color29PictureBox, color30PictureBox, color31PictureBox, color32PictureBox, color33PictureBox, color34PictureBox, color35PictureBox, color36PictureBox, color37PictureBox, color38PictureBox, color39PictureBox, color40PictureBox, color41PictureBox, color42PictureBox, color43PictureBox, color44PictureBox, color45PictureBox, color46PictureBox, color47PictureBox, color48PictureBox, color49PictureBox, color50PictureBox, color51PictureBox, color52PictureBox, color53PictureBox, color54PictureBox, color55PictureBox, color56PictureBox, color57PictureBox, color58PictureBox, color59PictureBox, color60PictureBox, color61PictureBox, color62PictureBox, color63PictureBox };
            addPalette("Default Palette", Core.ppu.defaultPalette);
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
            }
            else
            {
                foreach(PictureBox colorBox in pictureBoxes)
                {
                    colorBox.BackColor = System.Drawing.Color.FromName("Control");
                }
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
