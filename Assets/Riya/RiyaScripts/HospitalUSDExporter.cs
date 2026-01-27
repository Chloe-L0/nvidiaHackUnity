using UnityEngine;
using Unity.Formats.USD;
using USD.NET;
using pxr;
using System.IO;
using System.Collections.Generic;

public class HospitalUSDExporter : MonoBehaviour
{
    [Header("Export Settings")]
    public string exportFileName = "hospital_layout";
    public string exportPath = "Exports";

    [Header("Scene References")]
    public HospitalRoom[] rooms;
    public GameObject floorNavMesh;

    [Header("Footstep Data")]
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

        Debug.Log($"Starting USD export to: {usdFilePath}");

        ExportHospitalScene(usdFilePath);

        Debug.Log($"USD export completed: {usdFilePath}");
        Debug.Log($"File location: {usdFilePath}");
    }

    void ExportHospitalScene(string path)
    {
        var scene = Scene.Create(path);

        if (scene == null)
        {
            Debug.LogError("Failed to create USD scene!");
            return;
        }

        scene.Time = 0;
        scene.StartTime = 0;
        scene.EndTime = 1;

        var hospitalRoot = scene.Stage.DefinePrim(new pxr.SdfPath("/Hospital"), new pxr.TfToken("Xform"));

        ExportGeometry(scene);
        ExportFootstepData(scene);

        scene.Save();
        scene.Close();
    }

    void ExportGeometry(Scene scene)
    {
        Debug.Log("Exporting geometry...");

        var geomPath = new pxr.SdfPath("/Hospital/Geometry");
        var geomPrim = scene.Stage.DefinePrim(geomPath, new pxr.TfToken("Xform"));

        if (rooms != null)
        {
            foreach (var room in rooms)
            {
                if (room != null)
                {
                    ExportRoom(scene, room);
                }
            }
        }

        if (floorNavMesh != null)
        {
            ExportFloor(scene, floorNavMesh);
        }
    }

    void ExportRoom(Scene scene, HospitalRoom room)
    {
        string roomPath = $"/Hospital/Geometry/{room.roomID}";

        MeshFilter meshFilter = room.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning($"Room {room.roomID} has no mesh, skipping...");
            return;
        }

        var roomPrim = scene.Stage.DefinePrim(new pxr.SdfPath(roomPath), new pxr.TfToken("Xform"));

        roomPrim.SetCustomDataByKey(new pxr.TfToken("hospital:roomType"), new pxr.VtValue(room.roomType));
        roomPrim.SetCustomDataByKey(new pxr.TfToken("hospital:roomID"), new pxr.VtValue(room.roomID));

        ExportMesh(scene, meshFilter, roomPath + "/mesh", room.transform);

        Debug.Log($"Exported room: {room.roomID}");
    }

    void ExportMesh(Scene scene, MeshFilter meshFilter, string path, Transform transform)
    {
        Mesh mesh = meshFilter.sharedMesh;

        var meshPrim = scene.Stage.DefinePrim(new pxr.SdfPath(path), new pxr.TfToken("Mesh"));
        var usdMesh = new pxr.UsdGeomMesh(meshPrim);

        var points = new pxr.VtVec3fArray((uint)mesh.vertices.Length);
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 v = transform.TransformPoint(mesh.vertices[i]);
            points[i] = new pxr.GfVec3f(v.x, v.y, v.z);
        }
        usdMesh.GetPointsAttr().Set(points);

        var faceVertexIndices = new pxr.VtIntArray((uint)mesh.triangles.Length);
        var faceVertexCounts = new pxr.VtIntArray((uint)(mesh.triangles.Length / 3));

        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            faceVertexIndices[i] = mesh.triangles[i];
        }

        for (int i = 0; i < mesh.triangles.Length / 3; i++)
        {
            faceVertexCounts[i] = 3;
        }

        usdMesh.GetFaceVertexIndicesAttr().Set(faceVertexIndices);
        usdMesh.GetFaceVertexCountsAttr().Set(faceVertexCounts);
    }

    void ExportFloor(Scene scene, GameObject floor)
    {
        string floorPath = "/Hospital/Geometry/Floor";

        MeshFilter meshFilter = floor.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            var floorPrim = scene.Stage.DefinePrim(new pxr.SdfPath(floorPath), new pxr.TfToken("Xform"));
            ExportMesh(scene, meshFilter, floorPath + "/mesh", floor.transform);
            Debug.Log("Exported floor");
        }
    }

    void ExportFootstepData(Scene scene)
    {
        if (footstepRecorder == null)
        {
            Debug.LogWarning("FootstepRecorder reference not assigned!");
            return;
        }

        List<FootstepData> footsteps = footstepRecorder.GetFootstepsForExport();

        if (footsteps == null || footsteps.Count == 0)
        {
            Debug.LogWarning("No footstep data to export!");
            return;
        }

        Debug.Log($"Exporting {footsteps.Count} footsteps...");

        var trafficPath = new pxr.SdfPath("/Hospital/TrafficData");
        var trafficPrim = scene.Stage.DefinePrim(trafficPath, new pxr.TfToken("Xform"));

        var footstepsPath = new pxr.SdfPath("/Hospital/TrafficData/Footsteps");
        var footstepsPrim = scene.Stage.DefinePrim(footstepsPath, new pxr.TfToken("Points"));

        var positions = new pxr.VtVec3fArray((uint)footsteps.Count);

        for (int i = 0; i < footsteps.Count; i++)
        {
            FootstepData step = footsteps[i];
            positions[i] = new pxr.GfVec3f(step.position.x, step.position.y, step.position.z);
        }

        var pointsPrim = new pxr.UsdGeomPoints(footstepsPrim);
        pointsPrim.GetPointsAttr().Set(positions);

        footstepsPrim.SetCustomDataByKey(new pxr.TfToken("footstepCount"),
            new pxr.VtValue(footsteps.Count));

        Debug.Log($"Footstep positions exported: {footsteps.Count} points");
    }
}