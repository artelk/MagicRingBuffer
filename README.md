[![NuGet](https://img.shields.io/nuget/v/MagicRingBuffer)](https://www.nuget.org/packages/MagicRingBuffer)

# MagicRingBuffer
Fast ring (circular) buffer based on mapping the underlying memory segment to two contiguous regions of virtual memory.
Implemented for Windows and Linux platforms.

Explanation of how it works:
* https://fgiesen.wordpress.com/2012/07/21/the-magic-ring-buffer/
* https://lo.calho.st/posts/black-magic-buffer/
* https://ruby0x1.github.io/machinery_blog_archive/post/virtual-memory-tricks/index.html
* https://github.com/sklose/magic-buffer

# Usage
```cs
var buffer = new RingBuffer<uint>(1024);

while(...)
{
    var writerSpan = buffer.WriterSpan; // or WriterMemory or WriterChunk
    //... <write M bytes to the writerSpan>
    buffer.AdvanceWriter(M); // let buffer know how much was written

    Debug.Assert(buffer.ReaderSpan.Length + buffer.WriterSpan.Length == buffer.Size); // always true

    var readerSpan = buffer.ReaderSpan; // or ReaderMemory or ReaderChunk
    //... <read N bytes from the readerSpan>
    buffer.AdvanceReader(N); // let buffer know how much was read/consumed

    Debug.Assert(buffer.ReaderSpan.Length + buffer.WriterSpan.Length == buffer.Size); // always true
}
```