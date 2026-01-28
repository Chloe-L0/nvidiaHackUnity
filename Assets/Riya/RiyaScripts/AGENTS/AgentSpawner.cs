using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [Header("Agent Prefabs")]
    public GameObject patientPrefab;
    public GameObject emergencyPrefab;
    public GameObject staffPrefab;
    public GameObject visitorPrefab;

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public HospitalRoom[] allRooms;

    [Header("Spawn Rates")]
    public float spawnInterval = 3f;
    public int maxAgents = 50;

    private float nextSpawnTime;
    private int spawnedCount = 0;

    void Update()
    {
        if (spawnedCount >= maxAgents) return;

        if (Time.time > nextSpawnTime)
        {
            SpawnRandomAgent();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnRandomAgent()
    {
        float rand = Random.value;

        if (rand < 0.4f) SpawnPatient();
        else if (rand < 0.65f) SpawnVisitor();
        else if (rand < 0.85f) SpawnStaff();
        else SpawnEmergency();

        spawnedCount++;
    }

    void SpawnPatient()
    {
        if (patientPrefab == null) return;
        GameObject agent = Instantiate(patientPrefab, spawnPoint.position, Quaternion.identity);

        PatientAgent pa = agent.GetComponent<PatientAgent>();
        if (pa != null)
        {
            HospitalRoom targetRoom = GetRandomRoomForAgent("patient");
            pa.Initialize(targetRoom);
        }
    }

    void SpawnEmergency()
    {
        if (emergencyPrefab == null) return;
        GameObject agent = Instantiate(emergencyPrefab, spawnPoint.position, Quaternion.identity);

        EmergencyAgent ea = agent.GetComponent<EmergencyAgent>();
        if (ea != null)
        {
            HospitalRoom targetRoom = GetRandomRoomForAgent("emergency");
            ea.Initialize(targetRoom);
        }
    }

    void SpawnStaff()
    {
        if (staffPrefab == null) return;
        GameObject agent = Instantiate(staffPrefab, spawnPoint.position, Quaternion.identity);

        StaffAgent sa = agent.GetComponent<StaffAgent>();
        if (sa != null)
        {
            HospitalRoom[] accessibleRooms = GetAllRoomsForAgent("staff");
            sa.Initialize(accessibleRooms);
        }
    }

    void SpawnVisitor()
    {
        if (visitorPrefab == null) return;
        GameObject agent = Instantiate(visitorPrefab, spawnPoint.position, Quaternion.identity);

        VisitorAgent va = agent.GetComponent<VisitorAgent>();
        if (va != null)
        {
            HospitalRoom targetRoom = GetRandomRoomForAgent("visitor");
            va.Initialize(targetRoom);
        }
    }

    HospitalRoom GetRandomRoomForAgent(string agentType)
    {
        List<HospitalRoom> validRooms = new List<HospitalRoom>();

        foreach (HospitalRoom room in allRooms)
        {
            if (room != null && room.CanAgentEnter(agentType))
            {
                validRooms.Add(room);
            }
        }

        if (validRooms.Count > 0)
            return validRooms[Random.Range(0, validRooms.Count)];

        Debug.LogWarning($"[AgentSpawner] No valid rooms found for {agentType}");
        return null;
    }

    HospitalRoom[] GetAllRoomsForAgent(string agentType)
    {
        List<HospitalRoom> validRooms = new List<HospitalRoom>();

        foreach (HospitalRoom room in allRooms)
        {
            if (room != null && room.CanAgentEnter(agentType))
            {
                validRooms.Add(room);
            }
        }

        return validRooms.ToArray();
    }
}