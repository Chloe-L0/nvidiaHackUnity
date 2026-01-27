using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct FootstepData
{
    public Vector3 position;
    public float timestamp;
    public string agentType;
    public string currentRoomID;
    public string previousRoomID;
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
    public float recordInterval = 0.5f;

    [HideInInspector]
    public List<FootstepData> savedFootsteps = new List<FootstepData>();

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
        string path = Path.Combine(Application.dataPath, "../footsteps_export.json");
        ExportToJSON(path);
        SaveFootstepsForExport();
    }

    [ContextMenu("Save Footsteps for Export")]
    public void SaveFootstepsForExport()
    {
        savedFootsteps = new List<FootstepData>(allFootsteps);
        Debug.Log($"Saved {savedFootsteps.Count} footsteps for USD export");
    }

    [ContextMenu("Load Footsteps from JSON")]
    public void LoadFootstepsFromJSON()
    {
        string path = Path.Combine(Application.dataPath, "../footsteps_export.json");

        if (!File.Exists(path))
        {
            Debug.LogError("No JSON file found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        FootstepDataWrapper wrapper = JsonUtility.FromJson<FootstepDataWrapper>(json);

        savedFootsteps = wrapper.footsteps;
        Debug.Log($"Loaded {savedFootsteps.Count} footsteps from JSON for USD export");
    }

    public List<FootstepData> GetFootstepsForExport()
    {
        if (Application.isPlaying && allFootsteps.Count > 0)
        {
            return allFootsteps;
        }
        else
        {
            return savedFootsteps;
        }
    }
}