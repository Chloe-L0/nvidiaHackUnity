using UnityEngine;

public class EmergencyAgent : AgentController
{
    private HospitalRoom targetRoom;
    private float exitTimer;

    public void Initialize(HospitalRoom room)
    {
        agentType = "emergency";
        moveSpeed = 45f;
        targetRoom = room;
    }

    protected override void Start()
    {
        base.Start();

        if (targetRoom != null && targetRoom.entryPoint != null)
        {
            MoveTo(targetRoom.entryPoint.position);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (ReachedDestination() && exitTimer == 0)
        {
            exitTimer = Time.time + Random.Range(10f, 20f);
        }

        if (exitTimer > 0 && Time.time > exitTimer)
        {
            Destroy(gameObject);
        }
    }
}