namespace IlGpuTest.Mandelbrot
{
    public interface IIterator
    {
        void IterateRange(ComplexDouble start, int width, int height, double skalierungsfaktor, int limit, int[,] output);
    }
}
