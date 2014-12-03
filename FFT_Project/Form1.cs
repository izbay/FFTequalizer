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
        private static System.Timers.Timer aTimer;
        private static int counter = 0;
        private static int bytesPerSecond = 0;
        private static double[] doubleArray;

        //debug / testing
        private const int timerSpeedTest = 50;
        private static int timerSpeed = timerSpeedTest-3;

        private static double byteOffset = 0;
        private static int sampleSize = 64;
        private static double[] histogramValues = new double[sampleSize/2];
 
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

                        int fileHeaderSize = 44;

                        byte[] bytesPerSecondHeader = File.ReadAllBytes(openedFile.FileName).Skip(28).Take(4).ToArray(); // get the bytes from the header file that correspond to the bytes/second 

                        String hexValue = "";

                        for (int i = 3; i >= 0; i--)
                        {
                            hexValue += Convert.ToInt32(bytesPerSecondHeader[i]).ToString("X"); // convert into 32-bit hex 
                            // band-aid
                            if (bytesPerSecondHeader[i] == 0)
                                hexValue += "0";
                        }

                        bytesPerSecond = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber); // convert 32-bit hex into 32-bit signed int

                        byteOffset = (double)(((double)bytesPerSecond / (double)sampleSize) * (50.0 / 1000.0)); // find a 'byte offset' to move the FFT through the file in time with the audio 

                        byte[] bytes = File.ReadAllBytes(openedFile.FileName).Skip(fileHeaderSize).ToArray();

                        List<double> dList = new List<double>();
                        bytes.ToList<byte>().ForEach(b => dList.Add(Convert.ToDouble(b)));
                        doubleArray = dList.ToArray<double>();

                        // Create a timer with a 1ms interval.
                        aTimer = new System.Timers.Timer(timerSpeed);
                        // Hook up the Elapsed event for the timer. 
                        aTimer.Elapsed += OnTimed;


                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not find file specified");
                    }
                }

        }

        void OnTimed(object sender, EventArgs e)
        {

            if (counter > doubleArray.Length / sampleSize)
            {
                this.Invoke((MethodInvoker)delegate { chart1.Series["Series1"].Points.Clear(); });
                aTimer.Stop();
            }
            else
            {
                this.Invoke((MethodInvoker)delegate { doFFT(doubleArray.Skip(counter * sampleSize).Take(sampleSize).ToArray()); });
                counter += Convert.ToInt32(byteOffset); // Skip ahead so we're not sampling concentrated areas repeatedly.
            }

        }

        private void doFFT(double[] data)
        {
            //String FFTString = "";

            FFT(data);

            chart1.Series["Series1"].Points.Clear();

            for (int i = 2; i < data.Length-3; i += 2)
            {
                //FFTString += data[i];
                //FFTString += Environment.NewLine;
                histogramValues[(i-2)/2] = (histogramValues[(i-2)/2] + (Math.Sqrt(data[i]*data[i]+data[i+1]*data[i+1]))) / 2;
                chart1.Series["Series1"].Points.AddXY(i + 1, histogramValues[(i-2)/2]); 
            }

            //textBox2.Text = FFTString;
        }

        private void playSong(object sender, EventArgs e)
        {

            try
            {
                counter = 0;
                songPlayer.Play();
                aTimer.Start();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Could not find file specified");
            }
             
        }

        private void stopSong(object sender, EventArgs e)
        {
            aTimer.Stop();
            chart1.Series["Series1"].Points.Clear();
            songPlayer.Stop();
        }

        /** 
         * Code below taken from http://www.lomont.org/Software/Misc/FFT/LomontFFT.html for reference.
         * We will have to study/rewrite it as necessary, but this at least gets us moving.
        **/

        public void FFT(double[] data)
        {
            Reverse(data);                                                    

            // do transform: so single point transforms, then doubles, etc.
            int n = data.Length / 2, mmax = 1;
            while (n > mmax)
            {
                var istep = 2 * mmax;
                var theta = Math.PI / mmax;
                double wr = 1, wi = 0;
                var wpr = Math.Cos(theta);
                var wpi = Math.Sin(theta);
                for (var m = 0; m < istep; m += 2)
                {
                    for (var k = m; k < 2 * n; k += 2 * istep)
                    {
                        var j = k + istep;
                        var tempr = wr * data[j] - wi * data[j + 1];
                        var tempi = wi * data[j] + wr * data[j + 1];
                        data[j] = data[k] - tempr;
                        data[j + 1] = data[k + 1] - tempi;
                        data[k] = data[k] + tempr;
                        data[k + 1] = data[k + 1] + tempi;
                    }
                    var t = wr; // trig recurrence                                                               
                    wr = wr * wpr - wi * wpi;
                    wi = wi * wpr + t * wpi;
                }
                mmax = istep;
            }
        }

        static void Reverse(double[] data)
        {
            // bit reverse the indices. This is exercise 5 in section                                            
            // 7.2.1.1 of Knuth's TAOCP the idea is a binary counter                                             
            // in k and one with bits reversed in j                                                              
            int j = 0, k = 0, n = data.Length/2; // Knuth R1: initialize                                                            
            var top = n / 2;  // this is Knuth's 2^(n-1)                                                         
            while (true)
            {
                // Knuth R2: swap - swap j+1 and k+2^(n-1), 2 entries each                                       
                var t = data[j + 2];
                data[j + 2] = data[k + n];
                data[k + n] = t;
                t = data[j + 3];
                data[j + 3] = data[k + n + 1];
                data[k + n + 1] = t;
                if (j > k)
                { // swap two more                                                                               
                    // j and k                                                                                   
                    t = data[j];
                    data[j] = data[k];
                    data[k] = t;
                    t = data[j + 1];
                    data[j + 1] = data[k + 1];
                    data[k + 1] = t;
                    // j + top + 1 and k+top + 1                                                                 
                    t = data[j + n + 2];
                    data[j + n + 2] = data[k + n + 2];
                    data[k + n + 2] = t;
                    t = data[j + n + 3];
                    data[j + n + 3] = data[k + n + 3];
                    data[k + n + 3] = t;
                }
                // Knuth R3: advance k                                                                           
                k += 4;
                if (k >= n)
                    break;
                // Knuth R4: advance j                                                                           
                var h = top;
                while (j >= h)
                {
                    j -= h;
                    h /= 2;
                }
                j += h;
            } // bit reverse loop                                                                                
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
