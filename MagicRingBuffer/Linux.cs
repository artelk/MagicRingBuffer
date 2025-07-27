using System.Runtime.InteropServices;
using System;

namespace MagicRingBuffer
{
    internal unsafe static class Linux
    {
        public readonly static uint AllocationGranularity = (uint)Environment.SystemPageSize;
        private static readonly byte[] shmName = System.Text.Encoding.ASCII.GetBytes("/ringbuffershm\0");
        public static byte* Alloc(uint len)
        {
            int fd;
            fixed (byte* namePtr = shmName)
            {
                fd = shm_open(namePtr, O_CREAT | O_RDWR, S_IRUSR | S_IWUSR);
                if (fd == -1)
                    Throw.Win32ExceptionWithCurrentError();
                shm_unlink(namePtr);
            }

            if (ftruncate(fd, len) == -1)
                Throw.Win32ExceptionWithCurrentError();

            var ptr = mmap((byte*)0, (long)len * 2, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);

            if (ptr == (byte*)-1)
                Throw.Win32ExceptionWithCurrentError();

            try
            {
                var ptr2 = mmap(ptr + len, len, PROT_READ | PROT_WRITE, MAP_SHARED | MAP_FIXED, fd, 0);
                if (ptr == (byte*)-1)
                    Throw.Win32ExceptionWithCurrentError();
            }
            catch
            {
                munmap(ptr, (long)len * 2);
                throw;
            }

            return ptr;
        }

        public static void Free(byte* addr, uint len)
        {
            munmap(addr, (long)len * 2);
        }

        [DllImport("libc", SetLastError = true)]
        public static extern int shm_open(byte* name, int flag, int mode);

        [DllImport("libc", SetLastError = true)]
        internal static extern int shm_unlink(byte* name);
        //shm_open(name, NativeMethods.O_RDWR | NativeMethods.O_CREAT, S_IRUSR);

        [DllImport("libc", SetLastError = true)]
        public static extern int ftruncate(int fd, long length);

        [DllImport("libc", SetLastError = true)]
        public static extern byte* mmap(byte* addr, long length, int prot, int flags, int fd, long offset);

        [DllImport("libc", SetLastError = true)]
        public static extern byte* munmap(byte* addr, long length);

        public const int O_RDWR = 2;
        public const int O_CREAT = 64;
        public const int S_IRUSR = 0x400;
        public const int S_IWUSR = 0x200;
        public const int PROT_READ = 0x1;
        public const int PROT_WRITE = 0x2;
        public const int MAP_SHARED = 0x1;
        public const int MAP_FIXED = 0x10;
    }
}
