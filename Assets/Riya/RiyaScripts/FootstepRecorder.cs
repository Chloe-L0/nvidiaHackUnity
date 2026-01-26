using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct FootstepData
{
    public Vector3 position;
    public float timestamp;
    public string agentType;      // "patient", "emergency", "staff", "visitor"
    public string currentRoomID;  // Which room they're in/near
    public string previousRoomID; // Where they came from
}

[System.Serializable]
public class FootstepDataWrapper
{
    public List<FootstepData> footsteps;
}

public class FootstepRecorder : MonoBehaviour
{
    public static FootstepRecorder Instance;

    public List<FootstepData> allFootsteps = new List<FootstepData>();
    public float recordInterval = 0.5f; // Record every 0.5 seconds

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RecordFootstep(Vector3 pos, string agentType, string roomID, string prevRoom)
    {
        FootstepData step = new FootstepData
        {
            position = pos,
            timestamp = Time.time,
            agentType = agentType,
            currentRoomID = roomID,
            previousRoomID = prevRoom
        };

        allFootsteps.Add(step);
    }

    public void ExportToJSON(string filepath)
    {
        FootstepDataWrapper wrapper = new FootstepDataWrapper { footsteps = allFootsteps };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(filepath, json);
        Debug.Log($"Exported {allFootsteps.Count} footsteps to {filepath}");
    }

    void OnApplicationQuit()
    {
        // Auto-export on quit
        string path = Path.Combine(Application.dataPath, "../footsteps_export.json");
        ExportToJSON(path);
    }
}