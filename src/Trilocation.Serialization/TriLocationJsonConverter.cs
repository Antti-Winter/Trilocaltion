using System.Text.Json;
using System.Text.Json.Serialization;
using Trilocation.Core;

namespace Trilocation.Serialization
{
    /// <summary>
    /// System.Text.Json converter for TriLocation.
    /// Serializes as a JSON number (ulong index) for compact representation.
    /// </summary>
    public class TriLocationJsonConverter : JsonConverter<TriLocation>
    {
        /// <summary>Reads a TriLocation from JSON (expects a number).</summary>
        public override TriLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ulong index = reader.GetUInt64();
            return new TriLocation(index);
        }

        /// <summary>Writes a TriLocation to JSON as a number.</summary>
        public override void Write(Utf8JsonWriter writer, TriLocation value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Index);
        }
    }
}
