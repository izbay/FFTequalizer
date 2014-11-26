using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFFT
{
    class NaiveFFT : FFTBase
    {

        /// <summary>
        /// Naive implementation of a Fourier Transform
        /// Technically not a FFT :)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="forward"></param>
        public override void FFT(double[] data, bool forward)
        {
            int N = data.Length / 2;
            double[] y = new double[N*2];
            double[] x = data; // alias

            double sign = 1;
            if (!forward)
                sign = -1;

            for (int j = 0; j < N ; ++j)
            {
                y[j * 2] = y[j * 2 + 1] = 0; // technically unneeded in C#, written for clarity
                for (int k = 0; k < N ; ++k)
                {
                    double expRe = Math.Cos(2 * sign*Math.PI * j * k / N);
                    double expIm = Math.Sin(2 * sign*Math.PI * j * k / N);
                    // real and imaginary parts
                    y[j * 2] += expRe * x[k * 2] - expIm * x[k * 2 + 1];
                    y[j * 2 + 1] += expRe * x[2 * k + 1] + expIm * x[2 * k];
                }
            }
            
            // copy back with optional scaling
            double scale = 1;
            if (!forward)
                scale = 1.0 / N;
            for (int k = 0; k < N*2; ++k)
                data[k] = y[k]*scale;
        }
    }
}
