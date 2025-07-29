using BenchmarkDotNet.Attributes;

namespace MagicRingBuffer.Benchmark
{
    [ShortRunJob]
    [MemoryDiagnoser]
    public class Benchmarks
    {
        [Benchmark]
        public void CreateFillDispose()
        {
            using var bf = new RingBuffer<byte>(64 * 1024);
            bf.WriterSpan.Fill(11);
        }
    }
}
