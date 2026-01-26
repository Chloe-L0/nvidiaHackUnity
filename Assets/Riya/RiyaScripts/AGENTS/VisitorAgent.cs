using UnityEngine;

public class VisitorAgent : AgentController
{
    private enum State { ToWaiting, Waiting, ToPatientRoom, Visiting, Exiting }
    private State currentState = State.ToWaiting;

    private HospitalRoom waitingArea;
    private HospitalRoom patientRoom;
    private float stateTimer;

    public void Initialize(HospitalRoom waiting, HospitalRoom patient)
    {
        agentType = "visitor";
        moveSpeed = 18f; // Normal movement
        waitingArea = waiting;
        patientRoom = patient;
    }

    protected override void Start()
    {
        base.Start();

        // Start: Go to waiting area
        if (waitingArea != null && waitingArea.entryPoints.Length > 0)
        {
            MoveTo(waitingArea.entryPoints[0].position);
            currentState = State.ToWaiting;
        }
    }

    protected override void Update()
    {
        base.Update();

        switch (currentState)
        {
            case State.ToWaiting:
                if (ReachedDestination())
                {
                    currentState = State.Waiting;
                    stateTimer = Time.time + Random.Range(3f, 8f); // Wait 3-8 seconds
                }
                break;

            case State.Waiting:
                if (Time.time > stateTimer)
                {
                    if (patientRoom != null && patientRoom.entryPoints.Length > 0)
                    {
                        MoveTo(patientRoom.entryPoints[0].position);
                        currentState = State.ToPatientRoom;
                    }
                }
                break;

            case State.ToPatientRoom:
                if (ReachedDestination())
                {
                    currentState = State.Visiting;
                    stateTimer = Time.time + Random.Range(20f, 40f); // Visit 20-40 seconds
                }
                break;

            case State.Visiting:
                if (Time.time > stateTimer)
                {
                    Destroy(gameObject); // Exit simulation
                }
                break;
        }
    }
}