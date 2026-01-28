using UnityEngine;

public class StaffAgent : AgentController
{
    private HospitalRoom[] accessibleRooms;
    private int currentRoomIndex = 0;
    private float dwellTimer;

    public void Initialize(HospitalRoom[] rooms)
    {
        agentType = "staff";
        moveSpeed = 35f;
        accessibleRooms = rooms;
    }

    protected override void Start()
    {
        base.Start();
        MoveToNextRoom();
    }

    protected override void Update()
    {
        base.Update();

        if (ReachedDestination() && dwellTimer == 0)
        {
            dwellTimer = Time.time + Random.Range(5f, 10f);
        }

        if (dwellTimer > 0 && Time.time > dwellTimer)
        {
            dwellTimer = 0;
            MoveToNextRoom();
        }
    }

    void MoveToNextRoom()
    {
        if (accessibleRooms == null || accessibleRooms.Length == 0) return;

        currentRoomIndex = Random.Range(0, accessibleRooms.Length);

        if (accessibleRooms[currentRoomIndex] != null &&
            accessibleRooms[currentRoomIndex].entryPoint != null)
        {
            MoveTo(accessibleRooms[currentRoomIndex].entryPoint.position);
        }
    }
}