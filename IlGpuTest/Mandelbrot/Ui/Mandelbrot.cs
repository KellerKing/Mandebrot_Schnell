namespace IlGpuTest.Mandelbrot.Ui
{
    public partial class Mandelbrot : Form
    {
        private readonly IIterator m_Renderer;
        private readonly Bitmap m_Bitmap;

        public Mandelbrot(IIterator renderer)
        {
            InitializeComponent();
            m_Renderer = renderer;

            Width = 1920;
            Height = 1080;

            m_Bitmap = new Bitmap(Width, Height);
        }


        public void ShowMandelbrot()
        {
            var pixel = new int[m_Bitmap.Width, m_Bitmap.Height];
            var limit = 200_000;
            m_Renderer.IterateRange(new ComplexDouble(-2, -1), pixel.GetLength(0), pixel.GetLength(1), 0.003, limit, pixel);
            using var brushBl = new SolidBrush(Color.Black);
            using var p = Graphics.FromImage(m_Bitmap);

            for (var y = 0; y < m_Bitmap.Height; y++)
            {
                for (var x = 0; x < m_Bitmap.Width; x++)
                {
                    if (pixel[x, y] >= limit)
                        p.FillRectangle(brushBl, x, y, 1, 1);
                }
            }

            this.ShowDialog();
        }

        private void Mandelbrot_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(m_Bitmap, 0, 0);
        }
    }
}
