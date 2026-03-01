using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trilocation.Core;
using Trilocation.Core.Indexing;

namespace TestVectorGenerator;

public static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void Main(string[] args)
    {
        string specDir = args.Length > 0
            ? args[0]
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "spec"));

        Directory.CreateDirectory(specDir);

        Console.WriteLine("Trilocation Test Vector Generator");
        Console.WriteLine("=================================");
        Console.WriteLine("Output directory: " + specDir);
        Console.WriteLine();

        GenerateResolutionTable(specDir);
        GenerateTestVectors(specDir);

        Console.WriteLine();
        Console.WriteLine("Done!");
    }

    private static void GenerateResolutionTable(string specDir)
    {
        Console.WriteLine("Generating resolution-table.json...");

        var levels = new List<object>();
        for (int level = 0; level <= IndexConstants.MaxResolution; level++)
        {
            ulong start = CumulativeIndex.LevelStart(level);
            ulong end = CumulativeIndex.LevelEnd(level);
            ulong triangleCount = end - start + 1;
            ulong cumulativeCount = CumulativeIndex.CumulativeCount(level);

            // Calculate area for one cell at this level
            double areaKm2 = 0;
            if (level <= 24)
            {
                var sample = new TriLocation(start);
                var cell = sample.ToCell();
                areaKm2 = cell.AreaKm2;
            }

            levels.Add(new
            {
                level,
                triangle_count = triangleCount.ToString(CultureInfo.InvariantCulture),
                cumulative_count = cumulativeCount.ToString(CultureInfo.InvariantCulture),
                level_start = start.ToString(CultureInfo.InvariantCulture),
                level_end = end.ToString(CultureInfo.InvariantCulture),
                area_km2 = level <= 24 ? Math.Round(areaKm2, 6).ToString(CultureInfo.InvariantCulture) : null
            });
        }

        var table = new { version = "1.0", max_resolution = IndexConstants.MaxResolution, levels };
        string json = JsonSerializer.Serialize(table, JsonOptions);
        File.WriteAllText(Path.Combine(specDir, "resolution-table.json"), json);

        Console.WriteLine("  " + levels.Count + " levels generated");
    }

    private static void GenerateTestVectors(string specDir)
    {
        Console.WriteLine("Generating test-vectors.json...");

        var levelBoundaries = GenerateLevelBoundaries();
        var parentChild = GenerateParentChild();
        var baseFaces = GenerateBaseFaces();
        var coordinateConversions = GenerateCoordinateConversions();
        var neighbors = GenerateNeighbors();
        var containment = GenerateContainment();

        int total = levelBoundaries.Count + parentChild.Count + baseFaces.Count
            + coordinateConversions.Count + neighbors.Count + containment.Count;

        var vectors = new
        {
            version = "1.0",
            generator = "Trilocation C# Reference Implementation v0.1.0",
            statistics = new
            {
                level_boundaries = levelBoundaries.Count,
                parent_child = parentChild.Count,
                base_faces = baseFaces.Count,
                coordinate_conversions = coordinateConversions.Count,
                neighbors = neighbors.Count,
                containment = containment.Count,
                total
            },
            level_boundaries = levelBoundaries,
            parent_child = parentChild,
            base_faces = baseFaces,
            coordinate_conversions = coordinateConversions,
            neighbors = neighbors,
            containment = containment
        };

        string json = JsonSerializer.Serialize(vectors, JsonOptions);
        File.WriteAllText(Path.Combine(specDir, "test-vectors.json"), json);

        Console.WriteLine("  Level boundaries: " + levelBoundaries.Count);
        Console.WriteLine("  Parent-child: " + parentChild.Count);
        Console.WriteLine("  Base faces: " + baseFaces.Count);
        Console.WriteLine("  Coordinate conversions: " + coordinateConversions.Count);
        Console.WriteLine("  Neighbors: " + neighbors.Count);
        Console.WriteLine("  Containment: " + containment.Count);
        Console.WriteLine("  TOTAL: " + total);
    }

    private static List<object> GenerateLevelBoundaries()
    {
        var results = new List<object>();
        for (int level = 0; level <= IndexConstants.MaxResolution; level++)
        {
            results.Add(new
            {
                level,
                start = CumulativeIndex.LevelStart(level).ToString(CultureInfo.InvariantCulture),
                end = CumulativeIndex.LevelEnd(level).ToString(CultureInfo.InvariantCulture),
                count = (CumulativeIndex.LevelEnd(level) - CumulativeIndex.LevelStart(level) + 1)
                    .ToString(CultureInfo.InvariantCulture),
                cumulative = CumulativeIndex.CumulativeCount(level).ToString(CultureInfo.InvariantCulture)
            });
        }
        return results;
    }

    private static List<object> GenerateParentChild()
    {
        var results = new List<object>();

        // Test every base face (resolution 0)
        for (ulong face = 1; face <= 8; face++)
        {
            ulong[] children = CumulativeIndex.GetChildren(face);
            results.Add(new
            {
                index = face.ToString(CultureInfo.InvariantCulture),
                resolution = 0,
                base_face = CumulativeIndex.GetBaseFace(face),
                children = children.Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray()
            });
        }

        // Resolution 1: children of face 1
        for (int childIdx = 0; childIdx < 4; childIdx++)
        {
            ulong child = CumulativeIndex.GetChildren(1)[childIdx];
            ulong parent = CumulativeIndex.GetParent(child);
            ulong[] grandchildren = CumulativeIndex.GetChildren(child);
            results.Add(new
            {
                index = child.ToString(CultureInfo.InvariantCulture),
                resolution = 1,
                base_face = CumulativeIndex.GetBaseFace(child),
                parent = parent.ToString(CultureInfo.InvariantCulture),
                child_position = childIdx,
                children = grandchildren.Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray()
            });
        }

        // Random samples across different resolutions and faces
        int[] sampleResolutions = { 2, 3, 5, 8, 10, 15, 20, 24, 30 };
        foreach (int res in sampleResolutions)
        {
            // Pick a few indices at each resolution: first, middle, last of each face
            ulong levelStart = CumulativeIndex.LevelStart(res);
            ulong levelEnd = CumulativeIndex.LevelEnd(res);
            ulong levelSize = levelEnd - levelStart + 1;
            ulong perFace = levelSize / 8;

            // First cell of face 0 and face 4
            ulong[] sampleIndices = {
                levelStart,
                levelStart + perFace * 4,
                levelStart + perFace / 2,
                levelEnd
            };

            foreach (ulong idx in sampleIndices)
            {
                if (idx < levelStart || idx > levelEnd) continue;
                ulong parent = CumulativeIndex.GetParent(idx);
                int baseFace = CumulativeIndex.GetBaseFace(idx);

                var entry = new Dictionary<string, object>
                {
                    ["index"] = idx.ToString(CultureInfo.InvariantCulture),
                    ["resolution"] = res,
                    ["base_face"] = baseFace,
                    ["parent"] = parent.ToString(CultureInfo.InvariantCulture)
                };

                // Only include children for resolutions <= 29 (children would be at 30)
                if (res < IndexConstants.MaxResolution)
                {
                    ulong[] children = CumulativeIndex.GetChildren(idx);
                    entry["children"] = children.Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray();
                }

                results.Add(entry);
            }
        }

        return results;
    }

    private static List<object> GenerateBaseFaces()
    {
        var results = new List<object>();
        for (int face = 0; face < 8; face++)
        {
            ulong index = (ulong)(face + 1);
            var location = new TriLocation(index);
            var cell = location.ToCell();
            var (lat, lon) = location.ToLatLon();

            results.Add(new
            {
                face,
                index = index.ToString(CultureInfo.InvariantCulture),
                centroid_lat = Math.Round(lat, 10).ToString(CultureInfo.InvariantCulture),
                centroid_lon = Math.Round(lon, 10).ToString(CultureInfo.InvariantCulture),
                vertex_a_lat = Math.Round(cell.VertexA.Latitude, 10).ToString(CultureInfo.InvariantCulture),
                vertex_a_lon = Math.Round(cell.VertexA.Longitude, 10).ToString(CultureInfo.InvariantCulture),
                vertex_b_lat = Math.Round(cell.VertexB.Latitude, 10).ToString(CultureInfo.InvariantCulture),
                vertex_b_lon = Math.Round(cell.VertexB.Longitude, 10).ToString(CultureInfo.InvariantCulture),
                vertex_c_lat = Math.Round(cell.VertexC.Latitude, 10).ToString(CultureInfo.InvariantCulture),
                vertex_c_lon = Math.Round(cell.VertexC.Longitude, 10).ToString(CultureInfo.InvariantCulture),
                area_km2 = Math.Round(cell.AreaKm2, 2).ToString(CultureInfo.InvariantCulture)
            });
        }
        return results;
    }

    private static List<object> GenerateCoordinateConversions()
    {
        var results = new List<object>();

        // Major world cities
        var cities = new (string Name, double Lat, double Lon)[]
        {
            ("Helsinki", 60.1699, 24.9384),
            ("London", 51.5074, -0.1278),
            ("Tokyo", 35.6762, 139.6503),
            ("New York", 40.7128, -74.0060),
            ("Sydney", -33.8688, 151.2093),
            ("Cape Town", -33.9249, 18.4241),
            ("Sao Paulo", -23.5505, -46.6333),
            ("Moscow", 55.7558, 37.6173),
            ("Dubai", 25.2048, 55.2708),
            ("Singapore", 1.3521, 103.8198),
            ("Cairo", 30.0444, 31.2357),
            ("Mumbai", 19.0760, 72.8777),
            ("Mexico City", 19.4326, -99.1332),
            ("Beijing", 39.9042, 116.4074),
            ("Buenos Aires", -34.6037, -58.3816),
            ("Lagos", 6.5244, 3.3792),
            ("Nairobi", -1.2921, 36.8219),
            ("Bangkok", 13.7563, 100.5018),
            ("Berlin", 52.5200, 13.4050),
            ("Rome", 41.9028, 12.4964)
        };

        int[] resolutions = { 5, 10, 15, 20, 24 };
        foreach (var (name, lat, lon) in cities)
        {
            foreach (int res in resolutions)
            {
                var location = new TriLocation(lat, lon, res);
                var (backLat, backLon) = location.ToLatLon();
                results.Add(new
                {
                    name,
                    input_lat = lat.ToString(CultureInfo.InvariantCulture),
                    input_lon = lon.ToString(CultureInfo.InvariantCulture),
                    resolution = res,
                    expected_index = location.Index.ToString(CultureInfo.InvariantCulture),
                    centroid_lat = Math.Round(backLat, 10).ToString(CultureInfo.InvariantCulture),
                    centroid_lon = Math.Round(backLon, 10).ToString(CultureInfo.InvariantCulture),
                    base_face = location.BaseFace
                });
            }
        }

        // Special geographic points
        var specialPoints = new (string Name, double Lat, double Lon)[]
        {
            ("North Pole", 89.9999, 0.0),
            ("South Pole", -89.9999, 0.0),
            ("Equator-PrimeMeridian", 0.0, 0.0),
            ("Equator-90E", 0.0, 90.0),
            ("Equator-180", 0.0, 180.0),
            ("Equator-90W", 0.0, -90.0),
            ("Antimeridian-North", 45.0, 179.9999),
            ("Antimeridian-South", -45.0, -179.9999),
            ("Arctic Circle", 66.5, 25.0),
            ("Antarctic Circle", -66.5, 170.0)
        };

        foreach (var (name, lat, lon) in specialPoints)
        {
            foreach (int res in resolutions)
            {
                var location = new TriLocation(lat, lon, res);
                var (backLat, backLon) = location.ToLatLon();
                results.Add(new
                {
                    name,
                    input_lat = lat.ToString(CultureInfo.InvariantCulture),
                    input_lon = lon.ToString(CultureInfo.InvariantCulture),
                    resolution = res,
                    expected_index = location.Index.ToString(CultureInfo.InvariantCulture),
                    centroid_lat = Math.Round(backLat, 10).ToString(CultureInfo.InvariantCulture),
                    centroid_lon = Math.Round(backLon, 10).ToString(CultureInfo.InvariantCulture),
                    base_face = location.BaseFace
                });
            }
        }

        // Resolution 30 samples (highest resolution)
        var res30Points = new (string Name, double Lat, double Lon)[]
        {
            ("Helsinki-Res30", 60.1699, 24.9384),
            ("Equator-Res30", 0.0, 0.0),
            ("Sydney-Res30", -33.8688, 151.2093)
        };
        foreach (var (name, lat, lon) in res30Points)
        {
            var location = new TriLocation(lat, lon, 30);
            var (backLat, backLon) = location.ToLatLon();
            results.Add(new
            {
                name,
                input_lat = lat.ToString(CultureInfo.InvariantCulture),
                input_lon = lon.ToString(CultureInfo.InvariantCulture),
                resolution = 30,
                expected_index = location.Index.ToString(CultureInfo.InvariantCulture),
                centroid_lat = Math.Round(backLat, 10).ToString(CultureInfo.InvariantCulture),
                centroid_lon = Math.Round(backLon, 10).ToString(CultureInfo.InvariantCulture),
                base_face = location.BaseFace
            });
        }

        return results;
    }

    private static List<object> GenerateNeighbors()
    {
        var results = new List<object>();

        // Same-face neighbors at different resolutions
        int[] resolutions = { 1, 2, 5, 10, 15 };
        foreach (int res in resolutions)
        {
            // Pick a cell in the middle of face 0
            var center = new TriLocation(60.0, 45.0, res);
            var neighbors = center.GetNeighbors();
            results.Add(new
            {
                description = "Same-face neighbors at resolution " + res,
                center_index = center.Index.ToString(CultureInfo.InvariantCulture),
                resolution = res,
                base_face = center.BaseFace,
                neighbor_count = neighbors.Length,
                neighbor_indices = neighbors.Select(n => n.Index.ToString(CultureInfo.InvariantCulture)).ToArray()
            });
        }

        // Face-boundary neighbors
        var boundaryPoints = new (string Desc, double Lat, double Lon, int Res)[]
        {
            ("Equator face boundary (0-90E)", 0.5, 45.0, 10),
            ("Equator face boundary (90-180E)", 0.5, 135.0, 10),
            ("Equator face boundary (0-90W)", 0.5, -45.0, 10),
            ("Equator face boundary (90-180W)", 0.5, -135.0, 10),
            ("Near north pole", 89.0, 0.0, 5),
            ("Near south pole", -89.0, 0.0, 5),
            ("Near antimeridian", 45.0, 179.5, 10),
            ("Near prime meridian north", 45.0, 0.5, 10),
            ("Near prime meridian south", -45.0, 0.5, 10)
        };

        foreach (var (desc, lat, lon, res) in boundaryPoints)
        {
            var center = new TriLocation(lat, lon, res);
            var neighbors = center.GetNeighbors();
            bool crossesFace = neighbors.Any(n => n.BaseFace != center.BaseFace);
            results.Add(new
            {
                description = desc,
                center_index = center.Index.ToString(CultureInfo.InvariantCulture),
                resolution = res,
                base_face = center.BaseFace,
                crosses_face_boundary = crossesFace,
                neighbor_count = neighbors.Length,
                neighbor_indices = neighbors.Select(n => n.Index.ToString(CultureInfo.InvariantCulture)).ToArray(),
                neighbor_faces = neighbors.Select(n => n.BaseFace).ToArray()
            });
        }

        // GetNeighborsWithin tests
        var withinTests = new (double Lat, double Lon, int Res, int Distance)[]
        {
            (60.0, 25.0, 10, 1),
            (60.0, 25.0, 10, 2),
            (0.0, 0.0, 5, 1),
            (0.0, 0.0, 5, 2)
        };

        foreach (var (lat, lon, res, dist) in withinTests)
        {
            var center = new TriLocation(lat, lon, res);
            var within = center.GetNeighborsWithin(dist);
            results.Add(new
            {
                description = "GetNeighborsWithin distance=" + dist + " at res " + res,
                center_index = center.Index.ToString(CultureInfo.InvariantCulture),
                resolution = res,
                distance = dist,
                result_count = within.Length,
                result_indices = within.Select(n => n.Index.ToString(CultureInfo.InvariantCulture)).ToArray()
            });
        }

        return results;
    }

    private static List<object> GenerateContainment()
    {
        var results = new List<object>();

        // Parent contains child tests
        int[] resolutions = { 0, 1, 5, 10, 15, 20 };
        foreach (int parentRes in resolutions)
        {
            if (parentRes >= IndexConstants.MaxResolution) continue;
            int childRes = parentRes + 1;
            var parent = new TriLocation(60.0, 25.0, parentRes);
            var child = new TriLocation(60.0, 25.0, childRes);
            bool contains = parent.Contains(child);

            results.Add(new
            {
                description = "Parent (res " + parentRes + ") contains child (res " + childRes + ")",
                parent_index = parent.Index.ToString(CultureInfo.InvariantCulture),
                parent_resolution = parentRes,
                child_index = child.Index.ToString(CultureInfo.InvariantCulture),
                child_resolution = childRes,
                expected_contains = contains
            });
        }

        // Non-containment: different locations
        var nonContainTests = new (double ParentLat, double ParentLon, int ParentRes, double ChildLat, double ChildLon, int ChildRes)[]
        {
            (60.0, 25.0, 5, -33.0, 151.0, 6),
            (60.0, 25.0, 5, 60.0, 25.0, 3),
            (40.0, -74.0, 10, 35.0, 139.0, 11),
            (0.0, 0.0, 5, 0.0, 90.0, 6)
        };

        foreach (var (pLat, pLon, pRes, cLat, cLon, cRes) in nonContainTests)
        {
            var parent = new TriLocation(pLat, pLon, pRes);
            var child = new TriLocation(cLat, cLon, cRes);
            bool contains = parent.Contains(child);

            results.Add(new
            {
                description = "Non-containment: different locations or lower resolution",
                parent_index = parent.Index.ToString(CultureInfo.InvariantCulture),
                parent_resolution = pRes,
                child_index = child.Index.ToString(CultureInfo.InvariantCulture),
                child_resolution = cRes,
                expected_contains = contains
            });
        }

        return results;
    }
}
