using UnityEngine;

public class StaffAgent : AgentController
{
    private HospitalRoom[] patrolRooms;
    private int currentRoomIndex = 0;
    private float dwellTimer;
    private bool isWaiting = false;

    public void Initialize(HospitalRoom[] rooms)
    {
        agentType = "staff";
        moveSpeed = 35f; // Fast movement
        patrolRooms = rooms;
    }

    protected override void Start()
    {
        base.Start();
        MoveToNextRoom();
    }

    protected override void Update()
    {
        base.Update();

        if (!isWaiting && ReachedDestination())
        {
            // Reached room - wait briefly then move to next
            isWaiting = true;
            dwellTimer = Time.time + Random.Range(3f, 7f);
        }

        if (isWaiting && Time.time > dwellTimer)
        {
            isWaiting = false;
            MoveToNextRoom();
        }
    }

    void MoveToNextRoom()
    {
        if (patrolRooms == null || patrolRooms.Length == 0) return;

        currentRoomIndex = (currentRoomIndex + 1) % patrolRooms.Length;

        if (patrolRooms[currentRoomIndex] != null &&
            patrolRooms[currentRoomIndex].entryPoints.Length > 0)
        {
            MoveTo(patrolRooms[currentRoomIndex].entryPoints[0].position);
        }
    }
}
