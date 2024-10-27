using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionMenu : MonoBehaviour
{
    public GameObject optionsScreen;

    private bool isOptionsOpen = false;

    void Update()
    {
        // Check if the ESC key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle the options menu
            if (isOptionsOpen)
            {
                CloseOptions();
            }
            else
            {
                OpenOptions();
            }
        }
    }

    public void OpenOptions()
    {
        optionsScreen.SetActive(true);
        isOptionsOpen = true; // Update the state
    }

    public void CloseOptions()
    {
        optionsScreen.SetActive(false);
        isOptionsOpen = false; // Update the state
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
