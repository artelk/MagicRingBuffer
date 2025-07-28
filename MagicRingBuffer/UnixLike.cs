using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;

namespace MagicRingBuffer
{
    internal unsafe static class UnixLike
    {
        public static byte* Alloc(uint len)
        {
            using var mmf = MemoryMappedFile.CreateNew(null, len);
            var fd = (int)mmf.SafeMemoryMappedFileHandle.DangerousGetHandle();

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
        private static extern byte* mmap(byte* addr, long length, int prot, int flags, int fd, long offset);

        [DllImport("libc", SetLastError = true)]
        private static extern byte* munmap(byte* addr, long length);

        private const int PROT_READ = 0x1;
        private const int PROT_WRITE = 0x2;
        private const int MAP_SHARED = 0x1;
        private const int MAP_FIXED = 0x10;
    }
}
