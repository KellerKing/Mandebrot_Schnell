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
        private readonly int m_MaxIterations = 5;
        private bool m_IsCalculating = false;

        private bool m_IsDrag;
        private Tuple<double, double>? m_PositionMausLast;
        private Point? m_PositionMausLastPixel;

        public Form1(ICalculator calculator)
        {
            m_Calculator = calculator;
            m_PictureBox = new PictureBox();
            InitializeComponent();
        }

        public void ShowMandelbrot()
        {
            //m_CurrentSkalaX = new Tuple<double, double>(-0.819444732921077, -0.8194401354344543);
            //m_CurrentSkalaY = new Tuple<double, double>(-0.1731612926638697, -0.17315761467457164);
            m_CurrentSkalaX = new Tuple<double, double>(-2, 1);
            m_CurrentSkalaY = new Tuple<double, double>(-1.2, 1.2);

            //var width = 17321;
            //var height = 11547;

            var width = 1280;
            var height = 720;

            Width = width;
            Height = height;

            Update(m_CurrentSkalaX, m_CurrentSkalaY, width, height, m_MaxIterations);
            //bitmap.Save("mandelbrot.png", ImageFormat.Png);
            //return;
            Controls.Add(m_PictureBox);
            m_PictureBox.Dock = DockStyle.Fill;
            m_PictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            m_PictureBox.MouseWheel += Picturebox_MouseWheel;
            m_PictureBox.MouseDown += PictureBox_MouseDown;
            m_PictureBox.MouseUp += PictureBox_MouseUp;
            m_PictureBox.MouseMove += PictureBox_MouseMove;
            m_PictureBox.MouseLeave += M_PictureBox_MouseLeave; ;

            ShowDialog();
        }

        private void M_PictureBox_MouseLeave(object? sender, EventArgs e) => ResetDrag();
        private void PictureBox_MouseUp(object? sender, MouseEventArgs e) => ResetDrag();

        private void PictureBox_MouseDown(object? sender, MouseEventArgs e)
        {
            m_IsDrag = e.Button == MouseButtons.Left;
            m_PositionMausLast = MausImKoordinatenSystem(e.X, e.Y, m_CurrentSkalaX, m_CurrentSkalaY, Width, Height);
        }

        private void ResetDrag()
        {
            m_PositionMausLast = null;
            m_IsDrag = false;
            m_PositionMausLastPixel = null;
        }


        private void PictureBox_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!m_IsDrag) return;

            if (m_PositionMausLast == null || !m_PositionMausLastPixel.HasValue)
            {
                m_PositionMausLast = MausImKoordinatenSystem(e.X, e.Y, m_CurrentSkalaX, m_CurrentSkalaY, Width, Height);
                m_PositionMausLastPixel = e.Location;
                return;
            }

            var (mouseReX, mouseImY) = MausImKoordinatenSystem(e.X, e.Y, m_CurrentSkalaX, m_CurrentSkalaY, Width, Height);
            
            var prozentAbweichungX = Math.Abs(Convert.ToDouble(m_PositionMausLastPixel.Value.X - e.X)) / Width * 100.0;
            var prozentAbweichungY = Math.Abs(Convert.ToDouble(m_PositionMausLastPixel.Value.Y - e.Y)) / Height * 100.0;

            //var prozentAbweichungX = Math.Abs(m_PositionMausLast.Item1 - mouseReX) / m_PositionMausLast.Item1 * 100;
            //var prozentAbweichungY = Math.Abs(m_PositionMausLast.Item2 - mouseImY) / m_PositionMausLast.Item2 * 100;

            //Console.WriteLine($"Old: {m_PositionMausLastPixel.Value.X} / {m_PositionMausLastPixel.Value.Y}");
            //Console.WriteLine($"NEw: {e.X} / {e.Y}");
            //Console.WriteLine($"{prozentAbweichungX}% / {prozentAbweichungY}%");
            //Console.WriteLine($"{Math.Abs(m_PositionMausLastPixel.Value.X - e.X)} / {m_PositionMausLastPixel.Value.Y - e.Y}");

            if (Math.Abs(prozentAbweichungX) <= 0.5 && Math.Abs(prozentAbweichungY) <= 0.5)
                return;
            
            Console.WriteLine($"{prozentAbweichungX}% / {prozentAbweichungY}%");

            var isNachRechts = m_PositionMausLast.Item1 < mouseReX;
            var isNachUnten = m_PositionMausLast.Item2 < mouseImY;

            var offsetX = Math.Abs(m_PositionMausLast.Item1 - mouseReX);
            if (!isNachRechts) offsetX *= -1;

            var offsetY = Math.Abs(m_PositionMausLast.Item2 - mouseImY);
            if (!isNachUnten) offsetY *= -1;

            m_CurrentSkalaX = new Tuple<double, double>(
                m_CurrentSkalaX.Item1 - offsetX,
                m_CurrentSkalaX.Item2  - offsetX
            );

            m_CurrentSkalaY = new Tuple<double, double>(
                m_CurrentSkalaY.Item1 - offsetY,
                m_CurrentSkalaY.Item2 - offsetY
            );

            Update(m_CurrentSkalaX, m_CurrentSkalaY, Width, Height, m_MaxIterations);
    
            m_IsCalculating = false;
            m_PositionMausLast = MausImKoordinatenSystem(e.X, e.Y, m_CurrentSkalaX, m_CurrentSkalaY, Width, Height);
            m_PositionMausLastPixel = e.Location;
        }

        private Bitmap DrawMandelbrot(Tuple<double, double> skalaX, Tuple<double, double> skalaY, int width, int height, int maxIterations)
        {
            var sw = Stopwatch.StartNew();
            var output = new int[width * height];
            sw.Stop();

            Console.WriteLine($"Array initialisieren: {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            m_Calculator.Calculate(skalaX, skalaY, maxIterations, output, width, height);
            sw.Stop();

            Console.WriteLine($"Calculation: {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            output = CodiereFarbe(output, width, height, maxIterations);
            sw.Stop();

            Console.WriteLine($"Farbe codieren: {sw.ElapsedMilliseconds}ms");

            return RenderBitmap(output, width, height);
        }

        private static int[] CodiereFarbe(int[] pixel, int width, int height, int maxIterations)
        {
            var sw = Stopwatch.StartNew();

            Parallel.For(0, height, y =>
            {
                Parallel.For(0, width, x =>
                {
                    var index = Helper.FlacherIndex(width, x, y);
                    var color = (int)(255.0 * pixel[index] / maxIterations);
                    pixel[index] = Color.FromArgb(color, color, color).ToArgb();
                });
            });

            sw.Stop();

            return pixel;
        }

        private static Bitmap RenderBitmap(int[] pixel, int width, int height)
        {
            var sw = Stopwatch.StartNew();
            var handle = GCHandle.Alloc(pixel, GCHandleType.Pinned);
            var bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb, handle.AddrOfPinnedObject());

            handle.Free();
            pixel = null;

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

            Console.WriteLine($"Xmin: {m_CurrentSkalaX.Item1} | Xmax: {m_CurrentSkalaX.Item2}");
            Console.WriteLine($"Ymin: {m_CurrentSkalaY.Item1} | Ymax: {m_CurrentSkalaY.Item2}");

            m_IsCalculating = true;

            Update(m_CurrentSkalaX, m_CurrentSkalaY, Width, Height, m_MaxIterations);
        }

        private void Update(Tuple<double, double> skalaX, Tuple<double, double> skalaY, int width, int height, int iterationenMaximal)
        {
            var bitmap = DrawMandelbrot(skalaX, skalaY, width, height, iterationenMaximal);

            m_PictureBox.Image?.Dispose();
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
