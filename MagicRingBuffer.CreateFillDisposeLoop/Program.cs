namespace MagicRingBuffer.CreateFillDisposeLoop
{
    internal class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 100_000_000; i++)
            {
                using var bf = new RingBuffer<byte>(1024 * 1024);
                if (i % 1000 == 0)
                {
                    bf.WriterSpan.Fill(11);
                    GC.Collect();
                }
            }
        }
    }
}
