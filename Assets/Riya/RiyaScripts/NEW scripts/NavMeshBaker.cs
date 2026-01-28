using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    [Header("Floor Settings")]
    public GameObject floorObject;
    public Vector3 floorCenter = Vector3.zero;
    public Vector2 floorSize = new Vector2(2100, 1100);

    [Header("NavMesh Settings")]
    public float agentRadius = 5f;
    public float agentHeight = 20f;
    public float agentSlope = 45f;      // ✅ Correct property name
    public float agentClimb = 4f;       // ✅ Correct property name

    [Header("Status")]
    public bool navMeshBaked = false;

    public delegate void NavMeshCompleteCallback();
    public event NavMeshCompleteCallback OnNavMeshComplete;

    public void BakeNavMesh()
    {
        Debug.Log("[NavMeshBaker] Baking NavMesh...");

        NavMeshSurface surface = gameObject.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = gameObject.AddComponent<NavMeshSurface>();
        }

        // Position at floor level
        transform.position = Vector3.zero;

        // Use volume-based collection with height constraint
        surface.collectObjects = CollectObjects.Volume;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.size = new Vector3(floorSize.x, 5f, floorSize.y);  // Height = 5 units only
        surface.center = new Vector3(0, 2.5f, 0);

        surface.BuildNavMesh();

        navMeshBaked = true;
        Debug.Log("[NavMeshBaker] ✓ NavMesh baking complete!");

        OnNavMeshComplete?.Invoke();
    }

    void OnValidate()
    {
        if (floorObject == null)
        {
            floorObject = GameObject.Find("Floor");
        }
    }
}