using BenchmarkDotNet.Running;

namespace MagicRingBuffer.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
            BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
