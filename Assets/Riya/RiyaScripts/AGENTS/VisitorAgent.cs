using UnityEngine;

public class VisitorAgent : AgentController
{
    private HospitalRoom targetRoom;
    private float exitTimer;

    public void Initialize(HospitalRoom room)
    {
        agentType = "visitor";
        moveSpeed = 25f;
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
            exitTimer = Time.time + Random.Range(15f, 30f);
        }

        if (exitTimer > 0 && Time.time > exitTimer)
        {
            Destroy(gameObject);
        }
    }
}