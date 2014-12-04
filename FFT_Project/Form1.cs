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

        private const int timerSpeedTest = 50;
        private static int timerSpeed = timerSpeedTest-5;

        private static double byteOffset = 0;
        private static int sampleSize = 4096;
        private static double[] histogramValues = new double[sampleSize];
 
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

                    catch (Exception)
                    {
                        MessageBox.Show("Could not find file specified");
                    }
                }

        }

        private void playSong(object sender, EventArgs e)
        {

            try
            {
                counter = 0;
                songPlayer.Play();
                aTimer.Start();
            }

            catch (Exception)
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

        // Wrapper function to prepare data, run FFT, and display results in GUI.
        private void doFFT(double[] data)
        {
            //String FFTString = "";

            if (sampleSize <= data.Length) FFT(data);

            chart1.Series["Series1"].Points.Clear();

            for (int i = 2; i < sampleSize / 4; i = i << 1)
            {
                double sum = 0, n = 0;
                //average from data[i] --> data[i*i]
                for (int j = i; j < (i << 1); j += 2)
                {
                    sum += (Math.Sqrt(data[j] * data[j] + data[j + 1] * data[j + 1]));
                    n++;
                }

                // take log base 2 of i
                histogramValues[(int)Math.Log(i, 2) - 1] = (histogramValues[(int)Math.Log(i, 2) - 1] + (sum / n)) / 2;

                chart1.Series["Series1"].Points.AddXY(Math.Log(i, 2), histogramValues[(int)Math.Log(i, 2) - 1]);
            }

            /*
            for (int i = 2; i < data.Length - 3; i += 2)
            {
                //FFTString += data[i];
                //FFTString += Environment.NewLine;
                histogramValues[(i - 2) / 2] = (histogramValues[(i - 2) / 2] + (Math.Sqrt(data[i] * data[i] + data[i + 1] * data[i + 1]))) / 2;
                chart1.Series["Series1"].Points.AddXY(i + 1, histogramValues[(i - 2) / 2]);
            }
            */

            //textBox2.Text = FFTString;
        }

        public void FFT(double[] data)
        {
            int len = data.Length / 2;
    
            BitReverse(data, len);                                                    
            for(int i = 1; i < len; i *= 2)
            {
                int i_next = i * 2;
                double x = Math.PI / i, a = 0, b = 1;
                double cosx = Math.Cos(x), sinx = Math.Sin(x);
                for (int j = 0; j < i_next; j += 2)
                {
                    for (int k = j; k < 2 * len; k += 2 * i_next)
                    {
                        int m = k + i_next;
                        double a_temp = a * data[m] + b * data[m + 1];
                        double b_temp = b * data[m] - a * data[m + 1];
                
                        data[m] = data[k] - b_temp;
                        data[m + 1] = data[k + 1] - a_temp;
                        data[k] = data[k] + b_temp;
                        data[k + 1] = data[k + 1] + a_temp;
                    }
                    double temp = a;                                                               
                    a = a * cosx + b * sinx;
                    b = b * cosx - temp * sinx;
                }
            }
        }

        static void BitReverse(double[] data, int len)
        {
            int i = 0, j = 0;

            while (true)
            {
                // Do the swaps.
                double temp = data[i + 2];
                data[i + 2] = data[j + len];
                data[j + len] = temp;

                temp = data[i + 3];
                data[i + 3] = data[j + len + 1];
                data[j + len + 1] = temp;

                if (i > j)
                {
                    temp = data[i];
                    data[i] = data[j];
                    data[j] = temp;

                    temp = data[i + 1];
                    data[i + 1] = data[j + 1];
                    data[j + 1] = temp;

                    temp = data[i + len + 2];
                    data[i + len + 2] = data[j + len + 2];
                    data[j + len + 2] = temp;

                    temp = data[i + len + 3];
                    data[i + len + 3] = data[j + len + 3];
                    data[j + len + 3] = temp;
                }

                j += 4;
                if (j >= len) break;

                // len is guaranteed to be divisible by 2.
                int k = len / 2;
                while (i >= k)
                {
                    i -= k;
                    k /= 2;
                }
                i += k;
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
