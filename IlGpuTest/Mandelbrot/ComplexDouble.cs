namespace IlGpuTest.Mandelbrot
{
    public struct ComplexDouble
    {
        public double A { get; private set; }
        public double B { get; private set; }

        public double AbstandZumUrsprung => Math.Sqrt(Modulus);

        public bool GehtGegenUnendlich => Modulus >= 4;

        private double Modulus => A * A + B * B;


        public ComplexDouble(double a, double b)
        {
            A = a; B = b;
        }

        public ComplexDouble() {}

        public static ComplexDouble operator +(ComplexDouble x, ComplexDouble y) => new ComplexDouble(x.A + y.A, x.B + y.B);

        public static ComplexDouble operator *(ComplexDouble x, ComplexDouble y) => new ComplexDouble(x.A * y.A - x.B * y.B, x.A * y.B + x.B * y.A);

    }
}
