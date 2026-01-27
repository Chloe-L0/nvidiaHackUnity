using UnityEngine;

public class PatientAgent : AgentController
{
    private enum State { ToWaiting, Waiting, ToPatientRoom, InRoom, Exiting }
    private State currentState = State.ToWaiting;

    private HospitalRoom waitingArea;
    private HospitalRoom patientRoom;
    private float stateTimer;

    public void Initialize(HospitalRoom waiting, HospitalRoom patient)
    {
        agentType = "patient";
        moveSpeed = 25f; // Changed from 0.8 - normal walking speed
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
                    stateTimer = Time.time + Random.Range(5f, 10f); // Wait 5-10 seconds
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
                    currentState = State.InRoom;
                    stateTimer = Time.time + Random.Range(30f, 60f); // Stay 30-60 seconds
                }
                break;

            case State.InRoom:
                if (Time.time > stateTimer)
                {
                    Destroy(gameObject); // Exit simulation
                }
                break;
        }
    }
}
