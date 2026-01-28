using System.Collections.Generic;
using UnityEngine;

public class HospitalRoom : MonoBehaviour
{
    public enum AccessLevel
    {
        Level1_EmergencyStaffOnly = 1,
        Level2_PatientVisitorOnly = 2,
        Level3_EmergencyPatientStaff = 3,
        Level4_AllAccess = 4
    }

    [Header("Room Settings")]
    public string roomID;
    public AccessLevel accessLevel;
    public Transform entryPoint;

    private HashSet<GameObject> agentsInside = new HashSet<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Agent"))
        {
            agentsInside.Add(other.gameObject);
            AgentController agent = other.GetComponent<AgentController>();
            if (agent != null)
            {
                agent.OnEnteredRoom(this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Agent"))
        {
            agentsInside.Remove(other.gameObject);
            AgentController agent = other.GetComponent<AgentController>();
            if (agent != null)
            {
                agent.OnExitedRoom(this);
            }
        }
    }

    public bool CanAgentEnter(string agentType)
    {
        switch (accessLevel)
        {
            case AccessLevel.Level1_EmergencyStaffOnly:
                return agentType == "emergency" || agentType == "staff";

            case AccessLevel.Level2_PatientVisitorOnly:
                return agentType == "patient" || agentType == "visitor";

            case AccessLevel.Level3_EmergencyPatientStaff:
                return agentType == "emergency" || agentType == "patient" || agentType == "staff";

            case AccessLevel.Level4_AllAccess:
                return true;

            default:
                return false;
        }
    }

    public int GetOccupancy()
    {
        return agentsInside.Count;
    }
}