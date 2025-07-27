using System.Runtime.InteropServices;

namespace MagicRingBuffer
{
    internal static class PlatformInfo
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static readonly OSKind OS;

        static PlatformInfo()
        {
            if (IsWindows) OS = OSKind.Windows;
            else if (IsLinux) OS = OSKind.Linux;
            else OS = OSKind.Other;
        }
    }

    internal enum OSKind
    {
        Other,
        Windows,
        Linux
    }
}
