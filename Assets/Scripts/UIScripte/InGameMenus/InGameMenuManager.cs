using System;
using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{
    public static InGameMenuManager Instance{ get; private set; }
    void Awake() => Instance = this;

    public event Action testEvent;

    private void OnEnable()
    {
        testEvent?.Invoke();
    }

    private void OnDisable()
    {
        testEvent?.Invoke();
    }

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject statsMenu;
    [SerializeField] private GameObject actionRow;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject turretUpgradeMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject gameWonMenu;



    void Start()
    {
        //CloseAllMenus();
        OpenOneInGameMenu(3);
        TurretLevelManager.Instance.OnMilestoneReached += OpenTurretUpgradeChoice;

        PlayerManager.Instance.OnPlayerDeath += GameOver;
    }

    void OpenTurretUpgradeChoice(TurretType type, int progressLevel)
    {
        OpenOneInGameMenu(5);
        turretUpgradeMenu.GetComponent<TurretUpgradeMenuBehavior>().CreateListEntry(type, progressLevel);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeInHierarchy)
        {
            pauseMenu.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!turretUpgradeMenu.activeInHierarchy)
            {
                CloseAllMenus();
                OpenOneInGameMenu(1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && statsMenu.activeInHierarchy)
        {
            statsMenu.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!turretUpgradeMenu.activeInHierarchy)
            {
                CloseAllMenus();
                OpenOneInGameMenu(2);
            }
        }
    }

    public void CloseAllMenus()
    {
        pauseMenu.SetActive(false);
        statsMenu.SetActive(false);
        actionRow.SetActive(false);
        settingsMenu.SetActive(false);
        turretUpgradeMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        gameWonMenu.SetActive(false);
    }

    public void OpenOneInGameMenu(int menuToOpen)
    {
        switch (menuToOpen)
        {
            case 1:
                pauseMenu.SetActive(true);
                break;
                 
            case 2:
                statsMenu.SetActive(true);
                break;

            case 3:
                actionRow.SetActive(true);
                break;

            case 4:
                settingsMenu.SetActive(true);
                break;

            case 5:
                turretUpgradeMenu.SetActive(true);
                break;

            case 6:
                gameOverMenu.SetActive(true);
                break;

            case 7:
                gameWonMenu.SetActive(true);
                break;
        }
    }

    public void ReturnToGame()
    {
        //CloseAllMenus();
        OpenOneInGameMenu(3);
    }

    public void CloseTurretUpgrade()
    {
        turretUpgradeMenu.SetActive(false);
    }

    public void GameOver()
    {
        CloseAllMenus();
        OpenOneInGameMenu(6);
    }

    public void GameWon()
    {
        CloseAllMenus();
        OpenOneInGameMenu(7);
    }
}
