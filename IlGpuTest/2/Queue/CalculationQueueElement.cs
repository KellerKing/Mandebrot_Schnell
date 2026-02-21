namespace IlGpuTest._2.Queue
{
    internal struct CalculationQueueElement
    {
        public double SkalaMinimialX { get; set; }
        public double SkalaMaximalX { get; set; }
        public double SkalaMinimialY { get; set; }
        public double SkalaMaximalY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int AnzahlIterationenMaximal { get; set; }
    }
}
