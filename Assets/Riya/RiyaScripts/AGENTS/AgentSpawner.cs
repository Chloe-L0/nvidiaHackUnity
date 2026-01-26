using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [Header("Agent Prefabs")]
    public GameObject patientPrefab;
    public GameObject emergencyPrefab;
    public GameObject staffPrefab;
    public GameObject visitorPrefab;

    [Header("Spawn Settings")]
    public Transform spawnPoint; // Entrance location
    public HospitalRoom[] allRooms;

    [Header("Spawn Rates")]
    public float patientSpawnInterval = 5f;
    public float emergencySpawnInterval = 15f;
    public float visitorSpawnInterval = 8f;

    private float nextPatientSpawn;
    private float nextEmergencySpawn;
    private float nextVisitorSpawn;

    void Start()
    {
        // Spawn 2 staff immediately (they patrol continuously)
        SpawnStaff();
        SpawnStaff();
    }

    void Update()
    {
        if (Time.time > nextPatientSpawn)
        {
            SpawnPatient();
            nextPatientSpawn = Time.time + patientSpawnInterval;
        }

        if (Time.time > nextEmergencySpawn)
        {
            SpawnEmergency();
            nextEmergencySpawn = Time.time + emergencySpawnInterval;
        }

        if (Time.time > nextVisitorSpawn)
        {
            SpawnVisitor();
            nextVisitorSpawn = Time.time + visitorSpawnInterval;
        }
    }

    void SpawnPatient()
    {
        if (patientPrefab == null) return;

        GameObject agent = Instantiate(patientPrefab, spawnPoint.position, Quaternion.identity);
        HospitalRoom waiting = GetRoomByType("Waiting Area");
        HospitalRoom patient = GetRoomByType("Patient Room");

        PatientAgent patientAgent = agent.GetComponent<PatientAgent>();
        if (patientAgent != null)
        {
            patientAgent.Initialize(waiting, patient);
        }
    }

    void SpawnEmergency()
    {
        if (emergencyPrefab == null) return;

        GameObject agent = Instantiate(emergencyPrefab, spawnPoint.position, Quaternion.identity);
        HospitalRoom trauma = GetRoomByType("Trauma Bay");

        EmergencyAgent emergencyAgent = agent.GetComponent<EmergencyAgent>();
        if (emergencyAgent != null)
        {
            emergencyAgent.Initialize(trauma);
        }
    }

    void SpawnStaff()
    {
        if (staffPrefab == null) return;

        GameObject agent = Instantiate(staffPrefab, spawnPoint.position, Quaternion.identity);
        StaffAgent staffAgent = agent.GetComponent<StaffAgent>();
        if (staffAgent != null)
        {
            staffAgent.Initialize(allRooms);
        }
    }

    void SpawnVisitor()
    {
        if (visitorPrefab == null) return;

        GameObject agent = Instantiate(visitorPrefab, spawnPoint.position, Quaternion.identity);
        HospitalRoom waiting = GetRoomByType("Waiting Area");
        HospitalRoom patient = GetRoomByType("Patient Room");

        VisitorAgent visitorAgent = agent.GetComponent<VisitorAgent>();
        if (visitorAgent != null)
        {
            visitorAgent.Initialize(waiting, patient);
        }
    }

    HospitalRoom GetRoomByType(string type)
    {
        foreach (HospitalRoom room in allRooms)
        {
            if (room != null && room.roomType == type)
            {
                return room;
            }
        }
        return null;
    }
}