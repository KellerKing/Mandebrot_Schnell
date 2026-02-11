namespace IlGpuTest._2.Calculation
{
    public interface ICalculator
    {
        void Calculate(Tuple<double, double> skalaX, Tuple<double, double> skalaY, int maxIterations, int[,] output);
    }
}
