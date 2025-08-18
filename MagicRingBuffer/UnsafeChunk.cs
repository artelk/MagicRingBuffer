using System;
using System.Runtime.CompilerServices;

namespace MagicRingBuffer
{
    //Memory-like struct that works a bit faster
    public unsafe readonly struct UnsafeChunk<T>
        where T : unmanaged
    {
        private readonly T* _ptr;
        private readonly int _length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeChunk(T* ptr, int length)
        {
            _ptr = ptr;
            _length = length;
        }

        public Span<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Span<T>(_ptr, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int start)
            => new Span<T>(_ptr + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int start, int length)
            => new Span<T>(_ptr + start, length);

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(_ptr + index);
        }

        public T* Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ptr;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeChunk<T> Slice(int start)
            => new UnsafeChunk<T>(_ptr + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeChunk<T> Slice(int start, int length)
             => new UnsafeChunk<T>(_ptr + start, length);
    }

    //Memory-like struct that works a bit faster
    public unsafe readonly struct ReadOnlyUnsafeChunk<T>
        where T : unmanaged
    {
        private readonly T* _ptr;
        private readonly int _length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyUnsafeChunk(T* ptr, int length)
        {
            _ptr = ptr;
            _length = length;
        }

        public ReadOnlySpan<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ReadOnlySpan<T>(_ptr, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetSpan(int start)
            => new ReadOnlySpan<T>(_ptr + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetSpan(int start, int length)
            => new ReadOnlySpan<T>(_ptr + start, length);

        public ref readonly T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(_ptr + index);
        }

        public T* Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ptr;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyUnsafeChunk<T> Slice(int start)
            => new ReadOnlyUnsafeChunk<T>(_ptr + start, _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyUnsafeChunk<T> Slice(int start, int length)
             => new ReadOnlyUnsafeChunk<T>(_ptr + start, length);
    }
}
