using System;
using System.Diagnostics;

namespace TestFFT
{
    class NumericalRecipesFFT : FFTBase
    {

        /// <summary>
        /// This version is from Numerical Recipes
        /// The license is too restrictive to use in anything serious
        /// </summary>
        /// <param name="data"></param>
        /// <param name="forward"></param>

        public override void FFT(double[] data, bool forward)
        {
            int nn = data.Length;
            if ((nn & (nn - 1)) != 0) // checks nn is a power of 2 in 2's complement format
                throw new Exception("data length " + nn + " in FFT is not a power of 2");
            nn /= 2;
            int sign = 1;
            if (forward == false)
                sign = -1;

            int n = nn << 1;

            for (int k = 1, j = 1; k < n; k += 2)
            {
                double temp;
                if (j > k)
                { // swap real and imaginary parts
                    temp = data[j-1]; data[j-1] = data[k-1]; data[k-1] = temp;
                    temp = data[j+1-1]; data[j+1-1] = data[k+1-1]; data[k+1-1] = temp;
                }
                int m = n >> 1;
                while ((m >= 2) && (j > m))
                {
                    j -= m;
                    m >>= 1;
                }
                j += m;
            }

            // Danielson-Lanczos section
            int mmax = 2;
            while (n > mmax)
            {
                int istep = mmax << 1;
                double theta = sign * (2 * Math.PI / mmax);
                double wtemp = Math.Sin(0.5 * theta);
                double wpr = -2.0 * wtemp * wtemp; // real
                double wpi = Math.Sin(theta);
                double wr = 1, wi = 0;
                for (int m = 1; m < mmax; m += 2)
                {
                    for (int k = m; k <= n; k += istep)
                    {
                        int j = k + mmax;
                        double tempr = wr * data[j - 1] - wi * data[j + 1 - 1];
                        double tempi = wr * data[j + 1 - 1] + wi * data[j - 1];
                        data[j - 1] = data[k - 1] - tempr;
                        data[j + 1 - 1] = data[k + 1 - 1] - tempi;
                        data[k - 1] += tempr;
                        data[k + 1 - 1] += tempi;
                    }
                    wtemp = wr;
                    wr = wr * wpr - wi * wpi + wr; // trig recurrence
                    wi = wi * wpr + wtemp * wpi + wi;
                }
                mmax = istep;
            }
            if (!forward)
            { // scale back for inverse transform
                double scale = 2.0 / n;
                for (int i = 0; i < data.Length; ++i)
                    data[i] *= scale;
            }

        }

        /// <summary>
        /// Perform a FFT on real data, returning 
        /// half the (symmetric) complex transform
        /// as (real,imaginary) interleaved pairs
        /// </summary>
        /// <param name="data"></param>
        /// <param name="forward"></param>
        public override void RealFFT(double[] data, bool forward)
        {

            int n = data.Length;
            double theta = Math.PI / (n >> 1), c2, c1 = 0.5;
            if (forward == true)
            {
                c2 = -0.5;
                FFT(data, forward); // forward transform
            }
            else
            {
                c2 = 0.5;
                theta = -theta;
            }
            double wtemp = Math.Sin(0.5 * theta);
            double wpr = -2 * wtemp * wtemp;
            double wpi = Math.Sin(theta);
            double wr = 1 + wpr;
            double wi = wpi;
            int np3 = n + 3;
            for (int i = 2; i <= (n >> 2); ++i)
            {
                int i1 = i + i - 1;
                int i2 = i1 + 1;
                int i3 = np3 - i2;
                int i4 = 1 + i3;
                double h1r = c1 * (data[i1-1] + data[i3-1]);
                double h1i = c1 * (data[i2 - 1] - data[i4 - 1]);
                double h2r = -c2 * (data[i2 - 1] + data[i4 - 1]);
                double h2i = c2 * (data[i1 - 1] - data[i3 - 1]);
                data[i1 - 1] = h1r + wr * h2r - wi * h2i;
                data[i2 - 1] = h1i + wr * h2i + wi * h2r;
                data[i3 - 1] = h1r - wr * h2r + wi * h2i;
                data[i4 - 1] = -h1i + wr * h2i + wi * h2r;
                wtemp = wr;
                wr = wr * wpr - wi * wpi + wr;
                wi = wi * wpr + wtemp * wpi + wi;                
            }
            if (forward == true)
            {
                double h1r = data[1 - 1];
                data[1 - 1] = h1r + data[2 - 1];
                data[2 - 1] = h1r - data[2 - 1];
            }
            else
            {
                double h1r = data[1 - 1];
                data[1 - 1] = c1 * (h1r + data[2 - 1]);
                data[2 - 1] = c1 * (h1r - data[2 - 1]);
                FFT(data, false); 
            }
        }
    }
}

