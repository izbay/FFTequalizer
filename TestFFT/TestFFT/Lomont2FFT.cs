using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFFT
{
    class Lomont2FFT : FFTBase
    {

        void Swap(double [] x, int p, int q)
        {
            p *= 2; q *= 2;
            var t = x[p]; x[p] = x[q]; x[q] = t;
            p++; q++;
            t = x[p]; x[p] = x[q]; x[q] = t;
        }

        /// <summary>
        /// Compute the forward or inverse FFT of data, which is 
        /// complex valued items, stored in alternating real and 
        /// imaginary real numbers. The length must be a power of 2.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="forward"></param>
        public override void FFT(double[] data, bool forward)
        {
            int n = data.Length;
            // check all are valid
            if ((n & (n - 1)) != 0) // checks n is a power of 2 in 2's complement format
                throw new Exception("data length " + n + " in FFT is not a power of 2");
            n /= 2;


            // bit reverse the indices. This is exercise 5 in section 7.2.1.1 of Knuth's TAOCP
            // the idea is a binary counter in k and one with bits reversed in j
            // see also Alan H. Karp, "Bit Reversals on Uniprocessors", SIAM Review, vol. 38, #1, 1--26, March (1996) 
            // nn = number of samples, 2* this is length of data?
            int j = 0, k = 0; // Knuth R1: initialize
            int top = n / 2; // this is Knuth's 2^(n-1)
            while (true)
            {
                // Knuth R2: swap
                // swap j+1 and k+2^(n-1) - both have two entries
                Swap(data, j + 1, k + top);
                if (j > k)
                { // swap two more
                    Swap(data, j, k);
                    Swap(data, j + top + 1, k + top + 1);
                }
                // Knuth R3: advance k
                k += 2;
                if (k >= top) break;
                // Knuth R4: advance j
                int h = top / 2;
                while (j >= h)
                {
                    j -= h;
                    h /= 2;
                }
                j += h;
            }// while

            // do transform by doing single point transforms, then doubles, fours, etc.
            double sign = forward ? 1 : -1;
            int mmax = 1;
            while (n > mmax)
            {
                int istep = 2 * mmax;
                double theta = sign * Math.PI / mmax;
                double wr = 1, wi = 0;
                double wpr = Math.Cos(theta);
                double wpi = Math.Sin(theta);
                for (int m = 1; m <= mmax; ++m)
                {
                    for (k = m - 1; k < n; k += istep)
                    {
                        j = k + mmax;
                        double tempr = wr * data[2 * j] - wi * data[2 * j+1];
                        double tempi = wi * data[2 * j] + wr * data[2 * j + 1];
                        data[2 * j] = data[2 * k] - tempr;
                        data[2 * j + 1] = data[2 * k + 1] - tempi;
                        data[2 * k] = data[2 * k] + tempr;
                        data[2 * k + 1] = data[2 * k + 1] + tempi;
                    }
                    double t = wr;
                    wr = wr * wpr - wi * wpi;
                    wi = wi * wpr + t * wpi;
                }
                mmax = istep;
            }

            // copy out with optional scaling
            double scale = forward ? 1.0 : 1.0 / n;
            for (int i = 0; i < n; ++i)
            {
                data[i * 2] *= scale;
                data[i * 2+1] *= scale;
            }
        }
    }
}
