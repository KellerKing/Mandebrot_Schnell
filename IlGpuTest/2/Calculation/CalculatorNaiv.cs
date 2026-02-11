using IlGpuTest._2.Struct;
using System.Diagnostics;

namespace IlGpuTest._2.Calculation
{
    internal class CalculatorNaiv : ICalculator
    {
        public void Calculate(Tuple<double, double> skalaX, Tuple<double, double> skalaY, int maxIterations, int[,] output)
        {
            var sw = Stopwatch.StartNew();

            Parallel.For(0, output.GetLength(1), y =>
            {
                Parallel.For(0, output.GetLength(0), x =>
                {

                    var xPosKoordinatenSystem = skalaX.Item1 + x * (skalaX.Item2 - skalaX.Item1) / output.GetLength(0);
                    var yPosKoordinatenSystem = skalaY.Item1 + y * (skalaY.Item2 - skalaY.Item1) / output.GetLength(1);

                    var z = new KomplexeZahl();
                    var c = new KomplexeZahl(Convert.ToSingle(xPosKoordinatenSystem), Convert.ToSingle(yPosKoordinatenSystem));

                    for (var i = 0; i < maxIterations; i++)
                    {
                        z = z * z + c;
                        if (z.GehtGegenUnendlich)
                        {
                            output[x, y] = i;
                            break;
                        }
                    }
                });
            });

            sw.Stop();
        }
    }
}
