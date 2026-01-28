using UnityEngine;
using System.Collections;

public class SimulationSequencer : MonoBehaviour
{
    [Header("References")]
    public GameObject visualFloor;
    public FuturisticWallExtruder wallExtruder;
    public NavMeshBaker navMeshBaker;
    public AgentSpawner agentSpawner;
    public FootstepRecorder footstepRecorder;

    [Header("Floor Material")]
    public Texture2D floorPlanTexture;
    public Material floorMaterial;

    [Header("Sequence Status")]
    public bool floorReady = false;
    public bool wallsReady = false;
    public bool navMeshReady = false;
    public bool simulationRunning = false;

    void Start()
    {
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        Debug.Log("=== SIMULATION SEQUENCE START ===");

        // STEP 1: Setup Floor with Texture
        yield return StartCoroutine(SetupFloor());

        // STEP 2: Wait for Walls to Generate and Rise
        yield return StartCoroutine(WaitForWalls());

        // STEP 3: Bake NavMesh
        yield return StartCoroutine(BakeNavMesh());

        // STEP 4: Start Agent Simulation
        StartSimulation();

        Debug.Log("=== SIMULATION SEQUENCE COMPLETE ===");
    }

    IEnumerator SetupFloor()
    {
        Debug.Log("[Sequencer] STEP 1: Setting up floor...");

        if (visualFloor != null && floorPlanTexture != null)
        {
            MeshRenderer renderer = visualFloor.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (floorMaterial == null)
                {
                    floorMaterial = new Material(Shader.Find("Standard"));
                }

                floorMaterial.mainTexture = floorPlanTexture;
                floorMaterial.EnableKeyword("_EMISSION");
                floorMaterial.SetColor("_EmissionColor", new Color(0.2f, 0.3f, 0.5f) * 0.3f);

                renderer.material = floorMaterial;

                Debug.Log("[Sequencer] ✓ Floor texture applied");
            }
        }

        floorReady = true;
        yield return null;
    }

    IEnumerator WaitForWalls()
    {
        Debug.Log("[Sequencer] STEP 2: Waiting for walls to generate...");

        if (wallExtruder != null)
        {
            // Subscribe to wall completion event
            bool wallsDone = false;
            wallExtruder.OnWallsComplete += () => { wallsDone = true; };

            // Wait for walls to complete
            while (!wallsDone)
            {
                yield return null;
            }

            Debug.Log("[Sequencer] ✓ Walls complete");
        }

        wallsReady = true;
    }

    IEnumerator BakeNavMesh()
    {
        Debug.Log("[Sequencer] STEP 3: Baking NavMesh...");

        if (navMeshBaker != null)
        {
            bool navMeshDone = false;
            navMeshBaker.OnNavMeshComplete += () => { navMeshDone = true; };

            navMeshBaker.BakeNavMesh();

            while (!navMeshDone)
            {
                yield return null;
            }

            Debug.Log("[Sequencer] ✓ NavMesh complete");
        }

        navMeshReady = true;
        yield return new WaitForSeconds(0.5f); // Brief pause
    }

    void StartSimulation()
    {
        Debug.Log("[Sequencer] STEP 4: Starting agent simulation...");

        if (agentSpawner != null)
        {
            agentSpawner.enabled = true;
            Debug.Log("[Sequencer] ✓ Agent spawning started");
        }

        if (footstepRecorder != null)
        {
            footstepRecorder.enabled = true;
            Debug.Log("[Sequencer] ✓ Footstep recording started");
        }

        simulationRunning = true;
    }

    void OnValidate()
    {
        // Auto-find references
        if (visualFloor == null)
        {
            visualFloor = GameObject.Find("Floor");
            if (visualFloor == null) visualFloor = GameObject.Find("VisualFloor");
        }

        if (wallExtruder == null)
            wallExtruder = FindObjectOfType<FuturisticWallExtruder>();

        if (navMeshBaker == null)
            navMeshBaker = FindObjectOfType<NavMeshBaker>();

        if (agentSpawner == null)
            agentSpawner = FindObjectOfType<AgentSpawner>();

        if (footstepRecorder == null)
            footstepRecorder = FindObjectOfType<FootstepRecorder>();
    }
}