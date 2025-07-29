using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicRingBuffer
{
    using static Utils;

    public static class RingBuffer
    {
        public static uint AllocationGranularity { get; private set; }

        static RingBuffer()
        {
            AllocationGranularity = PlatformInfo.IsWindows
                ? Windows.AllocationGranularity
                : (uint)Environment.SystemPageSize;
        }
    }

    public readonly struct RingBuffer<T> : IDisposable
        where T : unmanaged
    {
        public static uint AllocationGranularity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => RingBufferMemoryManager<T>.AllocationGranularity;
        }

        private readonly RingBufferMemoryManager<T> impl;

        /// <summary>
        /// Creates a new buffer with at least <paramref name="sizeHint"/> size.
        /// </summary>
        /// <param name="sizeHint">Minimal size for the buffer.</param>
        /// <remarks>
        /// The buffer byte size is rounded up to be the least multiple of both AllocationGranularity and sizeof(T).
        /// On Windows the AllocationGranularity is 64Kb (or 4Kb if that is supported).
        /// On non-Windows platforms it is equal to the page size (4Kb on Linux, 16Kb on macOS).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RingBuffer(int sizeHint) => impl = new RingBufferMemoryManager<T>(sizeHint);

        public uint Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => impl.Size;
        }

        public ReadOnlySpan<T> ReaderSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => impl.ReaderSpan;
        }

        public ReadOnlyUnsafeChunk<T> ReaderChunk
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => impl.ReaderChunk;
        }

        public ReadOnlyMemory<T> ReaderMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => impl.ReaderMemory;
        }

        public Span<T> WriterSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => impl.WriterSpan;
        }

        public UnsafeChunk<T> WriterChunk
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => impl.WriterChunk;
        }

        public Memory<T> WriterMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => impl.WriterMemory;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceWriter(int count) => impl.AdvanceWriter(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceReader(int count) => impl.AdvanceReader(count);

        public void Dispose() => (impl as IDisposable).Dispose();
    }

    internal sealed unsafe class RingBufferMemoryManager<T> : MemoryManager<T>, IDisposable
        where T : unmanaged
    {
        public static uint AllocationGranularity { get; }
        private T* _addr;
        private readonly uint _size;
        private uint _readerPos;
        private uint _writtenCount;

        static RingBufferMemoryManager()
        {
            var size = Lcm(RingBuffer.AllocationGranularity, (uint)sizeof(T));
            AllocationGranularity = size > uint.MaxValue ? uint.MaxValue : (uint)size;
        }

        public RingBufferMemoryManager(int sizeHint)
        {
            if (sizeHint <= 0) Throw.ArgumentOutOfRange(nameof(sizeHint), sizeHint, "Must be greater that 0");
            var bufferByteSize = (ulong)sizeHint * (uint)sizeof(T);
            var blockCount = (bufferByteSize + AllocationGranularity - 1) / AllocationGranularity;

            bufferByteSize = blockCount * AllocationGranularity;
            if (bufferByteSize >= uint.MaxValue)
                Throw.ArgumentOutOfRange(nameof(bufferByteSize), bufferByteSize, "Too long buffer");
            var size = bufferByteSize / (uint)sizeof(T);
            if (size > int.MaxValue)
                Throw.ArgumentOutOfRange(nameof(size), size, "Too long buffer");

            _size = (uint)size;
            _addr = PlatformInfo.IsWindows
                ? (T*)Windows.Alloc((uint)bufferByteSize)
                : (T*)UnixLike.Alloc((uint)bufferByteSize);
        }

        public uint Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _size;
        }

        public ReadOnlySpan<T> ReaderSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(Ptr + _readerPos), (int)_writtenCount);
        }

        public ReadOnlyUnsafeChunk<T> ReaderChunk
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ReadOnlyUnsafeChunk<T>(Ptr + _readerPos, (int)_writtenCount);
        }

        public ReadOnlyMemory<T> ReaderMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CreateMemory((int)_readerPos, (int)_writtenCount);
        }

        public Span<T> WriterSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var writerPos = _readerPos + _writtenCount;
                writerPos = Math.Min(writerPos, writerPos - _size);
                return MemoryMarshal.CreateSpan(
                    ref Unsafe.AsRef<T>(Ptr + writerPos),
                    (int)(_size - _writtenCount));
            }
        }

        public UnsafeChunk<T> WriterChunk
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var writerPos = _readerPos + _writtenCount;
                writerPos = Math.Min(writerPos, writerPos - _size);
                return new UnsafeChunk<T>(Ptr + writerPos, (int)(_size - _writtenCount));
            }
        }

        public Memory<T> WriterMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var writerPos = _readerPos + _writtenCount;
                writerPos = Math.Min(writerPos, writerPos - _size);
                return CreateMemory((int)writerPos, (int)(_size - _writtenCount));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceWriter(int count)
        {
            if (count < 0) Throw.ArgumentOutOfRange(nameof(count), count, "Negative");
            var newWrittenCount = (ulong)_writtenCount + (uint)count;
            if (newWrittenCount > _size)
                Throw.ArgumentOutOfRange(nameof(count), count, $"Cannot advance past the end of readed span, which has a size of {_size - _writtenCount}.");
            _writtenCount = (uint)newWrittenCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceReader(int count)
        {
            if (count < 0) Throw.ArgumentOutOfRange(nameof(count), count, "Negative");
            var newWrittenCount = (long)_writtenCount - (uint)count;
            if (newWrittenCount < 0)
                Throw.ArgumentOutOfRange(nameof(count), count, $"Cannot advance past the end of written span, which has a size of {_writtenCount}.");
            _writtenCount = (uint)newWrittenCount;
            var newReaderPos = (ulong)_readerPos + (uint)count;
            _readerPos = (uint)Math.Min(newReaderPos, newReaderPos - _size);
        }

        private T* Ptr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var p = _addr;
                if (p == (T*)0)
                    Throw.ObjectDisposed("RingBuffer");
                return p;
            }
        }

        ~RingBufferMemoryManager() => Dispose(false);

        protected override void Dispose(bool disposing)
        {
            if (_addr == (byte*)0) return;
            if (PlatformInfo.IsWindows)
                Windows.Free((byte*)_addr, _size * (uint)sizeof(T));
            else
                UnixLike.Free((byte*)_addr, _size * (uint)sizeof(T));

            _addr = (T*)0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Span<T> GetSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(Ptr), (int)(_size << 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MemoryHandle Pin(int elementIndex = 0) => new MemoryHandle(Ptr + elementIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Unpin() { }
    }
}
