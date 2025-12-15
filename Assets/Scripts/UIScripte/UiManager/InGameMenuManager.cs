using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    public static InGameMenuManager Instance{ get; private set; }
    void Awake() => Instance = this;

    public List<GameObject> allMenuesInThisScene = new List<GameObject>();
    public List<GameObject> openMenus = new List<GameObject>();

    void Start()
    {
        // Ensuring that all menues are active and closed
        foreach (var menu in allMenuesInThisScene)
        {
            menu.SetActive(true);
        }
        CloseAllMenus();
        OpenOrCloseOneMenu("ActionRowDoc", true);

        //Suscribing to the Events that trigger a Menu opneing
        if (SceneManager.GetActiveScene().name != "MainMenu" && SceneManager.GetActiveScene().name != "HubScene")
        {
            TurretLevelManager.Instance.OnMilestoneReached += OpenTurretUpgradeChoice;
            PlayerManager.Instance.OnPlayerDeath += GameOver;
            EnemySpawnManager.Instance.OnBossDefeated += GameWon;
        }
    }


    void Update()
    {
        //Checks if escape was pressed, closes or opens the Pause menu, or closes the currentlxy open menu, depends
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (openMenus.Count == 0 || CheckForAnOpenMenu("StatsMenuDoc"))
            {
                CloseAllMenus();
                OpenOrCloseOneMenu("PauseMenuDoc", true);
            }
            else
            {
                EscapePressed();
            }
        }

        //Checks if Tab was pressed, closes or opens the StatsMenu
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (openMenus.Count == 0 || CheckForAnOpenMenu("PauseMenuDoc"))
            {
                CloseAllMenus();
                OpenOrCloseOneMenu("StatsMenuDoc", true);
            }
            else if (CheckForAnOpenMenu("StatsMenuDoc"))
            {
                ReturnToGame();
            }
        }
    }

    //Handles the Escape logic, just to keep the Void Update a lil cleaner and easier on the eyes
    private void EscapePressed()
    {
        if (openMenus.Last().tag == "NoEscMenu")
        {

        }
        else if (openMenus.Count == 1)
        {
            ReturnToGame();
        }
        else
        {
            OpenOrCloseOneMenu($"{openMenus.Last().name}", false);
        }
    }

    //Does what it says on the Label, this closes all registered Menus
    public void CloseAllMenus()
    {
        foreach (var menu in allMenuesInThisScene)
        {
            IMenu _menu = menu.GetComponent<IMenu>();
            if (_menu != null)
            {
                _menu.OpenOrClose(false);
                Debug.Log($"{menu.name} was closed");
            }
        }
    }

    //Also does what it says on the Label, this opens or closes one Menu. 
    //The Menu has to be registered for this to work, and mind the spelling
    public void OpenOrCloseOneMenu(string menuToOpen, Boolean openMenu)
    {
        foreach (var menu in allMenuesInThisScene)
        {
            if (menuToOpen == menu.name)
            {
                IMenu _menu = menu.GetComponent<IMenu>();
                if (_menu != null)
                {
                    _menu.OpenOrClose(openMenu);
                }
            }
        }
    }

    //Opens the Hub Menu, it*s its own Method cause i use it often
    public void ReturnToGame()
    {
        CloseAllMenus();
        OpenOrCloseOneMenu("ActionRowDoc", true);
    }

    //Checking for a specific Menu, either if it's open or if is even exists
    public Boolean CheckForAMenu(string menu)
    {
        foreach (var _menu in allMenuesInThisScene)
        {
            if (menu == _menu.name)
            {
                return true;
            }
        }
        Debug.Log("[InGameMenuManager]- CheckForAMenu: No Fitting Menu Found");
        return false;
    }
    public Boolean CheckForAnOpenMenu(string menu)
    {
        if (menu == "one" && openMenus.Count != 0)
        {
            return true;
        }
        foreach (var _menu in openMenus)
        {
            if (menu == _menu.name)
            {
                return true;
            }
        }
        Debug.Log("[InGameMenuManager]- CheckForAnOpenMenu: No Fitting Menu Found or Menu not open");
        return false;
    }

    //Methods that open one specific Menu upon receiving an Event
    void OpenTurretUpgradeChoice(TurretType type, int progressLevel)
    {
        OpenOrCloseOneMenu("TurretUpgradeChoiceDoc", true);
    }
    public void GameOver()
    {
        CloseAllMenus();
        OpenOrCloseOneMenu("GameOverDoc", true);
    }
    public void GameWon()
    {
        CloseAllMenus();
        OpenOrCloseOneMenu("GameWonDoc", true);
    }
}
