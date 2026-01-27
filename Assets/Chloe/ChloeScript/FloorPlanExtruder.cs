using UnityEngine;

public class FloorPlanExtruder : MonoBehaviour
{
    [Header("Input")]
    public Texture2D floorPlan;

    [Header("Scale Settings")]
    [Tooltip("World units per pixel (e.g. 0.1 = 10cm per pixel)")]
    public float pixelToWorld = 0.1f;

    [Tooltip("Wall height in world units")]
    public float wallHeight = 3f;

    [Header("Wall")]
    public GameObject wallPrefab;

    [Header("Generation")]
    public bool generateOnStart = true;

    void Start()
    {
        if (generateOnStart)
            Generate();
    }

    public void Generate()
    {
        if (floorPlan == null)
        {
            Debug.LogError("No floor plan texture assigned.");
            return;
        }

        if (wallPrefab == null)
        {
            Debug.LogError("No wall prefab assigned.");
            return;
        }

        ClearGenerated();

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

        // Merge horizontal wall runs
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
                    wallHeight * 0.5f,
                    y * pixelToWorld
                );

                GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
                wall.transform.localScale = new Vector3(
                    length * pixelToWorld,
                    wallHeight,
                    pixelToWorld
                );
            }
        }

        Debug.Log("Floor plan generation complete.");
    }

    void ClearGenerated()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
