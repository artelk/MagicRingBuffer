using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            get => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_ptr), _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int start)
            => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_ptr + start), _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int start, int length)
            => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_ptr + start), length);

        public ref T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(_ptr + index);
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
            get => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_ptr), _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetSpan(int start)
            => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_ptr + start), _length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetSpan(int start, int length)
            => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_ptr + start), length);

        public ref readonly T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(_ptr + index);
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
