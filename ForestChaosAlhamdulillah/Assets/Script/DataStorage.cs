using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataStorage : MonoBehaviour
{
    public static DataStorage Instance; // Singleton instance

    private void Awake()
    {
        // Ensure only one instance of DataStorage exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene changes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string playerName; // Store the player name

    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}
