using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MagicRingBuffer
{
    internal unsafe static class Windows
    {
        public readonly static uint AllocationGranularity;

        static Windows()
        {
            if (!PlatformInfo.IsWindows) return;

            // that is probably undocumented but Windows may support one page granularity
            if (Test((uint)Environment.SystemPageSize))
            {
                AllocationGranularity = (uint)Environment.SystemPageSize;
            }
            else
            {
                GetSystemInfo(out var info);
                AllocationGranularity = info.AllocationGranularity;
            }
        }

        private static bool Test(uint len)
        {
            try
            {
                var ptr = Alloc(len);
                try
                {
                    if (*ptr != *(ptr + len)) return false;
                    (*ptr)++;
                    if (*ptr != *(ptr + len)) return false;
                }
                finally
                {
                    Free(ptr, len);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static byte* Alloc(uint len)
        {
            var placeholder1 = VirtualAlloc2(
                                    IntPtr.Zero,
                                    (byte*)0,
                                    (IntPtr)(2 * (ulong)len),
                                    MEM_RESERVE | MEM_RESERVE_PLACEHOLDER,
                                    PAGE_NOACCESS,
                                    (void*)0,
                                    0);

            if (placeholder1 == (byte*)0)
                Throw.Win32ExceptionWithCurrentError();

            if (VirtualFree(placeholder1, (IntPtr)len, MEM_RELEASE | MEM_PRESERVE_PLACEHOLDER) == 0)
                Throw.Win32ExceptionWithCurrentError();

            var handle = CreateFileMappingA(
                            (IntPtr)(-1),
                            (void*)0,
                            PAGE_READWRITE,
                            0,
                            len,
                            (byte*)0);

            if (handle == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                VirtualFree(placeholder1, (IntPtr)0, MEM_RELEASE);
                Throw.Win32Exception(error);
            }

            try
            {
                var view1 = MapViewOfFile3(
                            handle,
                            IntPtr.Zero,
                            placeholder1,
                            0,
                            (IntPtr)len,
                            MEM_REPLACE_PLACEHOLDER,
                            PAGE_READWRITE,
                            (void*)0,
                            0);

                if (view1 == (byte*)0)
                {
                    var error = Marshal.GetLastWin32Error();
                    VirtualFree(placeholder1, IntPtr.Zero, MEM_RELEASE);
                    Throw.Win32Exception(error);
                }

                var placeholder2 = placeholder1 + len;
                var view2 = MapViewOfFile3(
                    handle,
                    IntPtr.Zero,
                    placeholder2,
                    0,
                    (IntPtr)len,
                    MEM_REPLACE_PLACEHOLDER,
                    PAGE_READWRITE,
                    (void*)0,
                    0);

                if (view2 == (byte*)0)
                {
                    var error = Marshal.GetLastWin32Error();
                    UnmapViewOfFile(view1);
                    Throw.Win32Exception(error);
                }

                return view1;
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        public static void Free(byte* addr, uint len)
        {
            UnmapViewOfFile(addr + len);
            UnmapViewOfFile(addr);
        }

        private const uint MEM_RESERVE = 0x00002000;
        private const uint MEM_RESERVE_PLACEHOLDER = 0x00040000;
        private const uint MEM_PRESERVE_PLACEHOLDER = 0x00000002;
        private const uint MEM_REPLACE_PLACEHOLDER = 0x00004000;
        private const uint MEM_RELEASE = 0x00008000;

        private const uint PAGE_NOACCESS = 0x01;
        private const uint PAGE_READWRITE = 0x04;

        [DllImport("Kernelbase", SetLastError = true)]
        private static extern byte* VirtualAlloc2(
            IntPtr handle,
            byte* baseAddress,
            IntPtr size,
            uint allocationType,
            uint pageProtection,
            void* extendedParameters,
            uint parameterCount);

        [DllImport("kernel32", SetLastError = true)]
        private static extern int VirtualFree(byte* addr, IntPtr len, uint freeType);

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr CreateFileMappingA(
            IntPtr hFile,
            void* lpFileMappingAttributes,
            uint flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            byte* lpName);

        [DllImport("Kernelbase", SetLastError = true)]
        private static extern byte* MapViewOfFile3(
            IntPtr fileMapping,
            IntPtr process,
            byte* baseAddress,
            ulong offset,
            IntPtr viewSize,
            uint allocationType,
            uint pageProtection,
            void* extendedParameters,
            uint parameterCount);

        [DllImport("kernel32", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32", SetLastError = true)]
        private static extern int UnmapViewOfFile(byte* baseAddress);

        [DllImport("kernel32.dll")]
        private static extern void GetSystemInfo(out SystemInfo systemInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct SystemInfo
        {
            public ProcessorArchitecture ProcessorArchitecture;
            public uint PageSize;
            public IntPtr MinimumApplicationAddress;
            public IntPtr MaximumApplicationAddress;
            public IntPtr ActiveProcessorMask;
            public uint NumberOfProcessors;
            public uint ProcessorType;
            public uint AllocationGranularity;
            public ushort ProcessorLevel;
            public ushort ProcessorRevision;
        }
    }
}
