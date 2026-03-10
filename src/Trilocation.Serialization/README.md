# Trilocation.Serialization

JSON and binary serialization for the Trilocation geospatial indexing system.

## Features

- **TriLocationJsonConverter** - System.Text.Json converter for TriLocation
- **TriLocationBinarySerializer** - Compact 8-byte little-endian binary format

## Quick Start

```csharp
using Trilocation.Serialization;

// JSON serialization
var options = new JsonSerializerOptions();
options.Converters.Add(new TriLocationJsonConverter());
string json = JsonSerializer.Serialize(location, options);

// Binary serialization (8 bytes)
byte[] bytes = TriLocationBinarySerializer.Serialize(location);
TriLocation restored = TriLocationBinarySerializer.Deserialize(bytes);

// Stream support
TriLocationBinarySerializer.WriteTo(location, stream);
TriLocation fromStream = TriLocationBinarySerializer.ReadFrom(stream);
```

## Dependencies

- Trilocation.Core

## License

AGPL-3.0-or-later
