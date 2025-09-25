using UnityEngine;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class CreditsMenuBehavior : MonoBehaviour
{
    private Button backBtn;
    private Label creditsHeadline;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        backBtn = root.Q<Button>("backBtn");
        backBtn.SetBinding("text", new LocalizedString("MenuTranslationaTable", "backBtnText"));
        backBtn.RegisterCallback<ClickEvent>(OnBackBtnClicked);

        creditsHeadline = root.Q<Label>("headline");
        creditsHeadline.SetBinding("text", new LocalizedString("MenuTranslationaTable", "creditsHeadline"));
    }

    void OnBackBtnClicked(ClickEvent clicked)
    {
        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           // this.gameObject.SetActive(false);
        }
    }
}
