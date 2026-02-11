using System.Numerics;

namespace IlGpuTest._2.Calculation
{
    internal class CalculatorSimd : ICalculator
    {
        public void Test()
        {
            

            //Bitmap bmp = new Bitmap(width, height);

            //// Kartierung: Bereich über der komplexen Ebene
            //double xmin = -2.0, xmax = 1.0;
            //double ymin = -1.5, ymax = 1.5;

            //for (int px = 0; px < width; px++)
            //{
            //    for (int py = 0; py < height; py++)
            //    {
            //        double x = xmin + px * (xmax - xmin) / width;
            //        double y = ymin + py * (ymax - ymin) / height;

            //        var c = new Complex(x, y);
            //        int iter = MandelbrotIterations(c, maxIter);

            //        // Einfaches Coloring
            //        int color = (int)(255.0 * iter / maxIter);
            //        bmp.SetPixel(px, py, Color.FromArgb(color, color, color));
            //    }
            //}

            //return bmp;


            ////var m1 = new Matrix4x4(null);
            ////Matrix4x4.Transpose()
        }

        public void Calculate(Tuple<double, double> skalaX, Tuple<double, double> skalaY, int maxIterations, int[,] output)
        {
            throw new NotImplementedException();
        }
    }
}
