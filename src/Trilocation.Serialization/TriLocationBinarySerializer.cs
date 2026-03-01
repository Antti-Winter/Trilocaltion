using Trilocation.Core;

namespace Trilocation.Serialization
{
    /// <summary>
    /// Compact binary serializer for TriLocation.
    /// Uses 8 bytes (ulong) in little-endian format.
    /// </summary>
    public static class TriLocationBinarySerializer
    {
        /// <summary>Serializes a TriLocation to an 8-byte array (little-endian).</summary>
        public static byte[] Serialize(TriLocation location)
        {
            return BitConverter.GetBytes(location.Index);
        }

        /// <summary>Deserializes a TriLocation from an 8-byte array.</summary>
        public static TriLocation Deserialize(byte[] data)
        {
            if (data.Length != 8)
            {
                throw new ArgumentException("Data must be exactly 8 bytes, got " + data.Length);
            }
            ulong index = BitConverter.ToUInt64(data, 0);
            return new TriLocation(index);
        }

        /// <summary>Writes a TriLocation to a stream (8 bytes, little-endian).</summary>
        public static void WriteTo(TriLocation location, Stream stream)
        {
            byte[] data = BitConverter.GetBytes(location.Index);
            stream.Write(data, 0, 8);
        }

        /// <summary>Reads a TriLocation from a stream (8 bytes, little-endian).</summary>
        public static TriLocation ReadFrom(Stream stream)
        {
            byte[] data = new byte[8];
            int bytesRead = stream.Read(data, 0, 8);
            if (bytesRead != 8)
            {
                throw new EndOfStreamException("Expected 8 bytes, got " + bytesRead);
            }
            ulong index = BitConverter.ToUInt64(data, 0);
            return new TriLocation(index);
        }
    }
}
