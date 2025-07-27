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
            GetSystemInfo(out var info);
            AllocationGranularity = info.AllocationGranularity;
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
        static extern byte* VirtualAlloc2(
            IntPtr handle,
            byte* baseAddress,
            IntPtr size,
            uint allocationType,
            uint pageProtection,
            void* extendedParameters,
            uint parameterCount);

        [DllImport("kernel32", SetLastError = true)]
        static extern int VirtualFree(byte* addr, IntPtr len, uint freeType);

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr CreateFileMappingA(
            IntPtr hFile,
            void* lpFileMappingAttributes,
            uint flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            byte* lpName);

        [DllImport("Kernelbase", SetLastError = true)]
        static extern byte* MapViewOfFile3(
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
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32", SetLastError = true)]
        static extern int UnmapViewOfFile(byte* baseAddress);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SystemInfo systemInfo);

        [StructLayout(LayoutKind.Sequential)]
        struct SystemInfo
        {
            public ProcessorArchitecture ProcessorArchitecture; // WORD
            public uint PageSize; // DWORD
            public IntPtr MinimumApplicationAddress; // (long)void*
            public IntPtr MaximumApplicationAddress; // (long)void*
            public IntPtr ActiveProcessorMask; // DWORD*
            public uint NumberOfProcessors; // DWORD (WTF)
            public uint ProcessorType; // DWORD
            public uint AllocationGranularity; // DWORD
            public ushort ProcessorLevel; // WORD
            public ushort ProcessorRevision; // WORD
        }
    }
}
