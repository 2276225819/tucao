 
namespace tucao
{
    internal static class OldSkool
    {
        internal static byte hibyte(ushort x)
        {
            return (byte)(0xff & (x >> 8));
        }

        internal static ushort hiword(ulong x)
        {
            return (ushort)(((ulong)0xffffL) & (x >> 0x10));
        }

        internal static byte lobyte(ushort x)
        {
            return (byte)(0xff & x);
        }

        internal static ushort loword(ulong x)
        {
            return (ushort)(((ulong)0xffffL) & x);
        }

        internal static ulong makelong(ushort lo, ushort hi)
        {
            return (ulong)((hi << 0x10) | lo);
        }

        internal static ushort makeword(byte lo, byte hi)
        {
            return (ushort)((hi << 8) | lo);
        }

        internal static ulong swaplong(ulong x)
        {
            return makelong(swapword(hiword(x)), swapword(loword(x)));
        }

        internal static ushort swapword(ushort x)
        {
            return makeword(hibyte(x), lobyte(x));
        }
    }
}
