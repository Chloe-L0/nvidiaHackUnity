using UnityEngine;

public class TestSingleAgent : MonoBehaviour
{
    public GameObject patientPrefab;
    public HospitalRoom waitingRoom;
    public HospitalRoom patientRoom;
    public Transform spawnPoint;

    void Start()
    {
        GameObject agent = Instantiate(patientPrefab, spawnPoint.position, Quaternion.identity);
        PatientAgent patientAgent = agent.GetComponent<PatientAgent>();
        if (patientAgent != null)
        {
            patientAgent.Initialize(waitingRoom, patientRoom);
            Debug.Log("Test patient spawned!");
        }
    }
}