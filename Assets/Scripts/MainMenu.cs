using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject menuScreen;
    [SerializeField] GameObject settingsScreen;

    public void Menu_Play() 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void Menu_Settings() 
    {
        menuScreen.SetActive(false);
        settingsScreen.SetActive(true);
    }

    public void Menu_Quit() 
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
