using UnityEngine;
using Unity.Formats.USD;
using USD.NET;
using pxr;
using System.IO;
using System.Collections.Generic;

public class HospitalUSDExporter_Complete : MonoBehaviour
{
    [Header("Export Settings")]
    public string exportFileName = "hospital_simulation";
    public string exportPath = "ExportedUSD";

    [Header("Scene References")]
    public GameObject visualFloor;
    public FuturisticWallExtruder wallExtruder;
    public FootstepRecorder footstepRecorder;

    [ContextMenu("Export to USD")]
    public void ExportToUSD()
    {
        string fullExportPath = Path.Combine(Application.dataPath, "..", exportPath);
        if (!Directory.Exists(fullExportPath))
        {
            Directory.CreateDirectory(fullExportPath);
        }

        string usdFilePath = Path.Combine(fullExportPath, exportFileName + ".usda");

        Debug.Log($"[USD Export] Starting export to: {usdFilePath}");

        ExportSceneManual(usdFilePath);

        Debug.Log($"[USD Export] ✓ Complete! File: {usdFilePath}");
    }

    void ExportSceneManual(string path)
    {
        // Write USD file manually (simpler and more reliable)
        using (StreamWriter writer = new StreamWriter(path))
        {
            WriteUSDHeader(writer);
            WriteFloorUSD(writer);
            WriteWallsUSD(writer);
            WriteFootstepsUSD(writer);
            writer.WriteLine("}"); // Close root
        }

        Debug.Log("[USD Export] Scene saved successfully!");
    }

    void WriteUSDHeader(StreamWriter writer)
    {
        writer.WriteLine("#usda 1.0");
        writer.WriteLine("(");
        writer.WriteLine("    defaultPrim = \"Hospital\"");
        writer.WriteLine("    upAxis = \"Y\"");
        writer.WriteLine("    metersPerUnit = 1.0");
        writer.WriteLine(")");
        writer.WriteLine();
        writer.WriteLine("def Xform \"Hospital\"");
        writer.WriteLine("{");
    }

    void WriteFloorUSD(StreamWriter writer)
    {
        if (visualFloor == null)
        {
            Debug.LogWarning("[USD Export] No floor assigned!");
            return;
        }

        Vector3 pos = visualFloor.transform.position;
        Vector3 scale = visualFloor.transform.localScale;

        float halfW = scale.x * 5;
        float halfD = scale.z * 5;

        writer.WriteLine("    def Mesh \"Floor\"");
        writer.WriteLine("    {");
        writer.WriteLine($"        double3 xformOp:translate = ({pos.x}, {pos.y}, {pos.z})");
        writer.WriteLine("        uniform token[] xformOpOrder = [\"xformOp:translate\"]");
        writer.WriteLine();
        writer.WriteLine("        point3f[] points = [");
        writer.WriteLine($"            ({-halfW}, {pos.y}, {-halfD}),");
        writer.WriteLine($"            ({halfW}, {pos.y}, {-halfD}),");
        writer.WriteLine($"            ({halfW}, {pos.y}, {halfD}),");
        writer.WriteLine($"            ({-halfW}, {pos.y}, {halfD})");
        writer.WriteLine("        ]");
        writer.WriteLine("        int[] faceVertexCounts = [4]");
        writer.WriteLine("        int[] faceVertexIndices = [0, 1, 2, 3]");
        writer.WriteLine("    }");
        writer.WriteLine();

        Debug.Log("[USD Export] ✓ Floor exported");
    }

    void WriteWallsUSD(StreamWriter writer)
    {
        if (wallExtruder == null)
        {
            Debug.LogWarning("[USD Export] No wall extruder assigned!");
            return;
        }

        List<GameObject> walls = wallExtruder.GetWalls();
        if (walls == null || walls.Count == 0)
        {
            Debug.LogWarning("[USD Export] No walls to export!");
            return;
        }

        writer.WriteLine("    def Xform \"Walls\"");
        writer.WriteLine("    {");

        int wallCount = 0;
        foreach (GameObject wall in walls)
        {
            if (wall == null) continue;

            Vector3 pos = wall.transform.position;
            Vector3 scale = wall.transform.localScale;

            writer.WriteLine($"        def Cube \"Wall_{wallCount}\"");
            writer.WriteLine("        {");
            writer.WriteLine($"            double3 xformOp:translate = ({pos.x}, {pos.y}, {pos.z})");
            writer.WriteLine($"            double3 xformOp:scale = ({scale.x}, {scale.y}, {scale.z})");
            writer.WriteLine("            uniform token[] xformOpOrder = [\"xformOp:translate\", \"xformOp:scale\"]");
            writer.WriteLine("            double size = 1.0");
            writer.WriteLine("        }");

            wallCount++;
        }

        writer.WriteLine("    }");
        writer.WriteLine();

        Debug.Log($"[USD Export] ✓ Exported {wallCount} walls");
    }

    void WriteFootstepsUSD(StreamWriter writer)
    {
        if (footstepRecorder == null)
        {
            Debug.LogWarning("[USD Export] No footstep recorder!");
            return;
        }

        List<FootstepData> footsteps = footstepRecorder.GetFootstepsForExport();

        if (footsteps == null || footsteps.Count == 0)
        {
            Debug.LogWarning("[USD Export] No footstep data!");
            return;
        }

        writer.WriteLine("    def Xform \"TrafficData\"");
        writer.WriteLine("    {");
        writer.WriteLine($"        # Total footsteps: {footsteps.Count}");
        writer.WriteLine();

        // Group by agent type
        var grouped = new Dictionary<string, List<FootstepData>>();
        foreach (var fp in footsteps)
        {
            string type = fp.agentType ?? "unknown";
            if (!grouped.ContainsKey(type))
                grouped[type] = new List<FootstepData>();
            grouped[type].Add(fp);
        }

        foreach (var kvp in grouped)
        {
            writer.WriteLine($"        def Points \"{kvp.Key}_Footsteps\"");
            writer.WriteLine("        {");
            writer.WriteLine($"            int footstepCount = {kvp.Value.Count}");

            // Write positions
            writer.Write("            point3f[] points = [");
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                var pos = kvp.Value[i].position;
                writer.Write($"({pos.x}, {pos.y}, {pos.z})");
                if (i < kvp.Value.Count - 1) writer.Write(", ");
            }
            writer.WriteLine("]");

            // Write timestamps
            writer.Write("            float[] timestamps = [");
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                writer.Write($"{kvp.Value[i].timestamp}");
                if (i < kvp.Value.Count - 1) writer.Write(", ");
            }
            writer.WriteLine("]");

            writer.WriteLine("        }");
            writer.WriteLine();
        }

        writer.WriteLine("    }");

        Debug.Log($"[USD Export] ✓ Exported {footsteps.Count} footsteps ({grouped.Count} agent types)");
    }

    void OnValidate()
    {
        if (visualFloor == null)
        {
            visualFloor = GameObject.Find("Floor");
            if (visualFloor == null) visualFloor = GameObject.Find("VisualFloor");
        }

        if (wallExtruder == null)
            wallExtruder = FindObjectOfType<FuturisticWallExtruder>();

        if (footstepRecorder == null)
            footstepRecorder = FindObjectOfType<FootstepRecorder>();
    }
}