using ImageCompression.Structures;

namespace ImageCompression.Algorithms
{
    public static class Quantization
    {
        public static Vector<byte> QuantizeVector(Vector<byte> color, Vector<byte> quantizationVector)
        {
            var bytes = new byte[3];
            for (var i = 0; i < 3; i++)
            {
                bytes[i] = QuantizeByte(color[i], quantizationVector[i]);
            }
            return new Vector<byte>(bytes);
        }

        public static byte QuantizeByte(byte value, byte bits)
        {
            return (byte)(((value >> (8 - bits)) << (8 - bits)) + (bits == 8 ? 0 : 1 << (8 - bits - 1)));
        }
    }
}
