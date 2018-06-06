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
        }


        private void rValueSlider_ValueChanged(object sender, EventArgs e)
        {
        }

        private void gValueSlider_ValueChanged(object sender, EventArgs e)
        {
        }

        private void bValueSlider_ValueChanged(object sender, EventArgs e)
        {
        }

    }
}
