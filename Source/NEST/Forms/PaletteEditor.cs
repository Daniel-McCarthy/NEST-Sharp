using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEST.Forms
{
    public partial class PaletteEditor : Form
    {
        public List<PictureBox> pictureBoxes;
        private int selectedPictureBoxIndex = -1;

        public PaletteEditor()
        {
            InitializeComponent();

            pictureBoxes = new List<PictureBox> { color0PictureBox, color1PictureBox, color2PictureBox, color3PictureBox, color4PictureBox, color5PictureBox, color6PictureBox, color7PictureBox, color8PictureBox, color9PictureBox, color10PictureBox, color11PictureBox, color12PictureBox, color13PictureBox, color14PictureBox, color15PictureBox, color16PictureBox, color17PictureBox, color18PictureBox, color19PictureBox, color20PictureBox, color21PictureBox, color22PictureBox, color23PictureBox, color24PictureBox, color25PictureBox, color26PictureBox, color27PictureBox, color28PictureBox, color29PictureBox, color30PictureBox, color31PictureBox, color32PictureBox, color33PictureBox, color34PictureBox, color35PictureBox, color36PictureBox, color37PictureBox, color38PictureBox, color39PictureBox, color40PictureBox, color41PictureBox, color42PictureBox, color43PictureBox, color44PictureBox, color45PictureBox, color46PictureBox, color47PictureBox, color48PictureBox, color49PictureBox, color50PictureBox, color51PictureBox, color52PictureBox, color53PictureBox, color54PictureBox, color55PictureBox, color56PictureBox, color57PictureBox, color58PictureBox, color59PictureBox, color60PictureBox, color61PictureBox, color62PictureBox, color63PictureBox };

        }

        public void loadPalette(SFML.Graphics.Color[] palette)
        {
            for (int i = 0; i < pictureBoxes.Count; i++)
            {
                SFML.Graphics.Color paletteColor = palette[i];
                pictureBoxes[i].BackColor = System.Drawing.Color.FromArgb(paletteColor.R, paletteColor.G, paletteColor.B);
            }
        }

        private void colorPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            
            //Change style of clicked picture box to show it is selected
            PictureBox senderPictureBox = (PictureBox)sender;
            int pictureBoxNumber = pictureBoxes.IndexOf(senderPictureBox);

            // Remove border style from previously selected picture box.
            if (selectedPictureBoxIndex != -1)
            {
                if (selectedPictureBoxIndex < pictureBoxes.Count)
                {
                    pictureBoxes[selectedPictureBoxIndex].BorderStyle = BorderStyle.None;
                }
            }

            selectedPictureBoxIndex = pictureBoxNumber;

            // Add border style to newly selected picture box.
            if (selectedPictureBoxIndex >= 0 && selectedPictureBoxIndex < pictureBoxes.Count)
            {
                pictureBoxes[selectedPictureBoxIndex].BorderStyle = BorderStyle.Fixed3D;
            }

            displayColor(pictureBoxes[selectedPictureBoxIndex].BackColor);

            if (e.Button == MouseButtons.Right)
            {
                //Set right clicked color picture box to new selected color.
                ColorDialog colorDialogue = new ColorDialog();

                if (colorDialogue.ShowDialog() == DialogResult.OK)
                {
                    ((PictureBox)sender).BackColor = colorDialogue.Color;
                }
            }
        }

        private void displayColor(Color color)
        {
            rValueSlider.Value = color.R;
            gValueSlider.Value = color.G;
            bValueSlider.Value = color.B;

            rValueTextbox.Text = color.R.ToString("X2");
            gValueTextbox.Text = color.G.ToString("X2");
            bValueTextbox.Text = color.B.ToString("X2");

            colorPreviewPictureBox.BackColor = color;
        }

        private void rValueSlider_ValueChanged(object sender, EventArgs e)
        {
            if (selectedPictureBoxIndex != -1)
            {
                Color color = pictureBoxes[selectedPictureBoxIndex].BackColor;
                byte newRValue = (byte)(((TrackBar)sender).Value);

                color = Color.FromArgb(newRValue, color.G, color.B);

                pictureBoxes[selectedPictureBoxIndex].BackColor = color;
                displayColor(color);
            }
        }

        private void gValueSlider_ValueChanged(object sender, EventArgs e)
        {
            if (selectedPictureBoxIndex != -1)
            {
                Color color = pictureBoxes[selectedPictureBoxIndex].BackColor;
                byte newGValue = (byte)(((TrackBar)sender).Value);

                color = Color.FromArgb(color.R, newGValue, color.B);

                pictureBoxes[selectedPictureBoxIndex].BackColor = color;
                displayColor(color);
            }
        }

        private void bValueSlider_ValueChanged(object sender, EventArgs e)
        {
            if (selectedPictureBoxIndex != -1)
            {
                Color color = pictureBoxes[selectedPictureBoxIndex].BackColor;
                byte newBValue = (byte)(((TrackBar)sender).Value);

                color = Color.FromArgb(color.R, color.G, newBValue);

                pictureBoxes[selectedPictureBoxIndex].BackColor = color;
                displayColor(color);
            }
        }

        public SFML.Graphics.Color[] returnPalette()
        {
            int length = pictureBoxes.Count;
            SFML.Graphics.Color[] palette = new SFML.Graphics.Color[length];

            for(int i = 0; i < length; i++)
            {
                Color storedColor = pictureBoxes[i].BackColor;
                SFML.Graphics.Color convertedColor = new SFML.Graphics.Color(storedColor.R, storedColor.G, storedColor.B);
                palette[i] = convertedColor;
            }

            return palette;
        }
    }
}
