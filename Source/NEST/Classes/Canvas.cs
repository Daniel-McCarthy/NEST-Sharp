using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Core = NEST.Classes.Core;
using SFML.System;

namespace NEST.Classes
{
    public partial class Canvas : UserControl
    {
        public SFML.Graphics.RenderWindow renderwindow;
        public SFML.Graphics.Color clearColor = new SFML.Graphics.Color(255, 80, 160);

        public Canvas()
        {
            InitializeComponent();
            renderwindow = new SFML.Graphics.RenderWindow(this.Handle);

            PreviewKeyDown += Canvas_PreviewKeyDown;
            KeyDown += Canvas_KeyDown;
            KeyUp += Canvas_KeyUp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
             clearScreen();
        }
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //Do nothing
        }

        delegate void refreshDelegate();

        public void refresh()
        {
            if (InvokeRequired)
            {
                if (Core.run)
                {
                    refreshDelegate del = new refreshDelegate(refresh);
                    Invoke(del);
                }
            }
            else
            {
                Refresh();
            }
        }

        delegate void setColorDelegate(SFML.Graphics.Color newColor);

        public void setClearColor(SFML.Graphics.Color color)
        {

            if (InvokeRequired)
            {
                if (Core.run)
                {
                    setColorDelegate del = new setColorDelegate(setClearColor);
                    Invoke(del, new object[] { color });
                }
            }
            else
            {
                clearColor = color;
            }
        }

        delegate void drawFrameDelegate(SFML.Graphics.Sprite newFrame);

        public void drawFrame(SFML.Graphics.Sprite newFrame)
        {
            if (InvokeRequired)
            {
                if (Core.run)
                {
                    drawFrameDelegate del = new drawFrameDelegate(drawFrame);
                    Invoke(del, new object[] { newFrame });
                }
            }
            else
            {
                renderwindow.Draw(newFrame);
                renderwindow.Display();
            }
        }

        delegate void clearScreenDelegate();
        public void clearScreen()
        {
            if (InvokeRequired)
            {
                if(Core.run)
                { 
                    clearScreenDelegate del = new clearScreenDelegate(clearScreen);
                    Invoke(del);
                }
            }
            else
            {
                renderwindow.Clear(clearColor);
                renderwindow.Display();
            }
        }

        private void Canvas_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        public void Canvas_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                //this.BackColor = System.Drawing.Color.Aqua;
                Core.input.setJoy1KeyInput(0, true);
            }

            if (e.KeyCode == Keys.Down)
            {
                Core.input.setJoy1KeyInput(1, true);
            }

            if (e.KeyCode == Keys.Left)
            {
                Core.input.setJoy1KeyInput(2, true);
            }

            if (e.KeyCode == Keys.Right)
            {
                Core.input.setJoy1KeyInput(3, true);
            }

            if (e.KeyCode == Keys.Enter)
            {
                Core.input.setJoy1KeyInput(4, true);
            }

            if (e.KeyCode == Keys.ShiftKey)
            {
                Core.input.setJoy1KeyInput(5, true);
            }

            if (e.KeyCode == Keys.Z)
            {
                Core.input.setJoy1KeyInput(6, true);
            }

            if (e.KeyCode == Keys.X)
            {
                Core.input.setJoy1KeyInput(7, true);
            }

            /*
            if (e.KeyCode == Keys.F1)
            {
                if (Core.beakMemory.romFilePath != "")
                {
                    Core.beakMemory.createSaveFile(Core.beakMemory.romFilePath, true);
                }
            }*/
        }

        public void Canvas_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Up)
            {
                //this.BackColor = System.Drawing.Color.Aqua;
                Core.input.setJoy1KeyInput(0, false);
            }

            if (e.KeyCode == Keys.Down)
            {
                Core.input.setJoy1KeyInput(1, false);
            }

            if (e.KeyCode == Keys.Left)
            {
                Core.input.setJoy1KeyInput(2, false);
            }

            if (e.KeyCode == Keys.Right)
            {
                Core.input.setJoy1KeyInput(3, false);
            }

            if (e.KeyCode == Keys.Enter)
            {
                Core.input.setJoy1KeyInput(4, false);
            }

            if (e.KeyCode == Keys.ShiftKey)
            {
                Core.input.setJoy1KeyInput(5, false);
            }

            if (e.KeyCode == Keys.Z)
            {
                Core.input.setJoy1KeyInput(6, false);
            }

            if (e.KeyCode == Keys.X)
            {
                Core.input.setJoy1KeyInput(7, false);
            }

        }

    }
}
