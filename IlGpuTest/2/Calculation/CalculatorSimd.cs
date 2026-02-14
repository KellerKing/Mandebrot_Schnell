using IlGpuTest._2.Struct;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace IlGpuTest._2.Calculation
{
    internal class CalculatorSimd : ICalculator
    {
        // Lane-Indizes: [0, 1, 2, 3, ...]
        private static readonly Vector<float> LaneOffsets = CreateLaneOffsets(); 
        
        private static Vector<float> CreateLaneOffsets() 
        { 
            var arr = new float[Vector<float>.Count]; 
            for (int i = 0; i < arr.Length; i++) 
                arr[i] = i; 

            return new Vector<float>(arr); 
        }

        public void Calculate(Tuple<double, double> skalaX, Tuple<double, double> skalaY, int maxIterations,
            int[,] output)
        {

            throw new NotImplementedException();

        }


        public static int IterateVectorized(Vector<float> x0, Vector<float> y0, int maxIter, Span<int> outIters)
        {
            // Wir zählen Iterationen pro Lane in einem float-Vektor.
            var x = x0;
            var y = y0;

            var iter = Vector<float>.Zero;
            var one = new Vector<float>(1f);
            var four = new Vector<float>(4f);

            for (int i = 0; i<maxIter; i++)
            {
                var xx = x * x;
                var yy = y * y;
                var mag2 = xx + yy;

                // Maske: noch nicht divergiert?
                var notEscapedMask = Vector.LessThan(mag2, four);

                // iter += notEscaped ? 1 : 0
                iter = Vector.ConditionalSelect(notEscapedMask, iter + one, iter);

                // Abbruch: wenn alle Lanes bereits escapet sind → optionaler Early-Out
                if (AllFalse(notEscapedMask))
                    break;

                // z = z^2 + c
                var twoXY = (x * y) + (x * y); // 2*x*y
                x = (xx - yy) + x0;
                y = twoXY + y0;
            }

            // iter (float) → outIters (int)
            // Achtung: ConditionalSelect hat floats beibehalten; wir casten jetzt lane-weise.
            for (int lane = 0; lane<Vector<float>.Count; lane++)
            {
                outIters[lane] = (int) iter[lane];
            }

            return Vector<float>.Count;
        }

            
        /// <summary>
        /// Hilfsfunktion: Prüft, ob alle Lanes false sind.
        /// </summary>
        private static bool AllFalse(Vector<int> mask)
        {
            // Für ConditionalSelect/Gleichheitsvergleiche erzeugt .NET bool-like masks,
            // die intern als all-ones oder all-zeroes pro Lane kodiert sind.
            // Ein Trick ist, Vector.Equals(mask, Vector<float>.Zero) – aber hier brauchen wir
            // 'alle false', also alle Lanes = 0.
            for (int i = 0; i < Vector<float>.Count; i++)
            {
                if (mask[i] != 0)
                    return false;
            }
            return true;

        }

        private static bool AllZero(Vector<int> v)
        {
            // true, wenn alle Lanes == 0
            return Vector.Equals(v, Vector<int>.Zero).Equals(Vector<int>.One);
        }

        public void Calculate(Tuple<double, double> skalaX, Tuple<double, double> skalaY, int maxIterations, int[] output, int width, int height)
        {
            var schrwittweitePixelX = (float)(skalaX.Item2 - skalaX.Item1) / width;
            var schrwittweitePixelY = (float)(skalaY.Item2 - skalaY.Item1) / height;

            Parallel.For(0, height, y =>
            {
                var cy = (float)skalaY.Item1 + y * schrwittweitePixelY;
                int rowOffset = y * width;

                for (int x = 0; x < width; x += Vector<float>.Count)
                {
                    int remaining = width - x;
                    int lanes = Math.Min(Vector<float>.Count, remaining);

                    float baseX = (float)skalaX.Item1 + x * schrwittweitePixelX;
                    var cx = new Vector<float>(baseX) + LaneOffsets * schrwittweitePixelX;
                    var cyVec = new Vector<float>(cy);

                    var zx = Vector<float>.Zero;
                    var zy = Vector<float>.Zero;

                    var iter = Vector<int>.Zero;
                    var one = Vector<int>.One;
                    var four = new Vector<float>(4);

                    for (var i = 0; i < maxIterations; i++)
                    {
                        var xx = zx * zx;
                        var yy = zy * zy;
                        var mag2 = xx + yy;

                        // Maske: noch nicht escaped (mag2 < 4)
                        var maskF = Vector.LessThan(mag2, four);

                        // Early-out: alle Lanes escaped?
                        if (AllZero(maskF))
                            break;

                        // iter += (mask & 1)
                        iter += (maskF & one);

                        // z = z^2 + c, aber nur für aktive Lanes
                        var twoXY = (zx * zy) + (zx * zy);
                        var zxNew = (xx - yy) + cx;
                        var zyNew = twoXY + cyVec;

                        zx = Vector.ConditionalSelect(maskF, zxNew, zx);
                        zy = Vector.ConditionalSelect(maskF, zyNew, zy);
                    }

                    // Ergebnisse zurück in den Output-Buffer
                    for (var lane = 0; lane < lanes; lane++)
                    {
                        output[rowOffset + x + lane] = iter[lane];
                    }
                }
            });
        }
    }





    //float* x0,
    //float* y0,
    //int maxIter,
    //int* output)
    //{
    //    if (!Avx.IsSupported)
    //        throw new PlatformNotSupportedException("AVX wird benötigt");

    //    // Register laden
    //    var vx0 = Avx.LoadVector256(x0);
    //    var vy0 = Avx.LoadVector256(y0);

    //    var x = vx0;
    //    var y = vy0;

    //    var iter = Vector256<float>.Zero;
    //    var one = Vector256.Create(1.0f);
    //    var four = Vector256.Create(4.0f);

    //    for (int i = 0; i<maxIter; i++)
    //    {
    //        var xx = Avx.Multiply(x, x);
    //        var yy = Avx.Multiply(y, y);
    //        var mag2 = Avx.Add(xx, yy);

    //        var mask = Avx.Compare(mag2, four, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling);

    //        // Wenn alle divergiere → Break
    //        if (Avx.MoveMask(mask) == 0)
    //            break;

    //        iter = Avx.Add(iter, Avx.And(mask, one));

    //        var xy = Avx.Multiply(x, y);
    //        var twoXY = Avx.Add(xy, xy);

    //        x = Avx.Add(Avx.Subtract(xx, yy), vx0);
    //        y = Avx.Add(twoXY, vy0);
    //    }

    //    // Iterationen zurückschreiben
    //    int* temp = stackalloc int[8];
    //    var iterInt = Avx.ConvertToVector256Int32(iter);
    //    Avx.Store((int*) temp, iterInt);

    //    for (int i = 0; i< 8; i++)
    //        output[i] = temp[i];
    //}

}
