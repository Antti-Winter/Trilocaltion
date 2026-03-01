using System.Text.Json;
using Xunit;
using Trilocation.Core;
using Trilocation.Serialization;

namespace Trilocation.Serialization.Tests
{
    public class TriLocationJsonConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public TriLocationJsonConverterTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new TriLocationJsonConverter());
        }

        [Fact]
        public void Serialize_WritesIndexAsNumber()
        {
            var location = new TriLocation(60.17, 24.94, 5);
            string json = JsonSerializer.Serialize(location, _options);
            // Should be a plain number, not an object
            Assert.DoesNotContain("{", json);
            Assert.DoesNotContain("}", json);
            ulong parsed = ulong.Parse(json);
            Assert.Equal(location.Index, parsed);
        }

        [Fact]
        public void Deserialize_ReadsIndexFromNumber()
        {
            var original = new TriLocation(60.17, 24.94, 5);
            string json = original.Index.ToString();
            var deserialized = JsonSerializer.Deserialize<TriLocation>(json, _options);
            Assert.Equal(original.Index, deserialized.Index);
        }

        [Fact]
        public void RoundTrip_PreservesIndex()
        {
            var original = new TriLocation(60.17, 24.94, 10);
            string json = JsonSerializer.Serialize(original, _options);
            var deserialized = JsonSerializer.Deserialize<TriLocation>(json, _options);
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
            string json = JsonSerializer.Serialize(original, _options);
            var deserialized = JsonSerializer.Deserialize<TriLocation>(json, _options);
            Assert.Equal(original.Index, deserialized.Index);
        }

        [Fact]
        public void Serialize_Object_WithTriLocationProperty()
        {
            var wrapper = new TestWrapper
            {
                Name = "Helsinki",
                Location = new TriLocation(60.17, 24.94, 10)
            };
            string json = JsonSerializer.Serialize(wrapper, _options);
            Assert.Contains("\"Name\"", json);
            Assert.Contains("\"Location\"", json);

            var deserialized = JsonSerializer.Deserialize<TestWrapper>(json, _options);
            Assert.NotNull(deserialized);
            Assert.Equal(wrapper.Location.Index, deserialized.Location.Index);
        }

        [Fact]
        public void Serialize_Array_OfTriLocations()
        {
            var locations = new TriLocation[]
            {
                new TriLocation(60.17, 24.94, 5),
                new TriLocation(0.0, 0.0, 5),
                new TriLocation(-33.87, 151.21, 5)
            };
            string json = JsonSerializer.Serialize(locations, _options);
            Assert.StartsWith("[", json);
            Assert.EndsWith("]", json);

            var deserialized = JsonSerializer.Deserialize<TriLocation[]>(json, _options);
            Assert.NotNull(deserialized);
            Assert.Equal(3, deserialized.Length);
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(locations[i].Index, deserialized[i].Index);
            }
        }

        private class TestWrapper
        {
            public string Name { get; set; } = "";
            public TriLocation Location { get; set; }
        }
    }
}
