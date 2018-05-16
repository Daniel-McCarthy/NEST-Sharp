using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NEST.Forms
{
    public partial class BreakpointInputBox : NEST.Forms.InputBox
    {
        public BreakpointInputBox()
        {
            InitializeComponent();
        }

        private void BreakpointInputBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                string textToVerify = inputTextBox.Text;
                int textLength = textToVerify.Length;

                if (textLength == 0 || textLength > 4)
                {
                    e.Cancel = true;

                    MessageBox.Show("Address length must be 1 to 4 hexadecimal characters.");
                }

                ushort address = 0;
                bool parseSuccess = ushort.TryParse(textToVerify, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out address);

                if (!parseSuccess)
                {
                    e.Cancel = true;

                    MessageBox.Show("This is not a valid hexadecimal address.");
                }
            }
        }
    }
}
