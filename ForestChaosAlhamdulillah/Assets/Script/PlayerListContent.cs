using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListContent : MonoBehaviour
{
    public Text playerNameText;

    void Start()
    {
        // Access the stored player name using the DataStorage singleton
        string playerName = DataStorage.Instance.playerName;

        // Display the player name in the UI
        playerNameText.text = "Player Name: " + playerName;
    }
}

