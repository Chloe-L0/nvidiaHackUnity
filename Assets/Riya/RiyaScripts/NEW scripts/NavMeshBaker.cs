using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    [Header("Floor Settings")]
    public Vector3 floorCenter = Vector3.zero;
    public Vector2 floorSize = new Vector2(2100, 1100);

    [Header("NavMesh Settings")]
    public float agentRadius = 5f;
    public float agentHeight = 20f;

    [Header("Status")]
    public bool navMeshBaked = false;

    public delegate void NavMeshCompleteCallback();
    public event NavMeshCompleteCallback OnNavMeshComplete;

    public void BakeNavMesh()
    {
        Debug.Log("[NavMeshBaker] Baking NavMesh...");

        // Create NavMesh surface
        NavMeshSurface surface = gameObject.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = gameObject.AddComponent<NavMeshSurface>();
        }

        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;

        // Build the NavMesh
        surface.BuildNavMesh();

        navMeshBaked = true;
        Debug.Log("[NavMeshBaker] ✓ NavMesh baking complete!");

        OnNavMeshComplete?.Invoke();
    }
}