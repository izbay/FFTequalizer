using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFT_Project
{
    public partial class Form1 : Form
    {

        protected Stream myStream;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadWaveFile(object sender, EventArgs e)
        {
            OpenFileDialog openedFile = new OpenFileDialog();
            openedFile.Filter = "wav files (*.wav)|*.wav";

            if (openedFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openedFile.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            SoundPlayer songPlayer = new SoundPlayer(myStream);
                            songPlayer.Play();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not find file specified");
                }
            }

        }
    }
}
