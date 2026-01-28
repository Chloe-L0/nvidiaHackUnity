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

    public delegate void WallsCompleteCallback();
    public event WallsCompleteCallback OnWallsComplete;

    void Start()
    {
        StartCoroutine(GenerateWithAnimation());
    }

    IEnumerator GenerateWithAnimation()
    {
        // Validation
        if (floorPlan == null)
        {
            Debug.LogError("[WallExtruder] No floor plan texture assigned!");
            yield break;
        }

        if (wallPrefab == null)
        {
            Debug.LogError("[WallExtruder] No wall prefab assigned!");
            yield break;
        }

        if (visualFloor == null)
        {
            Debug.LogError("[WallExtruder] No visual floor assigned!");
            yield break;
        }

        // Calculate floor dimensions and scale
        CalculateFloorMapping();

        Debug.Log($"[WallExtruder] Waiting {delayBeforeExtrusion}s before wall generation...");
        yield return new WaitForSeconds(delayBeforeExtrusion);

        Debug.Log("[WallExtruder] Generating walls...");

        // Generate walls
        GenerateWalls();

        // Animate extrusion
        yield return StartCoroutine(AnimateWallExtrusion());

        wallsGenerated = true;
        Debug.Log("[WallExtruder] ✓ Wall generation complete!");

        OnWallsComplete?.Invoke();
    }

    void CalculateFloorMapping()
    {
        // Get floor transform data
        Vector3 floorPos = visualFloor.transform.position;
        Vector3 floorScale = visualFloor.transform.localScale;

        // Unity plane is 10x10 units at scale 1
        float floorWorldWidth = floorScale.x * 10f;
        float floorWorldDepth = floorScale.z * 10f;

        // Use exact scale (don't use Min - causes offset)
        // We want texture to stretch to match floor exactly
        float scaleX = floorWorldWidth / floorPlan.width;
        float scaleZ = floorWorldDepth / floorPlan.height;

        // Store as a single value - use X scale for horizontal, Z scale for depth
        // But they should be the same if floor aspect matches texture aspect
        pixelToWorld = scaleX; // Use X scale

        // Verify aspect ratio match
        float textureAspect = (float)floorPlan.width / floorPlan.height;
        float floorAspect = floorWorldWidth / floorWorldDepth;

        if (Mathf.Abs(textureAspect - floorAspect) > 0.01f)
        {
            Debug.LogWarning($"[WallExtruder] Aspect ratio mismatch! Texture: {textureAspect:F2}, Floor: {floorAspect:F2}");
        }

        // Calculate bottom-left corner of floor in world space
        floorBottomLeft = new Vector3(
            floorPos.x - floorWorldWidth * 0.5f,
            floorPos.y,
            floorPos.z - floorWorldDepth * 0.5f
        );

        floorY = floorPos.y;

        Debug.Log($"[WallExtruder] Floor: pos={floorPos}, size={floorWorldWidth:F1} x {floorWorldDepth:F1}");
        Debug.Log($"[WallExtruder] Texture: {floorPlan.width} x {floorPlan.height}");
        Debug.Log($"[WallExtruder] Scale X: {scaleX:F4}, Scale Z: {scaleZ:F4}");
        Debug.Log($"[WallExtruder] Bottom-left: {floorBottomLeft}");
    }

    void GenerateWalls()
    {
        int width = floorPlan.width;
        int height = floorPlan.height;
        Color32[] pixels = floorPlan.GetPixels32();

        // Build wall map (black pixels = walls)
        bool[,] wallMap = new bool[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 p = pixels[y * width + x];
                wallMap[x, y] = (p.r < 10 && p.g < 10 && p.b < 10);
            }
        }

        // Generate horizontal wall segments
        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                // Skip non-wall pixels
                if (!wallMap[x, y])
                {
                    x++;
                    continue;
                }

                // Find wall segment length
                int startX = x;
                while (x < width && wallMap[x, y])
                {
                    x++;
                }
                int length = x - startX;

                // Create wall segment
                CreateWallSegment(startX, y, length);
            }
        }

        Debug.Log($"[WallExtruder] Created {walls.Count} wall segments");
    }

    void CreateWallSegment(int startX, int textureY, int length)
    {
        // Calculate pixel-to-world scale for each axis separately
        Vector3 floorScale = visualFloor.transform.localScale;
        float floorWorldWidth = floorScale.x * 10f;
        float floorWorldDepth = floorScale.z * 10f;

        float scaleX = floorWorldWidth / floorPlan.width;
        float scaleZ = floorWorldDepth / floorPlan.height;

        // Convert texture coordinates to world coordinates
        // Flip X axis
        float worldX = floorBottomLeft.x + (floorPlan.width - startX - length * 0.5f) * scaleX;
        // Flip Y axis (texture Y → world Z)
        float worldZ = floorBottomLeft.z + (floorPlan.height - 1 - textureY) * scaleZ;

        Vector3 position = new Vector3(
            worldX,
            floorY + 0.5f,
            worldZ
        );

        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);

        wall.transform.localScale = new Vector3(
            length * scaleX,  // Use X scale for width
            1f,
            scaleZ            // Use Z scale for depth
        );

        ApplyFuturisticMaterial(wall);
        walls.Add(wall);
    }

    IEnumerator AnimateWallExtrusion()
    {
        if (walls.Count == 0) yield break;

        float elapsed = 0f;

        // Store start and target values for each wall
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

        // Animate
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

        // Ensure final values
        for (int i = 0; i < walls.Count; i++)
        {
            if (walls[i] != null)
            {
                walls[i].transform.localScale = targetScales[i];
                walls[i].transform.position = targetPositions[i];
            }
        }
    }

    void ApplyFuturisticMaterial(GameObject wall)
    {
        MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.3f, 0.4f);

        // Emission glow
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", wallGlowColor * glowIntensity);

        // Metallic look
        mat.SetFloat("_Metallic", 0.7f);
        mat.SetFloat("_Glossiness", 0.8f);

        renderer.material = mat;
    }

    public List<GameObject> GetWalls()
    {
        return walls;
    }
}