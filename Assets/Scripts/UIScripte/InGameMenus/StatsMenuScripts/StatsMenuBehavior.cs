using UnityEngine;
using UnityEngine.UIElements;

public class StatsMenuBehavior : MonoBehaviour
{
    private InGameMenuManager gameMenuManger;

    private Button resumeButton;
    private Button charackterButton;
    private Button relicsButton;
    private Button towerButton;

    private VisualElement lowerButtonContainer;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        gameMenuManger = FindObjectOfType<InGameMenuManager>();


    }

    void OnResumeBtnClicked(ClickEvent evt)
    {
        gameMenuManger.ReturnToGame();
    }
    void OnCharackterBtnClicked(ClickEvent evt)
    {

    }
    void OnRelicsBtnClicked(ClickEvent evt)
    {

    }
    void OnTowerBtnClicked(ClickEvent evt)
    {

    }
}
