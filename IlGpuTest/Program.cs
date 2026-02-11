// See https://aka.ms/new-console-template for more information
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using IlGpuTest._2.Calculation;
using IlGpuTest._2.Ui;
using IlGpuTest.Mandelbrot;
using System.Diagnostics;

//Console.WriteLine("Hello, World!");



//new Main().Start();


//Console.ReadLine();


var ui = new Form1(new CalculatorNaiv());

ui.ShowMandelbrot();




static void Gpu()
{
    // Define array size
    var n = 1024 * 1024 * 10;
    // Create input arrays
    var a = new float[n];
    var b = new float[n];
    var c = new float[n];
    var rand = new Random();
    for (int i = 0; i < n; i++)
    {
        a[i] = (float)rand.NextDouble();
        b[i] = (float)rand.NextDouble();
    }


    using var context = Context.CreateDefault();


    Stopwatch sw = new Stopwatch();

    // For each available device...
    foreach (var device in context.GetCudaDevices())
    {
        sw.Start();
        // Create accelerator for the given device
        using var accelerator = device.CreateAccelerator(context);
        Console.WriteLine($"Performing operations on {accelerator}");

        var a_dev = accelerator.Allocate1D<float>(n);
        var b_dev = accelerator.Allocate1D<float>(n);
        var c_dev = accelerator.Allocate1D<float>(n);

        // Copy input arrays to device memory
        a_dev.CopyFromCPU(a);
        b_dev.CopyFromCPU(b);
        // Define kernel
        var add_kernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>(AddKernel);
        // Launch kernel
        add_kernel(n - 1, a_dev.View, b_dev.View, c_dev.View);

        // Reads data from the GPU buffer into a new CPU array.
        // Implicitly calls accelerator.DefaultStream.Synchronize() to ensure
        // that the kernel and memory copy are completed first.
        c_dev.CopyToCPU(c);


        //var data3 = c_dev.GetAsArray1D();
        sw.Stop();
        Console.WriteLine($"Gpu: {sw.ElapsedMilliseconds} ms");
        sw.Reset();
    }

    for (var i = 0; i < c.Length; i++) c[i] = 0;

    sw.Restart();

    Parallel.For(0, n, i =>
    {
        c[i] = a[i] + b[i];
    });
    sw.Stop();
    Console.WriteLine($"Cpu: {sw.ElapsedMilliseconds} ms");
}


///Kernel Funktion
static void AddKernel(Index1D index, ArrayView<float> a, ArrayView<float> b, ArrayView<float> c)
{
    c[index] = a[index] * a[index] + b[index];
}