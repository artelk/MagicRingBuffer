using System.Runtime.InteropServices;

namespace MagicRingBuffer
{
    internal static class PlatformInfo
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
