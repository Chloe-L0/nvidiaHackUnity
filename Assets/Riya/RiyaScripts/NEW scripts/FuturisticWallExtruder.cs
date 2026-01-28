using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FuturisticWallExtruder : MonoBehaviour
{
    [Header("Input")]
    public Texture2D floorPlan;

    [Header("Scale Settings")]
    public float pixelToWorld = 10f;
    public float wallHeight = 300f;

    [Header("Wall Prefab")]
    public GameObject wallPrefab;

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

    public delegate void WallsCompleteCallback();
    public event WallsCompleteCallback OnWallsComplete;

    void Start()
    {
        StartCoroutine(GenerateWithAnimation());
    }

    IEnumerator GenerateWithAnimation()
    {
        if (floorPlan == null || wallPrefab == null)
        {
            Debug.LogError("[WallExtruder] Missing floor plan or wall prefab!");
            yield break;
        }

        Debug.Log($"[WallExtruder] Waiting {delayBeforeExtrusion}s before wall generation...");
        yield return new WaitForSeconds(delayBeforeExtrusion);

        Debug.Log("[WallExtruder] Starting wall generation...");

        // Generate wall positions
        int width = floorPlan.width;
        int height = floorPlan.height;
        Color32[] pixels = floorPlan.GetPixels32();
        bool[,] wallMap = new bool[width, height];

        // Build wall map (black = wall)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 p = pixels[y * width + x];
                wallMap[x, y] = (p.r < 10 && p.g < 10 && p.b < 10);
            }
        }

        // Create walls (start flat)
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

                Vector3 position = new Vector3(
                    (startX + length * 0.5f) * pixelToWorld,
                    0.5f, // Start almost flat
                    y * pixelToWorld
                );

                GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
                wall.transform.localScale = new Vector3(
                    length * pixelToWorld,
                    1f, // Start nearly flat
                    pixelToWorld
                );

                // Apply futuristic material
                ApplyFuturisticMaterial(wall);

                walls.Add(wall);
            }
        }

        Debug.Log($"[WallExtruder] Created {walls.Count} wall segments");

        // Animate extrusion
        yield return StartCoroutine(AnimateWallExtrusion());

        wallsGenerated = true;
        Debug.Log("[WallExtruder] ✓ Wall generation complete!");

        OnWallsComplete?.Invoke();
    }

    IEnumerator AnimateWallExtrusion()
    {
        float elapsed = 0f;
        Vector3[] startScales = new Vector3[walls.Count];
        Vector3[] targetScales = new Vector3[walls.Count];
        Vector3[] startPositions = new Vector3[walls.Count];
        Vector3[] targetPositions = new Vector3[walls.Count];

        // Store start and target values
        for (int i = 0; i < walls.Count; i++)
        {
            startScales[i] = walls[i].transform.localScale;
            targetScales[i] = new Vector3(
                startScales[i].x,
                wallHeight,
                startScales[i].z
            );

            startPositions[i] = walls[i].transform.position;
            targetPositions[i] = new Vector3(
                startPositions[i].x,
                wallHeight * 0.5f,
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
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.3f, 0.4f);

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", wallGlowColor * glowIntensity);

            mat.SetFloat("_Metallic", 0.7f);
            mat.SetFloat("_Glossiness", 0.8f);

            renderer.material = mat;
        }
    }

    public List<GameObject> GetWalls()
    {
        return walls;
    }
}