namespace IlGpuTest.Mandelbrot
{
    internal class IteratorCpu :IIterator
    {
        public int Iterate(ComplexDouble c, int limit)
        {
            var z = new ComplexDouble();
            for (var i = 0; i < limit; i++)
            {
                z = z * z + c;
                if (z.GehtGegenUnendlich)
                    return i;
            }

            return limit;
        }

        public void IterateRange(ComplexDouble start, int width, int height, double skalierungsfaktor, int limit, int[,] output)
        {
            Parallel.For(0, height, y =>
            {
                Parallel.For(0, width, x =>
                {
                    var c = new ComplexDouble(start.A + x * skalierungsfaktor, start.B + y * skalierungsfaktor);
                    output[x, y] = Iterate(c, limit);
                });
            });
        }

        public override string ToString()
        {
            return "CPU";
        }
    }
}
