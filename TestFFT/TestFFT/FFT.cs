using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFFT
{
    /// <summary>
    /// Represent a base class for a FFT
    /// </summary>
    abstract class FFTBase
    {
        /// <summary>
        /// Implement a Fourier Transform
        /// </summary>
        /// <param name="data">contains the data as (real,imaginary) interleaved 
        /// sequence. Sequence must be a power of 2, say 2^n data points, thus 
        /// array would be 2*2^n in length</param>
        /// <param name="forward">true for a foward transform, else false for 
        /// a reverse transform</param>
        public abstract void FFT(double [] data, bool forward);

        /// <summary>
        /// For static initialization for a given size, call this
        /// </summary>
        /// <param name="size"></param>
        public virtual void Initialize(int size) { }

        /// <summary>
        /// Perform a FFT on real data, returning 
        /// half the (symmetric) complex transform
        /// as (real,imaginary) interleaved pairs
        /// </summary>
        /// <param name="data"></param>
        /// <param name="forward"></param>
        public virtual void RealFFT(double[] data, bool forward)
        {
        }

    }
}
