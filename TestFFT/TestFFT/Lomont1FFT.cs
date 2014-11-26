using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFFT
{
    class Lomont1FFT : FFTBase
    {

        class Complex
        {
            public double x, y;
            public Complex(double a, double b)
            {
                x = a; y = b;
            }
            public Complex() 
                : this(0,0)
            {
            }
            public Complex(Complex v)
                : this(v.x, v.y)
            {
            }
            static public Complex operator *(Complex a, Complex b)
            {return new Complex(a.x*b.x-a.y*b.y, a.x*b.y+a.y*b.x);}
            static public Complex operator +(Complex a, Complex b)
            { return new Complex(a.x + b.x, a.y + b.y); }
            static public Complex operator -(Complex a, Complex b)
            { return new Complex(a.x - b.x, a.y - b.y); }
            public Complex Conjugate()
            {
                return new Complex(x, -y);
            }
        }


        void Swap(Complex[] x, int p, int q)
        {
            Complex t = x[p]; x[p] = x[q]; x[q] = t;
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
            n/=2;

            Complex [] x = new Complex[n];
            for (int i = 0; i < n; ++i)
                x[i] = new Complex(data[i*2],data[i*2+1]);

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
                    Swap(x, j + 1, k + top);
                    if (j > k)
                    { // swap two more
                        Swap(x, j, k);
                        Swap(x, j + top + 1, k + top + 1);
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
                double theta = sign*Math.PI / mmax;
                Complex wp = new Complex(Math.Cos(theta),Math.Sin(theta));
                Complex w = new Complex(1, 0);
                for (int m = 1; m <= mmax; ++m)
                {
                    for (k = m - 1; k < n; k += istep)
                    {
                        j = k + mmax;
                        Complex temp = w * new Complex(x[j]);
                        x[j] = x[k] - temp;
                        x[k] = x[k] + temp;
                    }
                    w = w*wp;
                }
                mmax = istep;
            }
            
            // copy out with optional scaling
            double scale = forward ? 1.0 : 1.0/n;
            for (int i = 0; i < n; ++i)
            {
                data[i * 2] = x[i].x*scale;
                data[i * 2+1] = x[i].y*scale;
            }
        }

        public override void RealFFT(double[] data, bool forward)
        {
            Complex sign = new Complex(1, 0);
            if (forward)
                FFT(data, forward); // do packed FFT
            else
                sign.x = -1;
            int n = data.Length; // number of real input points, which is 1/2 the complex length
            var halfr = new Complex(0.5, 0);
            var halfi = new Complex(0, 0.5);
            Complex[] y = new Complex[n / 2];
            Complex[] t = new Complex[n / 2];

            // copy in
            for (int j = 0; j <= n / 2 - 1; ++j)
                t[j] = new Complex(data[2 * j], data[2 * j + 1]);

            for (int j = 1; j <= n / 2 - 1; ++j)
            {
                var tn = t[n / 2 - j].Conjugate();
                double theta = sign.x * 2 * Math.PI * j / n;
                var wj = new Complex(Math.Cos(theta), Math.Sin(theta));
                y[j] = halfr * (t[j] + tn) - sign * halfi * (t[j] - tn) * wj;
            }

            // copy out entries j=1,2,...n/2-1
            for (int j = 1; j <= n / 2 - 1; ++j)
            {
                data[2 * j] = y[j].x;
                data[2 * j + 1] = y[j].y;
            }

            if (forward)
            {
                // compute final y0 and y_{N/2} ones, place into data[0] and data[1]
                var t0C = t[0].Conjugate();
                data[0] = (halfr * (t[0] + t0C) - halfi * (t[0] - t0C)).x; // y0 real value
                data[1] = (halfr * (t[0] + t0C) + halfi * (t[0] - t0C)).x; // y_{N/2} real value
            }
            else
            {
                double temp = data[0]; // reverse 
                data[0] = 0.5 * (temp + data[1]);
                data[1] = 0.5 * (temp - data[1]);
                FFT(data, false);
            }
        }


    }
}
