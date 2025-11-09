using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameMenuManager : MonoBehaviour
{
    public static InGameMenuManager Instance{ get; private set; }
    void Awake() => Instance = this;

    [SerializeField] private GameObject PauseMenuDoc;
    [SerializeField] private GameObject StatsMenuDoc;
    [SerializeField] private GameObject ActionRowDoc;
    [SerializeField] private GameObject SettingsMenuDoc;
    [SerializeField] private GameObject TurretUpgradeChoiceDoc;
    [SerializeField] private GameObject GameOverDoc;
    [SerializeField] private GameObject GameWonDoc;


    public List<GameObject> openMenus = new List<GameObject>();


    void OnDisable() => Debug.Log($"{name} was disabled!");
    void Start()
    {
        //CloseAllMenus();
        ActionRowDoc.SetActive(true);
        OpenCloseOneMenu("ActionRowDoc", true);
        TurretLevelManager.Instance.OnMilestoneReached += OpenTurretUpgradeChoice;

        PlayerManager.Instance.OnPlayerDeath += GameOver;
        EnemySpawnManager.Instance.OnAllEnemiesDefeated += GameWon;

    }



void OpenTurretUpgradeChoice(TurretType type, int progressLevel)
    {
        OpenCloseOneMenu("TurretUpgradeChoiceDoc", true);
        TurretUpgradeChoiceDoc.GetComponent<TurretUpgradeMenuBehavior>().CreateListEntry(type, progressLevel);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (openMenus.Count == 0 || openMenus.Contains(StatsMenuDoc))
            {
                CloseAllMenus();
                OpenCloseOneMenu("PauseMenuDoc", true);
            }
            else
            {
                EscapePressed();
            }
        }



        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (openMenus.Count == 0 || openMenus.Contains(PauseMenuDoc))
            {
                CloseAllMenus();
                OpenCloseOneMenu("StatsMenuDoc", true);
            }
            else if (openMenus.Contains(StatsMenuDoc))
            {
                ReturnToGame();
            }
        }
    }

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
            OpenCloseOneMenu($"{openMenus.Last().name}", false);
        }
    }

    public void CloseAllMenus()
    {
        OpenCloseOneMenu("PauseMenuDoc", false);
        OpenCloseOneMenu("StatsMenuDoc", false);
        OpenCloseOneMenu("ActionRowDoc", false);
        OpenCloseOneMenu("SettingsMenuDoc", false);
        OpenCloseOneMenu("TurretUpgradeChoiceDoc", false);
        OpenCloseOneMenu("GameOverDoc", false);
        OpenCloseOneMenu("GameWonDoc", false);
    }

    public void OpenCloseOneMenu(string menuToOpen, Boolean openMenu)
    {
        switch (menuToOpen)
        {
            case "PauseMenuDoc":
                if (openMenu)
                {
                    var root = PauseMenuDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").RemoveFromClassList("pauseMenuSlideOut");
                    openMenus.Add(PauseMenuDoc);
                    GameManager.Instance.ChangeState(GameManager.GameState.Paused);
                }
                else
                {
                    var root = PauseMenuDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").AddToClassList("pauseMenuSlideOut");
                    openMenus.Remove(PauseMenuDoc);
                    GameManager.Instance.ChangeState(GameManager.GameState.Playing);
                }
                break;
                 
            case "StatsMenuDoc":
                if (openMenu)
                {
                    var root = StatsMenuDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").RemoveFromClassList("pauseMenuSlideOut");
                    openMenus.Add(StatsMenuDoc);
                    GameManager.Instance.ChangeState(GameManager.GameState.Paused);
                }
                else
                {
                    var root = StatsMenuDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").AddToClassList("pauseMenuSlideOut");
                    openMenus.Remove(StatsMenuDoc);
                    GameManager.Instance.ChangeState(GameManager.GameState.Playing);
                }
                break;

            case "ActionRowDoc":
                if (openMenu)
                {
                    var root = ActionRowDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").RemoveFromClassList("actionRowSlideOut");
                }
                else
                {
                    var root = ActionRowDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").AddToClassList("actionRowSlideOut");
                }
                break;

            case "SettingsMenuDoc":
                if (openMenu)
                {
                    var root = SettingsMenuDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").RemoveFromClassList("settingsMenuSlideOut");
                    openMenus.Add(SettingsMenuDoc);
                }
                else
                {
                    var root = SettingsMenuDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").AddToClassList("settingsMenuSlideOut");
                    openMenus.Remove(SettingsMenuDoc);
                }
                break;

            case "TurretUpgradeChoiceDoc":
                if (openMenu)
                {
                    var root = TurretUpgradeChoiceDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").RemoveFromClassList("turretChoiceMenuSlideOut");
                    openMenus.Add(TurretUpgradeChoiceDoc);
                    GameManager.Instance.ChangeState(GameManager.GameState.Paused);
                }
                else
                {
                    var root = TurretUpgradeChoiceDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").AddToClassList("turretChoiceMenuSlideOut");
                    openMenus.Remove(TurretUpgradeChoiceDoc);
                    GameManager.Instance.ChangeState(GameManager.GameState.Playing);
                }
                break;

            case "GameOverDoc":
                if (openMenu)
                {
                    var root = GameOverDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").RemoveFromClassList("settingsMenuSlideOut");
                    GameManager.Instance.ChangeState(GameManager.GameState.Paused);
                    openMenus.Add(GameOverDoc);
                }
                else
                {
                    var root = GameOverDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").AddToClassList("settingsMenuSlideOut");
                    GameManager.Instance.ChangeState(GameManager.GameState.Playing);
                    openMenus.Remove(GameOverDoc);
                }
                break;

            case "GameWonDoc":
                if (openMenu)
                {
                    var root = GameWonDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").RemoveFromClassList("settingsMenuSlideOut");
                    GameManager.Instance.ChangeState(GameManager.GameState.Paused);
                    openMenus.Add(GameWonDoc);
                }
                else
                {
                    var root = GameWonDoc.GetComponent<UIDocument>().rootVisualElement;
                    root.Q<VisualElement>("mainContainer").AddToClassList("settingsMenuSlideOut");
                    GameManager.Instance.ChangeState(GameManager.GameState.Playing);
                    openMenus.Remove(GameWonDoc);
                }
                break;
        }
    }

    public void ReturnToGame()
    {
        CloseAllMenus();
        OpenCloseOneMenu("ActionRowDoc", true);
    }

    public void GameOver()
    {
        CloseAllMenus();
        OpenCloseOneMenu("GameOverDoc", true);
    }

    public void GameWon()
    {
        CloseAllMenus();
        OpenCloseOneMenu("GameWonDoc", true);
    }
}
