namespace MagicRingBuffer.Tests
{
    public class Tests
    {
        private RingBuffer<uint> buf;

        static Tests()
        {
            Console.Error.WriteLine($"AllocationGranularity = {RingBuffer.AllocationGranularity}");
        }

        [SetUp]
        public void Setup()
        {
            buf = new RingBuffer<uint>(16 * 1024);
        }

        [TearDown]
        public void TearDown()
        {
            buf.Dispose();
        }

        [Test]
        public void TestSpan()
        {
            var x = 0U;
            var y = 0U;

            for (int k = 0; k < 10; k++)
            {
                Assert.That(buf.ReaderSpan.Length + buf.WriterSpan.Length, Is.EqualTo(buf.Size));

                var writerSpan = buf.WriterSpan[..^331];
                for (int i = 0; i < writerSpan.Length; i++)
                    writerSpan[i] = x++;
                buf.AdvanceWriter(writerSpan.Length);
                Assert.That(buf.ReaderSpan.Length + buf.WriterSpan.Length, Is.EqualTo(buf.Size));

                var readerSpan = buf.ReaderSpan[..^113];
                for (int i = 0; i < readerSpan.Length; i++)
                    Assert.That(readerSpan[i], Is.EqualTo(y++));
                buf.AdvanceReader(readerSpan.Length);
                Assert.That(buf.WriterSpan.Length, Is.EqualTo(buf.Size - 113));
            }
        }

        [Test]
        public void TestChunk()
        {
            var x = 0U;
            var y = 0U;

            for (int k = 0; k < 10; k++)
            {
                Assert.That(buf.ReaderChunk.Length + buf.WriterChunk.Length, Is.EqualTo(buf.Size));

                var writerChunk = buf.WriterChunk[..^331];
                for (int i = 0; i < writerChunk.Length; i++)
                    writerChunk[i] = x++;
                buf.AdvanceWriter(writerChunk.Length);
                Assert.That(buf.ReaderChunk.Length + buf.WriterChunk.Length, Is.EqualTo(buf.Size));

                var readerChunk = buf.ReaderChunk[..^113];
                for (int i = 0; i < readerChunk.Length; i++)
                    Assert.That(readerChunk[i], Is.EqualTo(y++));
                buf.AdvanceReader(readerChunk.Length);
                Assert.That(buf.WriterChunk.Length, Is.EqualTo(buf.Size - 113));
            }
        }

        [Test]
        public void TestChunkSpan()
        {
            var x = 0U;
            var y = 0U;

            for (int k = 0; k < 10; k++)
            {
                Assert.That(buf.ReaderChunk.Span.Length, Is.EqualTo(buf.ReaderChunk.Length));
                Assert.That(buf.WriterChunk.Span.Length, Is.EqualTo(buf.WriterChunk.Length));
                Assert.That(buf.ReaderChunk.Length + buf.WriterChunk.Length, Is.EqualTo(buf.Size));

                var writerSpan = buf.WriterChunk.Span[..^331];
                Assert.That(writerSpan == buf.WriterChunk[..^331].Span);
                for (int i = 0; i < writerSpan.Length; i++)
                    writerSpan[i] = x++;
                buf.AdvanceWriter(writerSpan.Length);
                Assert.That(buf.ReaderChunk.Length + buf.WriterChunk.Length, Is.EqualTo(buf.Size));

                var readerSpan = buf.ReaderChunk.Span[..^113];
                Assert.That(readerSpan == buf.ReaderChunk[..^113].Span);
                for (int i = 0; i < readerSpan.Length; i++)
                    Assert.That(readerSpan[i], Is.EqualTo(y++));
                buf.AdvanceReader(readerSpan.Length);
                Assert.That(buf.WriterChunk.Length, Is.EqualTo(buf.Size - 113));
            }
        }

        [Test]
        public void TestMemory()
        {
            var x = 0U;
            var y = 0U;

            for (int k = 0; k < 10; k++)
            {
                Assert.That(buf.ReaderMemory.Span.Length, Is.EqualTo(buf.ReaderMemory.Length));
                Assert.That(buf.WriterMemory.Span.Length, Is.EqualTo(buf.WriterMemory.Length));
                Assert.That(buf.ReaderMemory.Length + buf.WriterMemory.Length, Is.EqualTo(buf.Size));

                var writerSpan = buf.WriterMemory.Span[..^331];
                Assert.That(writerSpan == buf.WriterMemory[..^331].Span);
                for (int i = 0; i < writerSpan.Length; i++)
                    writerSpan[i] = x++;
                buf.AdvanceWriter(writerSpan.Length);
                Assert.That(buf.ReaderMemory.Length + buf.WriterMemory.Length, Is.EqualTo(buf.Size));

                var readerSpan = buf.ReaderMemory.Span[..^113];
                Assert.That(readerSpan == buf.ReaderMemory[..^113].Span);
                for (int i = 0; i < readerSpan.Length; i++)
                    Assert.That(readerSpan[i], Is.EqualTo(y++));
                buf.AdvanceReader(readerSpan.Length);
                Assert.That(buf.WriterChunk.Length, Is.EqualTo(buf.Size - 113));
            }
        }
    }
}
