using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FuturisticWallExtruder : MonoBehaviour
{
    [Header("Input")]
    public Texture2D floorPlan;
    public GameObject visualFloor;

    [Header("Wall Settings")]
    public GameObject wallPrefab;
    public float wallHeight = 100f;

    [Header("Animation")]
    public float delayBeforeExtrusion = 2f;
    public float extrusionDuration = 3f;
    public AnimationCurve extrusionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Effects")]
    public Color wallGlowColor = new Color(0, 1, 1, 1);
    public float glowIntensity = 2f;

    [Header("Status")]
    public bool wallsGenerated = false;

    private List<GameObject> walls = new List<GameObject>();
    private float pixelToWorld;
    private Vector3 floorBottomLeft;
    private float floorY;
    private Material wallMaterial; // Cache the material

    public delegate void WallsCompleteCallback();
    public event WallsCompleteCallback OnWallsComplete;

    void Start()
    {
        // Create material ONCE at start
        CreateWallMaterial();
        StartCoroutine(GenerateWithAnimation());
    }

    void CreateWallMaterial()
    {
        // Try Standard shader first
        Shader shader = Shader.Find("Standard");

        if (shader == null)
        {
            Debug.LogWarning("[WallExtruder] Standard shader not found, using Unlit/Color");
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            Debug.LogError("[WallExtruder] No shader found!");
            return;
        }

        wallMaterial = new Material(shader);

        // Base color
        wallMaterial.color = new Color(0.2f, 0.3f, 0.4f);

        // Try to enable emission (only if Standard shader)
        if (shader.name == "Standard")
        {
            wallMaterial.EnableKeyword("_EMISSION");
            wallMaterial.SetColor("_EmissionColor", wallGlowColor * glowIntensity);
            wallMaterial.SetFloat("_Metallic", 0.7f);
            wallMaterial.SetFloat("_Glossiness", 0.8f);
        }

        Debug.Log($"[WallExtruder] Created material using shader: {shader.name}");
    }

    IEnumerator GenerateWithAnimation()
    {
        if (floorPlan == null || wallPrefab == null || visualFloor == null)
        {
            Debug.LogError("[WallExtruder] Missing required references!");
            yield break;
        }

        if (wallMaterial == null)
        {
            Debug.LogError("[WallExtruder] Failed to create wall material!");
            yield break;
        }

        CalculateFloorMapping();

        Debug.Log($"[WallExtruder] Waiting {delayBeforeExtrusion}s before wall generation...");
        yield return new WaitForSeconds(delayBeforeExtrusion);

        Debug.Log("[WallExtruder] Generating walls...");
        GenerateWalls();

        yield return StartCoroutine(AnimateWallExtrusion());

        wallsGenerated = true;
        Debug.Log("[WallExtruder] ✓ Wall generation complete!");

        OnWallsComplete?.Invoke();
    }

    void CalculateFloorMapping()
    {
        Vector3 floorPos = visualFloor.transform.position;
        Vector3 floorScale = visualFloor.transform.localScale;

        float floorWorldWidth = floorScale.x * 10f;
        float floorWorldDepth = floorScale.z * 10f;

        float scaleX = floorWorldWidth / floorPlan.width;
        float scaleZ = floorWorldDepth / floorPlan.height;

        pixelToWorld = scaleX;

        floorBottomLeft = new Vector3(
            floorPos.x - floorWorldWidth * 0.5f,
            floorPos.y,
            floorPos.z - floorWorldDepth * 0.5f
        );

        floorY = floorPos.y;

        Debug.Log($"[WallExtruder] Floor: {floorWorldWidth:F1} x {floorWorldDepth:F1}");
        Debug.Log($"[WallExtruder] Texture: {floorPlan.width} x {floorPlan.height}");
        Debug.Log($"[WallExtruder] Scale: {scaleX:F4}");
    }

    void GenerateWalls()
    {
        int width = floorPlan.width;
        int height = floorPlan.height;
        Color32[] pixels = floorPlan.GetPixels32();

        bool[,] wallMap = new bool[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 p = pixels[y * width + x];
                wallMap[x, y] = (p.r < 10 && p.g < 10 && p.b < 10);
            }
        }

        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                if (!wallMap[x, y])
                {
                    x++;
                    continue;
                }

                int startX = x;
                while (x < width && wallMap[x, y])
                    x++;

                int length = x - startX;
                CreateWallSegment(startX, y, length);
            }
        }

        Debug.Log($"[WallExtruder] Created {walls.Count} wall segments");
    }

    void CreateWallSegment(int startX, int textureY, int length)
    {
        Vector3 floorScale = visualFloor.transform.localScale;
        float floorWorldWidth = floorScale.x * 10f;
        float floorWorldDepth = floorScale.z * 10f;

        float scaleX = floorWorldWidth / floorPlan.width;
        float scaleZ = floorWorldDepth / floorPlan.height;

        float worldX = floorBottomLeft.x + (floorPlan.width - startX - length * 0.5f) * scaleX;
        float worldZ = floorBottomLeft.z + (floorPlan.height - 1 - textureY) * scaleZ;

        Vector3 position = new Vector3(worldX, floorY + 0.5f, worldZ);

        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
        wall.transform.localScale = new Vector3(length * scaleX, 1f, scaleZ);

        // Apply material IMMEDIATELY
        MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
        if (renderer != null && wallMaterial != null)
        {
            renderer.material = wallMaterial;
        }
        else
        {
            Debug.LogError("[WallExtruder] Wall has no renderer or material is null!");
        }

        walls.Add(wall);
    }

    IEnumerator AnimateWallExtrusion()
    {
        if (walls.Count == 0) yield break;

        float elapsed = 0f;
        Vector3[] startScales = new Vector3[walls.Count];
        Vector3[] targetScales = new Vector3[walls.Count];
        Vector3[] startPositions = new Vector3[walls.Count];
        Vector3[] targetPositions = new Vector3[walls.Count];

        for (int i = 0; i < walls.Count; i++)
        {
            startScales[i] = walls[i].transform.localScale;
            startPositions[i] = walls[i].transform.position;

            targetScales[i] = new Vector3(
                startScales[i].x,
                wallHeight,
                startScales[i].z
            );

            targetPositions[i] = new Vector3(
                startPositions[i].x,
                floorY + wallHeight * 0.5f,
                startPositions[i].z
            );
        }

        while (elapsed < extrusionDuration)
        {
            elapsed += Time.deltaTime;
            float t = extrusionCurve.Evaluate(elapsed / extrusionDuration);

            for (int i = 0; i < walls.Count; i++)
            {
                if (walls[i] != null)
                {
                    walls[i].transform.localScale = Vector3.Lerp(startScales[i], targetScales[i], t);
                    walls[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                }
            }

            yield return null;
        }

        for (int i = 0; i < walls.Count; i++)
        {
            if (walls[i] != null)
            {
                walls[i].transform.localScale = targetScales[i];
                walls[i].transform.position = targetPositions[i];
            }
        }
    }

    public List<GameObject> GetWalls()
    {
        return walls;
    }
}