using UnityEngine;

public class EmergencyAgent : AgentController
{
    private HospitalRoom traumaBay;
    private float exitTimer;

    public void Initialize(HospitalRoom trauma)
    {
        agentType = "emergency";
        moveSpeed = 45f; // Fast movement
        traumaBay = trauma;
    }

    protected override void Start()
    {
        base.Start();

        // Go DIRECTLY to Trauma Bay (skip waiting)
        if (traumaBay != null && traumaBay.entryPoints.Length > 0)
        {
            MoveTo(traumaBay.entryPoints[0].position);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (ReachedDestination() && exitTimer == 0)
        {
            // Arrived at Trauma Bay - stay briefly then exit
            exitTimer = Time.time + Random.Range(10f, 20f);
        }

        if (exitTimer > 0 && Time.time > exitTimer)
        {
            Destroy(gameObject);
        }
    }
}