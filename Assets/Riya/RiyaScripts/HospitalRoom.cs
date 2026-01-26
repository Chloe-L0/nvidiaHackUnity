using System.Collections.Generic;
using UnityEngine;

public class HospitalRoom : MonoBehaviour
{
    public string roomID;        // "Trauma_Bay_0", "Medical_Lab_0", etc.
    public string roomType;      // "Trauma Bay", "Medical Lab", "Waiting Area", "Patient Room"
    public Transform[] entryPoints; // Where agents enter this room

    // For tracking agents inside
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

    public int GetOccupancy()
    {
        return agentsInside.Count;
    }
}