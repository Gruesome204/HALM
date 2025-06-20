using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject creditsMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnterMainMenu();
    }

    void TurnOffAll()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);  
        creditsMenu.SetActive(false);
    }

    public void EnterMainMenu()
    {
        TurnOffAll();
        mainMenu.SetActive(true);
    }
    public void EnterSettingsMenu()
    {
        settingsMenu.SetActive(true);
    }
    public void EnterCreditsMenu()
    {
        creditsMenu.SetActive(true);
    }
}
