using System;
using System.Timers;
using System.Windows.Forms;

namespace C8
{
    public partial class FormMain : Form
    {
        // The main core to be used
        private C8Core myC8Core;
        private readonly System.Timers.Timer c8timer;

        public FormMain()
        {
            InitializeComponent();

            // Create the timer
            c8timer = new System.Timers.Timer(5); // Timer is 200Hz
            c8timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        }

        // Occurs 60 times per second
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            myC8Core.MainLoop();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Create a C8 Core!
            myC8Core = new C8Core(pictureBoxScreen);
        }

        // When a key is pressed update the key array
        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D1:
                    myC8Core.key[0] = 1;
                    break;
                case Keys.D2:
                    myC8Core.key[1] = 1;
                    break;
                case Keys.D3:
                    myC8Core.key[2] = 1;
                    break;
                case Keys.D4:
                    myC8Core.key[3] = 1;
                    break;
                case Keys.Q:
                    myC8Core.key[4] = 1;
                    break;
                case Keys.W:
                    myC8Core.key[5] = 1;
                    break;
                case Keys.E:
                    myC8Core.key[6] = 1;
                    break;
                case Keys.R:
                    myC8Core.key[7] = 1;
                    break;
                case Keys.A:
                    myC8Core.key[8] = 1;
                    break;
                case Keys.S:
                    myC8Core.key[9] = 1;
                    break;
                case Keys.D:
                    myC8Core.key[10] = 1;
                    break;
                case Keys.F:
                    myC8Core.key[11] = 1;
                    break;
                case Keys.Z:
                    myC8Core.key[12] = 1;
                    break;
                case Keys.X:
                    myC8Core.key[13] = 1;
                    break;
                case Keys.C:
                    myC8Core.key[14] = 1;
                    break;
                case Keys.V:
                    myC8Core.key[15] = 1;
                    break;
            }
        }

        // Key a key is released update the key array
        private void FormMain_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D1:
                    myC8Core.key[0] = 0;
                    break;
                case Keys.D2:
                    myC8Core.key[1] = 0;
                    break;
                case Keys.D3:
                    myC8Core.key[2] = 0;
                    break;
                case Keys.D4:
                    myC8Core.key[3] = 0;
                    break;
                case Keys.Q:
                    myC8Core.key[4] = 0;
                    break;
                case Keys.W:
                    myC8Core.key[5] = 0;
                    break;
                case Keys.E:
                    myC8Core.key[6] = 0;
                    break;
                case Keys.R:
                    myC8Core.key[7] = 0;
                    break;
                case Keys.A:
                    myC8Core.key[8] = 0;
                    break;
                case Keys.S:
                    myC8Core.key[9] = 0;
                    break;
                case Keys.D:
                    myC8Core.key[10] = 0;
                    break;
                case Keys.F:
                    myC8Core.key[11] = 0;
                    break;
                case Keys.Z:
                    myC8Core.key[12] = 0;
                    break;
                case Keys.X:
                    myC8Core.key[13] = 0;
                    break;
                case Keys.C:
                    myC8Core.key[14] = 0;
                    break;
                case Keys.V:
                    myC8Core.key[15] = 0;
                    break;
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog myOpenFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),

                Title = "Browse Chip8 Roms",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "c8",
                Filter = "chip8 files (*.c8;*.ch8)|*.c8;*.ch8",
                FilterIndex = 2,
                RestoreDirectory = true,
            };

            if (myOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the filename
                myC8Core.gamePath = myOpenFileDialog.FileName;

                // Load the ROM
                myC8Core.LoadRom();
                // myC8Core.LoadTest();

                // Clear registers
                myC8Core.InitRegs();

                // Clear video memory and refresh screen
                myC8Core.display.Clear();

                // Run the mainloop
                //c8timer.Enabled = true;
            }
        }

        // Start emulation!
        private void buttonStart_Click(object sender, EventArgs e)
        {
            // Run the mainloop
            c8timer.Enabled = true;
        }

        // Stop emulation
        private void buttonStop_Click(object sender, EventArgs e)
        {
            // Stop the mainloop
            c8timer.Enabled = false;
        }

        // Clear registers and restart the game
        private void buttonReset_Click(object sender, EventArgs e)
        {
            // Clear registers
            myC8Core.InitRegs();

            // Clear video memory and refresh screen
            myC8Core.display.Clear();
        }
    }
}
