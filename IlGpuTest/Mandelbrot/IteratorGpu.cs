using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;

namespace IlGpuTest.Mandelbrot
{
    internal class IteratorGpu : IDisposable, IIterator
    {
        private readonly Context m_Context;
        private readonly Accelerator m_Accelerator;
        private Action<Index2D, ComplexDouble, double, int, ArrayView2D<int, Stride2D.DenseY>> m_Delegate;

        public IteratorGpu()
        {
            //TODO: Eigentlich mit Usings
            m_Context = Context.CreateDefault();
            var device = m_Context.GetCudaDevice(0);
            var cuda = device.CreateCudaAccelerator(m_Context);
            m_Accelerator = cuda;

            m_Delegate = m_Accelerator.LoadAutoGroupedStreamKernel<Index2D, ComplexDouble, double, int, ArrayView2D<int, Stride2D.DenseY>>(Iterate);
        }

        public void Dispose()
        {
            m_Context.Dispose();
            m_Accelerator.Dispose();
            m_Delegate = null;
        }

        public void IterateRange(ComplexDouble start, int width, int height, double skalierungsfaktor, int limit, int[,] output)
        {
            using var buffer = m_Accelerator.Allocate2DDenseY<int>(new LongIndex2D(width, height));

            m_Delegate(new Index2D(width, height), start, skalierungsfaktor, limit, buffer.View);

            buffer.CopyToCPU(output);
        }

        public static void Iterate(Index2D index, ComplexDouble start, double skalierungsfaktor, int limit, ArrayView2D<int, Stride2D.DenseY> output)
        {
            var c = new ComplexDouble(start.A + index.X * skalierungsfaktor, start.B + index.Y * skalierungsfaktor);
            var z = new ComplexDouble();

            var iterations = 0;
            for (; iterations < limit; iterations++)
            {
                z = z * z + c;

                if (z.GehtGegenUnendlich)
                    break;
            }

            output[index.X, index.Y] = iterations;
        }

        public override string ToString()
        {
            return "GPU";
        }
    }
}
