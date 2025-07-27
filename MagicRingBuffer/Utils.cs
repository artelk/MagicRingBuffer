namespace MagicRingBuffer
{
    internal static class Utils
    {
        public static ulong Gcd(ulong a, ulong b) => b == 0 ? a : Gcd(b, a % b);

        public static ulong Lcm(uint a, uint b) => (ulong)a * b / Gcd(a, b);
    }
}
