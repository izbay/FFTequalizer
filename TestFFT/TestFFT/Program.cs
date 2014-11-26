using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace TestFFT
{
    class Program
    {

        struct Result
        {
            public long max, min, avg;
            public long maxR, minR, avgR;
            public long length;
        }

        /// <summary>
        /// Run tests on the transform
        /// </summary>
        /// <param name="transform">The highest power of 2 transform tried</param>
        /// <param name="maxPass">The number of passes of 50 runs each to time</param>
        static List<Result> Test(FFTBase transform, int maxPass, int maxPower)
        {
            Stopwatch watch = new Stopwatch();
            List<Result> results = new List<Result>();

            int left = Console.CursorLeft;
            int top = Console.CursorTop;

            for (int i = 1; i <= maxPower; ++i)
            {
                // create sample data
                int length = 1 << i;
                double[] data = new double[length * 2];
                for (int j = 0; j < data.Length; ++j)
                    data[j] = Math.Sin(j)+Math.Cos(j)+j*j;


                transform.Initialize(data.Length/2);
                // run 100 times to prep cache
                for (int run = 0; run < 100; ++run)
                    transform.FFT(data,true);

                // now time the behavior
                Result res = new Result();
                res.min = long.MaxValue;
                res.max = long.MinValue;
                res.avg = 0;
                res.length = length;
                for (int pass = 0; pass < maxPass; ++pass)
                {
                    watch.Reset();
                    watch.Start();
                    for (int inner = 0; inner < 50; ++inner)
                        transform.FFT(data, true);
                    watch.Stop();

                    Console.SetCursorPosition(left, top);
                    Console.Write("Length {0}/{1}, {2}%     ",i,maxPower, pass * 100 / maxPass);
                    
                    res.avg += watch.ElapsedTicks;
                    res.min = Math.Min(res.min, watch.ElapsedTicks);
                    res.max = Math.Max(res.max, watch.ElapsedTicks);
                }
                res.avg /= maxPass;
                results.Add(res);
            }
            Console.SetCursorPosition(left, top);
            Console.Write(new string(' ', 40));
            Console.SetCursorPosition(left, top);
            return results;
        } // Test


        /// <summary>
        /// Run tests on the transform
        /// </summary>
        /// <param name="transform">The highest power of 2 transform tried</param>
        /// <param name="maxPass">The number of passes of 50 runs each to time</param>
        static List<Result> TestReal(FFTBase transform, int maxPass, int maxPower)
        {
            Stopwatch watch = new Stopwatch();
            List<Result> results = new List<Result>();

            int left = Console.CursorLeft;
            int top = Console.CursorTop;

            for (int i = 1; i <= maxPower; ++i)
            {
                // create sample data
                int length = 1 << i;
                double[] data = new double[length * 2];
                for (int j = 0; j < data.Length; ++j)
                    data[j] = Math.Sin(j) + Math.Cos(j) + j * j;



                // now time the FFT behavior
                transform.Initialize(data.Length);
                // run 100 times to prep cache
                for (int run = 0; run < 100; ++run)
                    transform.FFT(data, true);

                Result res = new Result();
                res.min = long.MaxValue;
                res.max = long.MinValue;
                res.avg = 0;
                res.length = length;
                for (int pass = 0; pass < maxPass; ++pass)
                {
                    watch.Reset();
                    watch.Start();
                    for (int inner = 0; inner < 50; ++inner)
                    {
                        double[] data2 = new double[data.Length*2];
                        for (int k = 0; k < data.Length; k++)
                            data2[2 * k] = data[k];
                        transform.FFT(data2, true);
                    }
                    watch.Stop();

                    Console.SetCursorPosition(left, top);
                    Console.Write("Length {0}/{1}, {2}%     ", i, maxPower, pass * 100 / maxPass);

                    res.avg += watch.ElapsedTicks;
                    res.min = Math.Min(res.min, watch.ElapsedTicks);
                    res.max = Math.Max(res.max, watch.ElapsedTicks);
                }
                res.avg /= maxPass;


                // now time the real FFT behavior
                transform.Initialize(data.Length/2);
                // run 100 times to prep cache
                for (int run = 0; run < 100; ++run)
                    transform.RealFFT(data, true);
                res.minR = long.MaxValue;
                res.maxR = long.MinValue;
                res.avgR = 0;
                res.length = length;
                for (int pass = 0; pass < maxPass; ++pass)
                {
                    watch.Reset();
                    watch.Start();
                    for (int inner = 0; inner < 50; ++inner)
                        transform.RealFFT(data, true);
                    watch.Stop();

                    Console.SetCursorPosition(left, top);
                    Console.Write("Length {0}/{1}, {2}%     ", i, maxPower, pass * 100 / maxPass);

                    res.avgR += watch.ElapsedTicks;
                    res.minR = Math.Min(res.min, watch.ElapsedTicks);
                    res.maxR = Math.Max(res.max, watch.ElapsedTicks);
                }
                res.avgR /= maxPass;

                // save result
                results.Add(res);

            }
            Console.SetCursorPosition(left, top);
            Console.Write(new string(' ', 40));
            Console.SetCursorPosition(left, top);
            return results;
        } // Test

        /// <summary>
        /// Compare two arrays of doubles for "equality"
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static bool Compare(double[] a, double[] b)
        {
            if (a.Length != b.Length) 
                return false;
            for (int i = 0; i < a.Length; ++i)
                if ((Math.Abs(a[i] - b[i]) > 0.0001))
                    return false;
            return true;
        }

        static bool UnitTest(FFTBase transform)
        {
            // some tests of various lengths
            double[] t2 = { 0.999921, 0.264994, 0.086187, 0.927387 };
            double[] a2 = { 1.08611, 1.19238, 0.913734, -0.662394 };
            double[] t4 = { 1, 0, 1, 0, 1, 0, 1, 0 };
            double[] a4 = { 4, 0, 0, 0, 0, 0, 0, 0 };
            double[] t8 = { 0.71307, 0.612091, 0.19175, 0.334253, 0.627216, 0.581504, 0.578624, 0.0468427, 0.383359, 0.301881, 0.371672, 0.321972, 0.250879, 0.887941, 0.231716, 0.577345 };
            double[] a8 = {3.34828, 3.66383, 0.630061, 1.18843, 0.186297, -0.802392, 0.762237, -0.331856, 0.600761, 1.103, 0.642236, 0.184664, 0.250371, -0.308556, -0.715689, 0.199603};
            double [] t32 = {1, 0, 0.980785, 0.19509, 0.92388, 0.382683, 0.83147, 0.55557, 0.707107, 0.707107, 0.55557, 0.83147, 0.382683, 0.92388, 0.19509, 0.980785, 0, 1, -0.19509, 0.980785, -0.382683, 0.92388, -0.55557, 
                            0.83147, -0.707107, 0.707107, -0.83147, 0.55557, -0.92388, 0.382683, -0.980785, 0.19509, -1, 0, -0.980785, -0.19509, -0.92388, -0.382683, -0.83147, -0.55557, -0.707107, -0.707107, -0.55557, -0.83147, -0.382683, -0.92388, -0.19509, -0.980785, 0, -1, 0.19509, 
                            -0.980785, 0.382683, -0.92388, 0.55557, -0.83147, 0.707107, -0.707107, 0.83147, -0.55557, 0.92388, -0.382683, 0.980785, -0.19509};
            double[] a32 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 0};

            double[][] tests = { t2, a2 };//, t4, a4, t8, a8, t32, a32 };

            //Action<double[]> dump = d => { foreach (var v in d) Console.Write("{0:f4} ", v); Console.WriteLine(); };
            Action<double[]> dump = d => { };

            bool ret = true;
            for (int testIndex = 0; testIndex < tests.Length; testIndex += 2)
            {
                double[] test = tests[testIndex].ToArray(); // makes copy
                double[] answer = tests[testIndex + 1];

                dump(test);
                dump(answer);
                transform.Initialize(test.Length/2);
                transform.FFT(test, true);
                ret &= Compare(test, answer);
                dump(test);
                transform.FFT(test, false);
                ret &= Compare(test, tests[testIndex]);
                dump(test);
            }
            return ret;

        }

        // Test a real valued fourier transform
        static bool UnitTestReal(FFTBase transform)
        {
            // some tests of various lengths
            double[] t4 = { 1,1,1,1 };
            double[] a4 = {4, 0, 0, 0};
            double[] t4a = { 1, 2, 3, 4 };
            double[] a4a = { 10, -2, -2, -2 };

            double[] t8 = {0.100652, -0.442825, -0.457954, -0.00624455, 0.19978, -0.267328, -0.47192, -0.235878};
            double[] a8 = { -1.58172, 0.322834, -0.385598, 0.0522465, 1.23031, -0.468031, 0.187343, 0.0243147 };

            double [] t32 = {-0.333615, 0.468917, 0.884538, 0.0276625, 0.979812, 0.91061, -0.175599, 0.1756, -0.695263, 0.557298, 0.112251, -0.285586, -0.73988, -0.0750604, -0.332421, 0.391004, 0.0588164, -0.18941,                     -0.416513, -0.596507, 0.659257, -0.654753, -0.472673, 0.875249, -0.00712734, -0.12367, -0.357211, -0.152413, 0.0130609, -0.0342799, 0.818388, 0.671986};
            double [] a32 = {1.96247, -1.97083, 4.71435, 1.34203, 1.41278, 2.2209, -0.301542, 1.30462, 0.717877, -1.42063, -3.19595, -1.52441, -0.474644, -2.90705, 0.747585, 2.44391, -0.125698, -0.247344, -4.4128, -1.07521, -1.28254, 2.42047, -1.30217, -0.450559, -4.49676, -2.19137, 0.193633, 0.848902, 2.05478, -1.91513, 0.417439, 1.79843};

            double[][] tests = { t4, a4 };// t4a, a4a, t8, a8, t32, a32 };
            //double[][] tests = { t4a, a4a};
            //double[][] tests = { t8, a8 };

            //Action<double[]> dump = d => { foreach (var v in d) Console.Write("{0:f4} ", v); Console.WriteLine(); };
            Action<double[]> dump = d => { };

            bool ret = true;
            for (int testIndex = 0; testIndex < tests.Length; testIndex += 2)
            {
                double[] test = tests[testIndex].ToArray(); // makes copy
                double[] answer = tests[testIndex + 1];

                dump(test);
                dump(answer);
                transform.Initialize(test.Length/2);
                transform.RealFFT(test, true);
                ret &= Compare(test, answer);
                dump(test);
                transform.RealFFT(test, false);
                ret &= Compare(test, tests[testIndex]);
                dump(test);
            }
            return ret;

        }

        static void Main(string[] args)
        {


            //Console.WriteLine("Testing NR (real): {0}", UnitTestReal(new NumericalRecipesFFT()));
            //Console.WriteLine("Testing Lomont4 (real): {0}", UnitTestReal(new Lomont4FFT()));
            Console.WriteLine("Testing LomontFFT {0}", Lomont.LomontFFT.UnitTest());
            //return;

            Console.WriteLine("FFT Performance testing");
            
            // enumerate all FFT classes
            Assembly assm = Assembly.GetExecutingAssembly();
            List<Type> FFTTypes = new List<Type>();
            foreach (var type in assm.GetTypes())
                if (type.BaseType == typeof(FFTBase))
                    FFTTypes.Add(type);

            //Console.WriteLine("Testing Naive  : {0}", UnitTest(new NaiveFFT()));
            //Console.WriteLine("Testing NR     : {0}", UnitTest(new NumericalRecipesFFT()));
            //Console.WriteLine("Testing NAudio : {0}", UnitTest(new NAudioFFT()));
            //Console.WriteLine("Testing Lomont1: {0}", UnitTest(new Lomont1FFT()));
            //Console.WriteLine("Testing Lomont2: {0}", UnitTest(new Lomont2FFT()));
            //Console.WriteLine("Testing Lomont3: {0}", UnitTest(new Lomont3FFT()));
            //Console.WriteLine("Testing Lomont4: {0}", UnitTest(new Lomont4FFT()));
            //return;

            // measure some performance items
            foreach (var type in FFTTypes)
            {
                if (type != typeof(NAudioFFT)) // type == typeof(NaiveFFT))
                    continue;
                FFTBase transform = Activator.CreateInstance(type) as FFTBase;
                Console.Write("Testing {0}: ", type.Name);
                Console.Write("Length, min, max, avg, minR, maxR, avgR, minR/min:");
                Console.Write("Passed checks {0}: ", UnitTest(transform));
                Console.WriteLine();
#if true
                var results = TestReal(transform, 50, 2);
                foreach (var res in results)
                {
                    Console.Write("{0}, {1}, {2}, {3},", res.length, res.min, res.max, res.avg);
                    Console.Write("{0}, {1}, {2}, {3},", res.minR, res.maxR, res.avgR, (double)(res.min)/res.minR);
                    Console.WriteLine();
                }
#endif
            }


        }
    }
}
