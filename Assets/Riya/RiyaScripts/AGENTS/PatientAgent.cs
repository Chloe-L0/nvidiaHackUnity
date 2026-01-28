using UnityEngine;

public class PatientAgent : AgentController
{
    private HospitalRoom targetRoom;
    private float exitTimer;

    public void Initialize(HospitalRoom room)
    {
        agentType = "patient";
        moveSpeed = 25f;
        targetRoom = room;
    }

    protected override void Start()
    {
        base.Start();

        Debug.Log($"[PatientAgent] Initialized. Target room: {(targetRoom != null ? targetRoom.roomID : "NULL")}");

        if (targetRoom != null && targetRoom.entryPoint != null)
        {
            Debug.Log($"[PatientAgent] Moving to {targetRoom.entryPoint.position}");
            MoveTo(targetRoom.entryPoint.position);
        }
        else
        {
            Debug.LogError("[PatientAgent] No valid target room or entry point!");
        }
    }

    protected override void Update()
    {
        base.Update();

        if (ReachedDestination() && exitTimer == 0)
        {
            exitTimer = Time.time + Random.Range(20f, 40f);
        }

        if (exitTimer > 0 && Time.time > exitTimer)
        {
            Destroy(gameObject);
        }
    }
}