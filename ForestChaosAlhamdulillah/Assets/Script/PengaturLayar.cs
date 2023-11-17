using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PengaturLayar : MonoBehaviour
{
    public void LoadEnergiSuryaScene()
    {
        SceneManager.LoadScene("EnergiSuryaScene");
    }

    public void LoadJenisPanelSuryaScene()
    {
        SceneManager.LoadScene("JanisPanelSuryaScene");
    }

    public void LoadManfaatdanPotensiScene()
    {
        SceneManager.LoadScene("ManfaatdanPotensiScene");
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    
    public void LoadNextScene()
    {
        // Get the current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Load the next scene
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}

