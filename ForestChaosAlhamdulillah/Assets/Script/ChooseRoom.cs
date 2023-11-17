using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseRoom : MonoBehaviour
{
    public InputField playerNameInput;

    public void OnNextButtonClick()
    {
        string playerName = playerNameInput.text;

        // Store the player name using the DataStorage singleton
        DataStorage.Instance.SetPlayerName(playerName);

        // Load the next scene
        SceneManager.LoadScene("PlayerListScene");
    }
}
