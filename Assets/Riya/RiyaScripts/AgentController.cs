using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentController : MonoBehaviour
{
    [Header("Agent Identity")]
    public string agentType; // "patient", "emergency", "staff", "visitor"
    public string agentID;

    [Header("Movement")]
    public NavMeshAgent navAgent;
    public float moveSpeed = 1.0f;

    [Header("Tracking")]
    protected string currentRoomID = "hallway";
    protected string previousRoomID = "";
    private float nextRecordTime;

    protected virtual void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;
        agentID = $"{agentType}_{GetInstanceID()}";
        nextRecordTime = Time.time;

        // Tag this agent
        gameObject.tag = "Agent";
    }

    protected virtual void Update()
    {
        // Record footstep at intervals
        if (Time.time >= nextRecordTime)
        {
            if (FootstepRecorder.Instance != null)
            {
                FootstepRecorder.Instance.RecordFootstep(
                    transform.position,
                    agentType,
                    currentRoomID,
                    previousRoomID
                );
            }
            nextRecordTime = Time.time + FootstepRecorder.Instance.recordInterval;
        }
    }

    public void OnEnteredRoom(HospitalRoom room)
    {
        previousRoomID = currentRoomID;
        currentRoomID = room.roomID;
        Debug.Log($"{agentID} entered {currentRoomID}");
    }

    public void OnExitedRoom(HospitalRoom room)
    {
        previousRoomID = room.roomID;
        currentRoomID = "hallway";
    }

    public void MoveTo(Vector3 destination)
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(destination);
        }
    }

    protected bool ReachedDestination()
    {
        if (!navAgent.isOnNavMesh) return false;
        return !navAgent.pathPending && navAgent.remainingDistance < 0.5f;
    }
}