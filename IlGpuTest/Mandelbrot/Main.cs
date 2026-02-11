using System.Diagnostics;

namespace IlGpuTest.Mandelbrot
{
    internal class Main
    {

        public void Start()
        {
            var renderer = new IteratorGpu();
            var mandelbrot = new Ui.Mandelbrot(renderer);
            mandelbrot.ShowMandelbrot();

        }

        public void Test()
        {
            var gpu = new IteratorGpu();
            var cpu = new IteratorCpu();

            var cpuResult = Test(cpu);
            var gpuResult = Test(gpu);

            Compare(cpuResult, gpuResult);

            gpu.Dispose();
        }


        private void Compare(int[,] cpu, int[,] gpu)
        {
            for (var y = 0;  y < gpu.GetLength(1); y++)
            {
                for(var x = 0; x < gpu.GetLength(0); x++)
                {
                    if (cpu[x, y] != gpu[x, y])
                        throw new Exception($"x: {x}, y: {y}");
                }
            }
        }


        private int[,] Test(IIterator iterator)
        {
            var width = 6000;
            var height = 4000;
            var start = new ComplexDouble();
            var skalierung = 0.000001;
            var limit = 1000;
            var output = new int[width, height];

            var sw = Stopwatch.StartNew();

            iterator.IterateRange(start, width, height, skalierung, limit, output);
        
            sw.Stop();
            Console.WriteLine($"{iterator.ToString()}, {sw.ElapsedMilliseconds} ms");

            return output;
        }
    }
}
