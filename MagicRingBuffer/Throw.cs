using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicRingBuffer
{
    internal static class Throw
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Win32ExceptionWithCurrentError()
            => throw new Win32Exception(Marshal.GetLastWin32Error());

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Win32Exception(int error)
            => throw new Win32Exception(error);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ArgumentOutOfRange(string paramName, object actualValue, string message)
            => throw new ArgumentOutOfRangeException(paramName, actualValue, message);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ObjectDisposed(string objectName)
            => throw new ObjectDisposedException(objectName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void NotSupported()
            => throw new NotSupportedException();
    }
}
