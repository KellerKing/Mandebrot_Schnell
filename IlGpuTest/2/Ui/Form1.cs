using IlGpuTest._2.Calculation;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IlGpuTest._2.Ui
{
    public partial class Form1 : Form
    {
        private readonly ICalculator m_Calculator;
        private readonly PictureBox m_PictureBox;
        private Tuple<double, double> m_CurrentSkalaX;
        private Tuple<double, double> m_CurrentSkalaY;
        private readonly int m_MaxIterations = 100;
        private bool m_IsCalculating = false;
        
        private bool m_IsDrag;

        public Form1(ICalculator calculator)
        {
            m_Calculator = calculator;
            m_PictureBox = new PictureBox();
            InitializeComponent();
        }

        public void ShowMandelbrot()
        {
            m_CurrentSkalaX = new Tuple<double, double>(-2, 1);
            m_CurrentSkalaY = new Tuple<double, double>(-1.2, 1.2);

            var width = 1280;
            var height = 720;

            Width = width;
            Height = height;

            var bitmap = DrawMandelbrot(m_CurrentSkalaX, m_CurrentSkalaY, width, height, m_MaxIterations);
            Controls.Add(m_PictureBox);
            m_PictureBox.Dock = DockStyle.Fill;
            m_PictureBox.Image = bitmap;
            m_PictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            m_PictureBox.MouseWheel += Picturebox_MouseWheel;
            m_PictureBox.MouseDown += PictureBox_MouseDown;
            m_PictureBox.MouseUp += PictureBox_MouseUp;
            m_PictureBox.MouseMove += PictureBox_MouseMove;

            ShowDialog();
        }

        private void PictureBox_MouseUp(object? sender, MouseEventArgs e) => m_IsDrag = false;

        private void PictureBox_MouseDown(object? sender, MouseEventArgs e)
        {
            m_IsDrag = e.Button == MouseButtons.Left;
        }

        private void PictureBox_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!(e.Button == MouseButtons.Left)) return;

            var (mouseReX, mouseImY) = MausImKoordinatenSystem(e.X, e.Y, m_CurrentSkalaX, m_CurrentSkalaY, Width, Height);
            Console.WriteLine($"X: {mouseReX}, Y: {mouseImY}");
            // 3. Neue Grenzen berechnen (Interpolation zum Mauspunkt)
            m_CurrentSkalaX = new Tuple<double, double>(
                mouseReX + m_CurrentSkalaX.Item1,
                mouseReX + m_CurrentSkalaX.Item2
            );

            m_CurrentSkalaY = new Tuple<double, double>(
                mouseImY + m_CurrentSkalaY.Item1,
                mouseImY + m_CurrentSkalaY.Item2
            );

            m_IsCalculating = true;

            var bitmap = DrawMandelbrot(m_CurrentSkalaX, m_CurrentSkalaY, Width, Height, m_MaxIterations);

            m_PictureBox.Image.Dispose();
            m_PictureBox.Image = bitmap;
            m_IsCalculating = false;
        }

        private Bitmap DrawMandelbrot(Tuple<double, double> skalaX, Tuple<double, double> skalaY, int width, int height, int maxIterations)
        {
            var output = new int[width, height];
            m_Calculator.Calculate(skalaX, skalaY, maxIterations, output);

            //var bitmap = new Bitmap(Width, Height);

            //var sw = Stopwatch.StartNew();

            //for (var y = 0; y < height; y++)
            //{
            //    for (var x = 0; x < width; x++)
            //    {
            //        var color = (int)(255.0 * output[x, y] / maxIterations);
            //        bitmap.SetPixel(x, y, Color.FromArgb(color, color, color));
            //    }
            //}

            //sw.Stop();
            //output = null;

            //return bitmap;



            output = CodiereFarbe(output, maxIterations);
            return RenderBitmap(output);
        }

        private static int[,] CodiereFarbe(int[,] pixel, int maxIterations)
        {
            var sw = Stopwatch.StartNew();

            Parallel.For(0, pixel.GetLength(1), y =>
            {
                Parallel.For(0, pixel.GetLength(0), x =>
                {
                    var color = (int)(255.0 * pixel[x, y] / maxIterations);
                    pixel[x, y] = Color.FromArgb(color, color, color).ToArgb();
                });
            });
            sw.Stop();

            return pixel;

        }

        private static Bitmap RenderBitmap(int[,] pixel)
        {
            var sw = Stopwatch.StartNew();
            var bits = new int[pixel.GetLength(0) * pixel.GetLength(1)];
            var handle = GCHandle.Alloc(bits, GCHandleType.Pinned);
            var bitmap = new Bitmap(pixel.GetLength(0), pixel.GetLength(1), pixel.GetLength(0) * 4, PixelFormat.Format32bppRgb, handle.AddrOfPinnedObject());

            Parallel.For(0, pixel.GetLength(1), y =>
            {
                Parallel.For(0, pixel.GetLength(0), x =>
                {
                    var index = x + (y * pixel.GetLength(0));
                    bits[index] = pixel[x, y];
                });
            });
            handle.Free();
            bits = null;

            sw.Stop();
            return bitmap;
        }

        private void Picturebox_MouseWheel(object? sender, MouseEventArgs e)
        {

            if (m_IsCalculating || ModifierKeys != Keys.Control) return;

            var (mouseReX, mouseImY) = MausImKoordinatenSystem(e.X, e.Y, m_CurrentSkalaX, m_CurrentSkalaY, Width, Height);

            var zoomFactor = e.Delta > 0 ? 1 / 1.25 : 1.25;

            // 3. Neue Grenzen berechnen (Interpolation zum Mauspunkt)
            m_CurrentSkalaX = new Tuple<double, double>(
                mouseReX + (m_CurrentSkalaX.Item1 - mouseReX) * zoomFactor,
                mouseReX + (m_CurrentSkalaX.Item2 - mouseReX) * zoomFactor
            );

            m_CurrentSkalaY = new Tuple<double, double>(
                mouseImY + (m_CurrentSkalaY.Item1 - mouseImY) * zoomFactor,
                mouseImY + (m_CurrentSkalaY.Item2 - mouseImY) * zoomFactor
            );

            m_IsCalculating = true;

            var bitmap = DrawMandelbrot(m_CurrentSkalaX, m_CurrentSkalaY, Width, Height, m_MaxIterations);

            m_PictureBox.Image.Dispose();
            m_PictureBox.Image = bitmap;
            m_IsCalculating = false;
        }

        private static Tuple<double, double> MausImKoordinatenSystem(int xMaus, int yMaus, Tuple<double, double> skalaX, Tuple<double, double> skalaY, int width, int height)
        {
            double mouseReX = skalaX.Item1 + (double)xMaus / width * (skalaX.Item2 - skalaX.Item1);
            double mouseImY = skalaY.Item1 + (double)yMaus / height * (skalaY.Item2 - skalaY.Item1);

            return Tuple.Create(mouseReX, mouseImY);
        }
    }
}
