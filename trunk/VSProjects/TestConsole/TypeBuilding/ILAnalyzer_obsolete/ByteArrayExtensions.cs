using System;

namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    public static class ByteArrayExtensions
    {
        public static int GetInt32Safe(this byte[] bytes, int index)
        {
            if (bytes.Length < index + 4)
                return int.MinValue;
            return
                bytes[index + 0] |
                bytes[index + 1] << 8 |
                bytes[index + 2] << 16 |
                bytes[index + 3] << 24;
        }

        public static Int64 GetInt64(this byte[] bytes, int index)
        {
            Int64 i1 = bytes.GetInt32Safe(index);
            int i2 = bytes.GetInt32Safe(index);
            return i1 << 32 | i2;
        }
    }
}
