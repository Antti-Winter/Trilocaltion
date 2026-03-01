using System.Globalization;
using System.Text.Json.Nodes;
using Trilocation.Core;
using Trilocation.Core.Extensions;
using Trilocation.Core.Indexing;
using Trilocation.Core.Primitives;

const int MaxViewportResolution = 15;
const int MaxExploreResolution = 20;
const int MaxViewportCells = 10000;
const int MaxNeighborDistance = 5;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

// GET /api/cell?lat=60.17&lon=24.94&resolution=10
// GET /api/cell?index=12345
app.MapGet("/api/cell", (double? lat, double? lon, int? resolution, string? index) =>
{
    if (index != null)
    {
        if (!ulong.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out ulong idx))
        {
            return Results.BadRequest(new { error = "Invalid index format" });
        }

        if (!IndexValidator.IsValid(idx))
        {
            return Results.BadRequest(new { error = "Index out of valid range" });
        }

        var location = new TriLocation(idx);
        return Results.Json(BuildFeature(location));
    }

    if (lat == null || lon == null)
    {
        return Results.BadRequest(new { error = "Provide lat+lon+resolution or index" });
    }

    int res = resolution ?? 10;
    if (res < 0 || res > MaxExploreResolution)
    {
        return Results.BadRequest(new { error = "Resolution must be 0-" + MaxExploreResolution });
    }

    var loc = new TriLocation(lat.Value, lon.Value, res);
    return Results.Json(BuildFeature(loc));
});

// GET /api/viewport?minLat=60&maxLat=61&minLon=24&maxLon=25&resolution=10
app.MapGet("/api/viewport", (double minLat, double maxLat, double minLon, double maxLon, int resolution) =>
{
    if (resolution < 0 || resolution > MaxViewportResolution)
    {
        return Results.BadRequest(new { error = "Viewport resolution must be 0-" + MaxViewportResolution });
    }

    var bounds = new GeoBounds(minLat, maxLat, minLon, maxLon);
    TriLocation[] locations = TriIndex.FromBounds(bounds, resolution);

    if (locations.Length > MaxViewportCells)
    {
        return Results.UnprocessableEntity(new
        {
            error = "Too many cells (" + locations.Length + "). Zoom in or reduce resolution.",
            count = locations.Length
        });
    }

    return Results.Json(BuildFeatureCollection(locations));
});

// GET /api/parent?index=12345
app.MapGet("/api/parent", (string index) =>
{
    if (!ulong.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out ulong idx))
    {
        return Results.BadRequest(new { error = "Invalid index format" });
    }

    var location = new TriLocation(idx);
    if (location.Resolution == 0)
    {
        return Results.BadRequest(new { error = "Base faces have no parent" });
    }

    return Results.Json(BuildFeature(location.GetParent()));
});

// GET /api/children?index=12345
app.MapGet("/api/children", (string index) =>
{
    if (!ulong.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out ulong idx))
    {
        return Results.BadRequest(new { error = "Invalid index format" });
    }

    var location = new TriLocation(idx);
    if (location.Resolution >= MaxExploreResolution)
    {
        return Results.BadRequest(new { error = "Max explore resolution is " + MaxExploreResolution });
    }

    return Results.Json(BuildFeatureCollection(location.GetChildren()));
});

// GET /api/neighbors?index=12345
app.MapGet("/api/neighbors", (string index) =>
{
    if (!ulong.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out ulong idx))
    {
        return Results.BadRequest(new { error = "Invalid index format" });
    }

    var location = new TriLocation(idx);
    return Results.Json(BuildFeatureCollection(location.GetNeighbors()));
});

// GET /api/neighbors-within?index=12345&distance=2
app.MapGet("/api/neighbors-within", (string index, int distance) =>
{
    if (!ulong.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out ulong idx))
    {
        return Results.BadRequest(new { error = "Invalid index format" });
    }

    if (distance < 1 || distance > MaxNeighborDistance)
    {
        return Results.BadRequest(new { error = "Distance must be 1-" + MaxNeighborDistance });
    }

    var location = new TriLocation(idx);
    return Results.Json(BuildFeatureCollection(location.GetNeighborsWithin(distance)));
});

// GET /api/resolution-table
app.MapGet("/api/resolution-table", () =>
{
    var table = new JsonArray();
    for (int i = 0; i <= MaxExploreResolution; i++)
    {
        ulong count = 8UL * (1UL << (2 * (i + 1))) / 4;
        // Triangle count at level i = 8 * 4^i
        ulong triangleCount = 8UL;
        for (int j = 0; j < i; j++)
        {
            triangleCount *= 4;
        }

        var entry = new JsonObject
        {
            ["level"] = i,
            ["triangleCount"] = triangleCount.ToString(CultureInfo.InvariantCulture),
            ["levelStart"] = CumulativeIndex.LevelStart(i).ToString(CultureInfo.InvariantCulture),
            ["levelEnd"] = CumulativeIndex.LevelEnd(i).ToString(CultureInfo.InvariantCulture)
        };
        table.Add(entry);
    }

    return Results.Json(table);
});

app.MapFallbackToFile("index.html");
app.Run();

// === Helper methods ===

static JsonObject BuildFeature(TriLocation location)
{
    TriCell cell = location.ToCell();

    var properties = new JsonObject
    {
        ["index"] = location.Index.ToString(CultureInfo.InvariantCulture),
        ["resolution"] = location.Resolution,
        ["baseFace"] = location.BaseFace,
        ["areaKm2"] = Math.Round(cell.AreaKm2, 6),
        ["centroidLat"] = Math.Round(cell.Centroid.Latitude, 8),
        ["centroidLon"] = Math.Round(cell.Centroid.Longitude, 8)
    };

    if (location.Resolution > 0)
    {
        properties["parentIndex"] = location.GetParent().Index.ToString(CultureInfo.InvariantCulture);
    }

    var ring = new JsonArray
    {
        CreateCoord(cell.VertexA.Longitude, cell.VertexA.Latitude),
        CreateCoord(cell.VertexB.Longitude, cell.VertexB.Latitude),
        CreateCoord(cell.VertexC.Longitude, cell.VertexC.Latitude),
        CreateCoord(cell.VertexA.Longitude, cell.VertexA.Latitude)
    };

    var coordinates = new JsonArray { ring };

    var geometry = new JsonObject
    {
        ["type"] = "Polygon",
        ["coordinates"] = coordinates
    };

    return new JsonObject
    {
        ["type"] = "Feature",
        ["properties"] = properties,
        ["geometry"] = geometry
    };
}

static JsonArray CreateCoord(double lon, double lat)
{
    return new JsonArray
    {
        Math.Round(lon, 10),
        Math.Round(lat, 10)
    };
}

static JsonObject BuildFeatureCollection(TriLocation[] locations)
{
    var features = new JsonArray();
    for (int i = 0; i < locations.Length; i++)
    {
        features.Add(BuildFeature(locations[i]));
    }

    return new JsonObject
    {
        ["type"] = "FeatureCollection",
        ["features"] = features
    };
}
