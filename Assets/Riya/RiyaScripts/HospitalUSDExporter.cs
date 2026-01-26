using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Hospital Layout USD Exporter
/// Exports Unity scene to USD format for Omniverse with hospital metadata
/// </summary>
public class HospitalUSDExporter : MonoBehaviour
{
    [Header("Export Settings")]
    [Tooltip("Where to save USD files")]
    public string exportFolder = "Exports";

    [Tooltip("Base filename")]
    public string fileName = "hospital_layout";

    [Header("Hospital Data")]
    public List<RoomData> rooms = new List<RoomData>();

    /// <summary>
    /// Export current hospital layout to USD
    /// </summary>
    [ContextMenu("Export Hospital to USD")]
    public void ExportHospitalLayout()
    {
        string fullPath = Path.Combine(Application.dataPath, "..", exportFolder, $"{fileName}.usda");
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        StringBuilder usd = new StringBuilder();

        // USD Header
        usd.AppendLine("#usda 1.0");
        usd.AppendLine("(");
        usd.AppendLine("    defaultPrim = \"Hospital\"");
        usd.AppendLine("    upAxis = \"Y\"");
        usd.AppendLine("    metersPerUnit = 1");
        usd.AppendLine(")");
        usd.AppendLine();

        // Root
        usd.AppendLine("def Xform \"Hospital\"");
        usd.AppendLine("{");

        // Geometry section
        usd.AppendLine("    def Xform \"Geometry\"");
        usd.AppendLine("    {");

        // Export floor
        ExportFloor(usd);

        // Export rooms
        foreach (var room in rooms)
        {
            ExportRoom(usd, room);
        }

        usd.AppendLine("    }"); // Close Geometry

        // Navigation section (placeholder for now)
        usd.AppendLine();
        usd.AppendLine("    def Xform \"NavigationData\"");
        usd.AppendLine("    {");
        usd.AppendLine("        # Spawn points, goals, waypoints will go here");
        usd.AppendLine("    }");

        // Traffic data section (for simulation results)
        usd.AppendLine();
        usd.AppendLine("    def Xform \"TrafficData\"");
        usd.AppendLine("    {");
        usd.AppendLine("        # Footsteps and heatmap data will go here");
        usd.AppendLine("    }");

        usd.AppendLine("}"); // Close Hospital

        // Write file
        File.WriteAllText(fullPath, usd.ToString());
        Debug.Log($"✅ Hospital exported to USD: {fullPath}");

        // Open folder
#if UNITY_EDITOR_WIN
        System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(fullPath).Replace('/', '\\'));
#elif UNITY_EDITOR_OSX
        System.Diagnostics.Process.Start("open", Path.GetDirectoryName(fullPath));
#endif
    }

    private void ExportFloor(StringBuilder usd)
    {
        usd.AppendLine("        def Cube \"Floor\"");
        usd.AppendLine("        {");
        usd.AppendLine("            double size = 1");
        usd.AppendLine("            double3 xformOp:scale = (100, 0.1, 100)");
        usd.AppendLine("            double3 xformOp:translate = (0, 0, 0)");
        usd.AppendLine("            uniform token[] xformOpOrder = [\"xformOp:translate\", \"xformOp:scale\"]");
        usd.AppendLine("            color3f[] primvars:displayColor = [(0.8, 0.8, 0.8)]");
        usd.AppendLine("        }");
        usd.AppendLine();
    }

    private void ExportRoom(StringBuilder usd, RoomData room)
    {
        usd.AppendLine($"        def Cube \"{room.roomType}_{room.id}\"");
        usd.AppendLine("        {");

        // Geometry
        usd.AppendLine("            double size = 1");
        usd.AppendLine($"            double3 xformOp:scale = ({room.size.x}, {room.height}, {room.size.y})");
        usd.AppendLine($"            double3 xformOp:translate = ({room.position.x}, {room.height / 2}, {room.position.y})");
        usd.AppendLine("            uniform token[] xformOpOrder = [\"xformOp:translate\", \"xformOp:scale\"]");

        // Color based on room type
        Color roomColor = GetRoomColor(room.roomType);
        usd.AppendLine($"            color3f[] primvars:displayColor = [({roomColor.r:F2}, {roomColor.g:F2}, {roomColor.b:F2})]");

        // Metadata - THIS IS KEY for Omniverse simulation
        usd.AppendLine();
        usd.AppendLine("            # Hospital Metadata");
        usd.AppendLine($"            custom string hospital:roomType = \"{room.roomType}\"");
        usd.AppendLine($"            custom int hospital:capacity = {room.capacity}");
        usd.AppendLine($"            custom string hospital:function = \"{room.function}\"");
        usd.AppendLine($"            custom int hospital:roomID = {room.id}");

        usd.AppendLine("        }");
        usd.AppendLine();
    }

    private Color GetRoomColor(string roomType)
    {
        switch (roomType)
        {
            case "ER_Room": return new Color(1f, 0.2f, 0.2f); // Red
            case "Patient_Room": return new Color(1f, 1f, 0.3f); // Yellow
            case "Waiting_Area": return new Color(0.3f, 0.5f, 1f); // Blue
            case "Nursing_Station": return new Color(0.3f, 1f, 0.3f); // Green
            case "Imaging_Room": return new Color(0.8f, 0.3f, 1f); // Purple
            case "Entrance": return new Color(0.3f, 1f, 1f); // Cyan
            default: return new Color(0.7f, 0.7f, 0.7f); // Gray
        }
    }

    /// <summary>
    /// Quick test - add some sample rooms
    /// </summary>
    [ContextMenu("Generate Test Data")]
    public void GenerateTestData()
    {
        rooms.Clear();

        rooms.Add(new RoomData
        {
            id = 0,
            roomType = "Entrance",
            position = new Vector2(0, -20),
            size = new Vector2(8, 6),
            height = 3,
            capacity = 50,
            function = "Main entrance"
        });

        rooms.Add(new RoomData
        {
            id = 1,
            roomType = "Waiting_Area",
            position = new Vector2(-10, -10),
            size = new Vector2(12, 10),
            height = 3,
            capacity = 30,
            function = "Patient waiting"
        });

        rooms.Add(new RoomData
        {
            id = 2,
            roomType = "ER_Room",
            position = new Vector2(15, -5),
            size = new Vector2(8, 8),
            height = 3,
            capacity = 10,
            function = "Emergency care"
        });

        rooms.Add(new RoomData
        {
            id = 3,
            roomType = "Patient_Room",
            position = new Vector2(-5, 10),
            size = new Vector2(6, 5),
            height = 3,
            capacity = 2,
            function = "Patient care"
        });

        Debug.Log($"✅ Generated {rooms.Count} test rooms");
    }
}

/// <summary>
/// Data structure for a single room
/// </summary>
[System.Serializable]
public class RoomData
{
    public int id;
    public string roomType;
    public Vector2 position;  // X, Z position (Y is up)
    public Vector2 size;      // Width, Depth
    public float height = 3f;
    public int capacity;
    public string function;
}