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
        private SoundPlayer songPlayer;

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
                    songPlayer = new SoundPlayer(openedFile.FileName);

                    textBox1.Text = openedFile.FileName;

                    byte[] bytes = File.ReadAllBytes(openedFile.FileName);

                    String byteString = "";
                    
                    for (int i = 0; i < 4096; i++)
                    {
                        byteString += bytes[i];
                        byteString += " ";
                    }
                    
                    textBox1.Text = /*bytes.Length.ToString();*/ byteString;
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Could not find file specified");
                }
            }

        }

        private void playSong(object sender, EventArgs e)
        {

            try
            {
                songPlayer.Play();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Could not find file specified");
            }
             
        }

        private void pauseSong(object sender, EventArgs e)
        {
            songPlayer.Stop();
        }
    }
}
