using Xunit;
using Trilocation.Core;
using Trilocation.Serialization;

namespace Trilocation.Serialization.Tests
{
    public class TriLocationBinarySerializerTests
    {
        [Fact]
        public void Serialize_Returns8Bytes()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            byte[] data = TriLocationBinarySerializer.Serialize(location);
            Assert.Equal(8, data.Length);
        }

        [Fact]
        public void Serialize_LittleEndian()
        {
            var location = new TriLocation(60.17, 24.94, 5);
            byte[] data = TriLocationBinarySerializer.Serialize(location);
            ulong fromBytes = BitConverter.ToUInt64(data, 0);
            Assert.Equal(location.Index, fromBytes);
        }

        [Fact]
        public void RoundTrip_PreservesIndex()
        {
            var original = new TriLocation(60.17, 24.94, 10);
            byte[] data = TriLocationBinarySerializer.Serialize(original);
            var deserialized = TriLocationBinarySerializer.Deserialize(data);
            Assert.Equal(original.Index, deserialized.Index);
            Assert.Equal(original.Resolution, deserialized.Resolution);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(24)]
        [InlineData(30)]
        public void RoundTrip_DifferentResolutions(int resolution)
        {
            var original = new TriLocation(60.17, 24.94, resolution);
            byte[] data = TriLocationBinarySerializer.Serialize(original);
            var deserialized = TriLocationBinarySerializer.Deserialize(data);
            Assert.Equal(original.Index, deserialized.Index);
        }

        [Fact]
        public void WriteTo_ReadFrom_Stream_RoundTrip()
        {
            var original = new TriLocation(60.17, 24.94, 10);
            using var stream = new MemoryStream();
            TriLocationBinarySerializer.WriteTo(original, stream);
            Assert.Equal(8, stream.Length);

            stream.Position = 0;
            var deserialized = TriLocationBinarySerializer.ReadFrom(stream);
            Assert.Equal(original.Index, deserialized.Index);
        }

        [Fact]
        public void Stream_MultipleLocations_RoundTrip()
        {
            var locations = new TriLocation[]
            {
                new TriLocation(60.17, 24.94, 5),
                new TriLocation(0.0, 0.0, 5),
                new TriLocation(-33.87, 151.21, 5)
            };

            using var stream = new MemoryStream();
            foreach (var loc in locations)
            {
                TriLocationBinarySerializer.WriteTo(loc, stream);
            }
            Assert.Equal(24, stream.Length); // 3 * 8 bytes

            stream.Position = 0;
            for (int i = 0; i < locations.Length; i++)
            {
                var deserialized = TriLocationBinarySerializer.ReadFrom(stream);
                Assert.Equal(locations[i].Index, deserialized.Index);
            }
        }

        [Fact]
        public void Deserialize_InvalidLength_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                TriLocationBinarySerializer.Deserialize(new byte[4]));
        }
    }
}
