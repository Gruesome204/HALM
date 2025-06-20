using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject statsMenu;
    [SerializeField] private GameObject actionRow;
    [SerializeField] private GameObject settingsMenu;

    void Start()
    {
        CloseAllMenus();
        OpenOneInGameMenu(3);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale == 0)
        {
            ReturnToGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllMenus();
            OpenOneInGameMenu(1);
        }

        if (Input.GetKeyDown(KeyCode.Tab) && Time.timeScale == 0)
        {
            ReturnToGame();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            CloseAllMenus();
            OpenOneInGameMenu(2);
        }
    }

    public void CloseAllMenus()
    {
        pauseMenu.SetActive(false);
        statsMenu.SetActive(false);
        actionRow.SetActive(false);
        settingsMenu.SetActive(false);
    }

    public void OpenOneInGameMenu(int menuToOpen)
    {
        switch (menuToOpen)
        {
            case 1:
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
                break;

            case 2:
                statsMenu.SetActive(true);
                Time.timeScale = 0;
                break;

            case 3:
                actionRow.SetActive(true);
                break;

            case 4:
                settingsMenu.SetActive(true);
                break;
        }
    }

    public void ReturnToGame()
    {
        Time.timeScale = 1;
        CloseAllMenus();
        OpenOneInGameMenu(3);
    }
}
