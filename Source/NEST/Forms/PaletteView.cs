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
        public Color[] nesPalette       = { new Color(0x52, 0x52, 0x52), new Color(0x01, 0x1A, 0x51), new Color(0x0F, 0x0F, 0x65), new Color(0x23, 0x06, 0x63), new Color(0x36, 0x03, 0x4B), new Color(0x40, 0x04, 0x26), new Color(0x3F, 0x09, 0x04), new Color(0x32, 0x13, 0x00), new Color(0x1F, 0x20, 0x00), new Color(0x0B, 0x2A, 0x00), new Color(0x00, 0x2F, 0x00), new Color(0x00, 0x2E, 0x0A), new Color(0x00, 0x26, 0x2D), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xA0, 0xA0, 0xA0), new Color(0x1E, 0x4A, 0x9D), new Color(0x38, 0x37, 0xBC), new Color(0x58, 0x28, 0xB8), new Color(0x75, 0x21, 0x94), new Color(0x84, 0x23, 0x5C), new Color(0x82, 0x2E, 0x24), new Color(0x6F, 0x3F, 0x00), new Color(0x51, 0x52, 0x00), new Color(0x31, 0x63, 0x00), new Color(0x1A, 0x6B, 0x05), new Color(0x0E, 0x69, 0x2E), new Color(0x10, 0x5C, 0x68), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xFE, 0xFF, 0xFF), new Color(0x69, 0x9E, 0xFC), new Color(0x89, 0x87, 0xFF), new Color(0xAE, 0x76, 0xFF), new Color(0xCE, 0x6D, 0xF1), new Color(0xE0, 0x70, 0xB2), new Color(0xDE, 0x7C, 0x70), new Color(0xC8, 0x91, 0x3E), new Color(0xA6, 0xA7, 0x25), new Color(0x81, 0xBA, 0x28), new Color(0x63, 0xC4, 0x46), new Color(0x54, 0xC1, 0x7D), new Color(0x56, 0xB3, 0xC0), new Color(0x3C, 0x3C, 0x3C), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xFE, 0xFF, 0xFF), new Color(0xBE, 0xD6, 0xFD), new Color(0xCC, 0xCC, 0xFF), new Color(0xDD, 0xC4, 0xFF), new Color(0xEA, 0xC0, 0xF9), new Color(0xF2, 0xC1, 0xDF), new Color(0xF1, 0xC7, 0xC2), new Color(0xE8, 0xD0, 0xAA), new Color(0xD9, 0xDA, 0x9D), new Color(0xC9, 0xE2, 0x9E), new Color(0xBC, 0xE6, 0xAE), new Color(0xB4, 0xE5, 0xC7), new Color(0xB5, 0xDF, 0xE4), new Color(0xA9, 0xA9, 0xA9), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00) };
        public Color[] greyScalePalette = { new Color(0x3A, 0x3A, 0x3A), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xD6, 0xD6, 0xD6), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x20, 0x20, 0x20), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xFF, 0xFF, 0xFF), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0xC4, 0xC4, 0xC4), new Color(0x12, 0x12, 0x12), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xFF, 0xFF, 0xFF), new Color(0xE9, 0xE9, 0xE9), new Color(0x00, 0x00, 0x00), new Color(0x00, 0x00, 0x00) };


        public List<PictureBox> pictureBoxes;
        public List<SFML.Graphics.Color[]> palettes = new List<SFML.Graphics.Color[]>();
        public List<string> paletteNames = new List<string>();

        public PaletteView()
        {
            InitializeComponent();
            pictureBoxes = new List<PictureBox> { color0PictureBox, color1PictureBox, color2PictureBox, color3PictureBox, color4PictureBox, color5PictureBox, color6PictureBox, color7PictureBox, color8PictureBox, color9PictureBox, color10PictureBox, color11PictureBox, color12PictureBox, color13PictureBox, color14PictureBox, color15PictureBox, color16PictureBox, color17PictureBox, color18PictureBox, color19PictureBox, color20PictureBox, color21PictureBox, color22PictureBox, color23PictureBox, color24PictureBox, color25PictureBox, color26PictureBox, color27PictureBox, color28PictureBox, color29PictureBox, color30PictureBox, color31PictureBox, color32PictureBox, color33PictureBox, color34PictureBox, color35PictureBox, color36PictureBox, color37PictureBox, color38PictureBox, color39PictureBox, color40PictureBox, color41PictureBox, color42PictureBox, color43PictureBox, color44PictureBox, color45PictureBox, color46PictureBox, color47PictureBox, color48PictureBox, color49PictureBox, color50PictureBox, color51PictureBox, color52PictureBox, color53PictureBox, color54PictureBox, color55PictureBox, color56PictureBox, color57PictureBox, color58PictureBox, color59PictureBox, color60PictureBox, color61PictureBox, color62PictureBox, color63PictureBox };
            addPalette("Default Palette", defaultPalette);
            addPalette("NES Palette", nesPalette);
            addPalette("GreyScale Palette", greyScalePalette);
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
                editPaletteButton.Enabled = true;
                savePalButton.Enabled = true;
            }
            else
            {
                foreach(PictureBox colorBox in pictureBoxes)
                {
                    colorBox.BackColor = System.Drawing.Color.FromName("Control");
                }

                setPaletteButton.Enabled = false;
                editPaletteButton.Enabled = false;
                savePalButton.Enabled = false;
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


        private void newPaletteButton_Click(object sender, EventArgs e)
        {
            PaletteEditor editor = new PaletteEditor();
            editor.loadPalette(defaultPalette);

            if(editor.ShowDialog() == DialogResult.OK)
            {
                InputBox nameBox = new InputBox();
                nameBox.Text = "Enter Palette Name:";

                string paletteName = "New Palette";

                if(nameBox.ShowDialog() == DialogResult.OK)
                {
                    paletteName = nameBox.inputTextBox.Text;
                }

                addPalette(paletteName, editor.returnPalette());
            }
        }

        private void editPaletteButton_Click(object sender, EventArgs e)
        {
            PaletteEditor editor = new PaletteEditor();

            int selectedPaletteNumber = palettesListBox.SelectedIndex;
            editor.loadPalette(palettes[selectedPaletteNumber]);

            if (editor.ShowDialog() == DialogResult.OK)
            {
                palettes[selectedPaletteNumber] = editor.returnPalette();
            }
        }

        private void savePalButton_Click(object sender, EventArgs e)
        {
            int paletteNumber = palettesListBox.SelectedIndex;

            if (paletteNumber != -1)
            {

                using (SaveFileDialog saveDialogue = new SaveFileDialog())
                {
                    saveDialogue.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";

                    if (saveDialogue.ShowDialog() == DialogResult.OK)
                    {
                        byte[] paletteData = new byte[64 * 3];

                        for (int i = 0; i < 64; i++)
                        {
                            SFML.Graphics.Color color = palettes[paletteNumber][i];
                            paletteData[(i * 3)] = color.R;
                            paletteData[(i * 3) + 1] = color.G;
                            paletteData[(i * 3) + 2] = color.B;
                        }

                        System.IO.File.WriteAllBytes(saveDialogue.FileName, paletteData);

                        MessageBox.Show("Successfully Saved.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to save to selected path.");
                    }
                }
            }
            else
            {
                MessageBox.Show("No palette selected.");
            }
        }

        private void loadPalButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialogue = new OpenFileDialog())
            {
                openDialogue.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";

                if (openDialogue.ShowDialog() == DialogResult.OK)
                {
                    byte[] paletteData = System.IO.File.ReadAllBytes(openDialogue.FileName);

                    if(paletteData.Length >= (64 * 3))
                    {
                        SFML.Graphics.Color[] palette = new Color[64];

                        for(int i = 0; i < 64; i++)
                        {
                            SFML.Graphics.Color newColor = new SFML.Graphics.Color(paletteData[(i * 3)], paletteData[(i * 3) + 1], paletteData[(i * 3) + 2]);
                            palette[i] = newColor;
                        }

                        //Accept input for palette name
                        InputBox nameBox = new InputBox();
                        nameBox.Text = "Enter Palette Name:";

                        string paletteName = "New Palette";

                        if (nameBox.ShowDialog() == DialogResult.OK)
                        {
                            paletteName = nameBox.inputTextBox.Text;
                        }

                        addPalette(paletteName, palette);
                    }
                }
            }
        }
    }
}
