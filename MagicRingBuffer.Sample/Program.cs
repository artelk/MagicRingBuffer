namespace MagicRingBuffer.Sample
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            using var buffer = new RingBuffer<long>(1024 * 1024);

            long x = 0;
            long y = 0;

            for (int k = 0; k < 1000; k++)
            {
                if (buffer.WriterSpan.Length + buffer.ReaderSpan.Length != buffer.Size)
                    throw new Exception();

                var writerSpan = buffer.WriterSpan;
                var l = Random.Shared.Next(writerSpan.Length);
                writerSpan = writerSpan.Slice(0, l);

                for (int i = 0; i < writerSpan.Length; i++)
                    writerSpan[i] = x++;
                buffer.AdvanceWriter(l);

                if (buffer.WriterSpan.Length + buffer.ReaderSpan.Length != buffer.Size)
                    throw new Exception();

                var readerSpan = buffer.ReaderSpan;
                l = Random.Shared.Next(readerSpan.Length);
                readerSpan = readerSpan.Slice(0, l);

                for (int i = 0; i < readerSpan.Length; i++)
                    if (readerSpan[i] != y++)
                        throw new Exception("Not equal");
                buffer.AdvanceReader(l);

                Console.Write(".");
            }

            Console.WriteLine();
            Console.WriteLine("Done");
        }
    }
}
