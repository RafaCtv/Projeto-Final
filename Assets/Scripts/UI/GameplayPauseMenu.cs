using ProjetoFinal.Core;
using ProjetoFinal.Localization;
using ProjetoFinal.UI.Components;
using ProjetoFinal.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Read Escape before the weapon shop can close itself and release its pause.
[DefaultExecutionOrder(-10)]
public sealed class GameplayPauseMenu : MonoBehaviour
{
    [SerializeField] private GameManager game;
    [SerializeField] private GameObject menuButtonPrefab;

    private GameObject panel;
    private Button resumeButton;
    private GameObject previousSelection;
    private float previousTimeScale;

    private void Update()
    {
        if (Keyboard.current?.escapeKey.wasPressedThisFrame != true)
            return;

        if (panel != null && panel.activeSelf)
            Close();
        else if (Time.timeScale > 0f || !game.IsRunning)
            Open();
    }

    private void Open()
    {
        if (panel == null)
            CreatePanel();

        previousTimeScale = Time.timeScale;
        previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        Time.timeScale = 0f;
        panel.SetActive(true);
        resumeButton.Select();
    }

    private void Close()
    {
        if (panel == null || !panel.activeSelf)
            return;

        panel.SetActive(false);
        Time.timeScale = previousTimeScale;
        if (game != null && game.Player != null && game.Player.TryGetComponent<PlayerCombat>(out var combat))
            combat.BlockFireUntilReleased();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(previousSelection);
        previousSelection = null;
    }

    private void OnDisable() => Close();

    private void ReturnToMainMenu()
    {
        if (SceneLoader.TryLoad(SceneNames.MainMenu))
            previousTimeScale = 1f;
    }

    private void CreatePanel()
    {
        panel = new GameObject("PauseMenu", typeof(RectTransform), typeof(Image));
        panel.SetActive(false);
        var rect = (RectTransform)panel.transform;
        rect.SetParent(transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.08f, 0.92f);

        var title = new GameObject("Title", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        title.transform.SetParent(rect, false);
        Place(title.rectTransform, 174f, new Vector2(560f, 82f));
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 62f;
        title.raycastTarget = false;
        if (UITheme.Default != null)
            title.font = UITheme.Default.Font;
        title.gameObject.AddComponent<LocalizedText>().SetKey("pause.title");

        resumeButton = AddButton("pause.resume", 73f);
        resumeButton.onClick.AddListener(Close);
        var mainMenuButton = AddButton("pause.mainMenu", -29f);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        LinkButtons(resumeButton, null, mainMenuButton);
#if !UNITY_WEBGL || UNITY_EDITOR
        var quitButton = AddButton("menu.quit", -131f);
        quitButton.gameObject.AddComponent<QuitGameButton>();
        LinkButtons(mainMenuButton, resumeButton, quitButton);
        LinkButtons(quitButton, mainMenuButton, null);
#else
        LinkButtons(mainMenuButton, resumeButton, null);
#endif
    }

    private Button AddButton(string key, float verticalPosition)
    {
        var instance = Instantiate(menuButtonPrefab, panel.transform, false);
        instance.name = key;
        Place((RectTransform)instance.transform, verticalPosition, new Vector2(560f, 90f));
        var label = instance.GetComponentInChildren<LocalizedText>(true);
        label.SetKey(key);
        var text = label.GetComponent<TMP_Text>();
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = 42f;
        return instance.GetComponent<Button>();
    }

    private static void Place(RectTransform rect, float y, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = size;
    }

    private static void LinkButtons(Button button, Button above, Button below)
    {
        button.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = above,
            selectOnDown = below,
        };
    }
}
