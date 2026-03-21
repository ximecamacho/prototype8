using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GameObject hudPanel;
    private GameObject mainMenuPanel;
    private GameObject transfurPanel;
    private GameObject levelCompletePanel;
    private GameObject gameWonPanel;
    private GameObject symbolPuzzlePanel;

    private Image keyIcon;
    private Image interactIcon;

    private Door currentSymbolDoor;
    private int[] currentSymbolInput;
    private int symbolInputIndex;
    private Image[] symbolSlots;
    private Image[] symbolChoices;

    private Canvas canvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        BuildUI();
        SubscribeToEvents();
        ShowMainMenu();
    }

    void SubscribeToEvents()
    {
        var gm = GameManager.Instance;
        if (gm == null)
            return;
        gm.OnLevelChanged += OnLevelChanged;
        gm.OnTranfur += ShowTransfurScreen;
        gm.OnLevelComplete += ShowLevelComplete;
        gm.OnGameWon += ShowGameWon;
    }

    void OnDestroy()
    {
        var gm = GameManager.Instance;
        if (gm == null)
            return;
        gm.OnLevelChanged -= OnLevelChanged;
        gm.OnTranfur -= ShowTransfurScreen;
        gm.OnLevelComplete -= ShowLevelComplete;
        gm.OnGameWon -= ShowGameWon;
    }

    void BuildUI()
    {
        var canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(544, 416);
        canvasObj.AddComponent<GraphicRaycaster>();

        BuildHUD(canvasObj);
        BuildMainMenu(canvasObj);
        BuildTransfurScreen(canvasObj);
        BuildLevelComplete(canvasObj);
        BuildGameWon(canvasObj);
        BuildSymbolPuzzle(canvasObj);

        HideAllPanels();
    }

    void BuildHUD(GameObject parent)
    {
        hudPanel = CreatePanel(parent, "HUD", Color.clear);

        var keyObj = CreateUIRect(
            hudPanel,
            "KeyIcon",
            new Vector2(0.03f, 0.03f),
            new Vector2(0.08f, 0.08f),
            new Color(1f, 0.85f, 0.2f, 0f)
        );
        keyIcon = keyObj.GetComponent<Image>();

        var interactObj = CreateUIRect(
            hudPanel,
            "InteractIcon",
            new Vector2(0.47f, 0.12f),
            new Vector2(0.53f, 0.17f),
            new Color(1f, 1f, 1f, 0f)
        );
        interactIcon = interactObj.GetComponent<Image>();
    }

    void BuildMainMenu(GameObject parent)
    {
        mainMenuPanel = CreatePanel(parent, "MainMenu", new Color(0.03f, 0.03f, 0.06f, 0.97f));

        CreateUIRect(
            mainMenuPanel,
            "MenuIcon",
            new Vector2(0.35f, 0.45f),
            new Vector2(0.65f, 0.75f),
            new Color(0.7f, 0.15f, 0.15f, 0.9f)
        );

        CreateUIRect(
            mainMenuPanel,
            "PlayTriangle",
            new Vector2(0.44f, 0.25f),
            new Vector2(0.56f, 0.38f),
            new Color(1f, 1f, 1f, 0.8f)
        );

        CreateUIRect(
            mainMenuPanel,
            "ArrowUp",
            new Vector2(0.27f, 0.12f),
            new Vector2(0.29f, 0.15f),
            new Color(0.5f, 0.5f, 0.5f, 0.5f)
        );
        CreateUIRect(
            mainMenuPanel,
            "ArrowDown",
            new Vector2(0.27f, 0.06f),
            new Vector2(0.29f, 0.09f),
            new Color(0.5f, 0.5f, 0.5f, 0.5f)
        );
        CreateUIRect(
            mainMenuPanel,
            "ArrowLeft",
            new Vector2(0.24f, 0.09f),
            new Vector2(0.26f, 0.12f),
            new Color(0.5f, 0.5f, 0.5f, 0.5f)
        );
        CreateUIRect(
            mainMenuPanel,
            "ArrowRight",
            new Vector2(0.30f, 0.09f),
            new Vector2(0.32f, 0.12f),
            new Color(0.5f, 0.5f, 0.5f, 0.5f)
        );

        CreateUIRect(
            mainMenuPanel,
            "SpaceHint",
            new Vector2(0.42f, 0.08f),
            new Vector2(0.58f, 0.12f),
            new Color(0.5f, 0.5f, 0.5f, 0.5f)
        );

        CreateUIRect(
            mainMenuPanel,
            "ShiftHint",
            new Vector2(0.66f, 0.08f),
            new Vector2(0.76f, 0.12f),
            new Color(0.5f, 0.5f, 0.5f, 0.5f)
        );
    }

    void BuildTransfurScreen(GameObject parent)
    {
        transfurPanel = CreatePanel(parent, "Transfur", new Color(0.12f, 0.02f, 0.15f, 0f));

        CreateUIRect(
            transfurPanel,
            "DeathIcon",
            new Vector2(0.3f, 0.35f),
            new Vector2(0.7f, 0.75f),
            new Color(0.5f, 0.1f, 0.6f, 0.9f)
        );

        CreateUIRect(
            transfurPanel,
            "RetryIcon",
            new Vector2(0.44f, 0.15f),
            new Vector2(0.56f, 0.25f),
            new Color(1f, 1f, 1f, 0.7f)
        );
    }

    void BuildLevelComplete(GameObject parent)
    {
        levelCompletePanel = CreatePanel(
            parent,
            "LevelComplete",
            new Color(0.02f, 0.08f, 0.02f, 0.9f)
        );

        CreateUIRect(
            levelCompletePanel,
            "CheckIcon",
            new Vector2(0.35f, 0.4f),
            new Vector2(0.65f, 0.7f),
            new Color(0.2f, 0.85f, 0.3f, 0.9f)
        );

        for (int i = 0; i < 5; i++)
        {
            float x = 0.35f + i * 0.07f;
            CreateUIRect(
                levelCompletePanel,
                $"LevelDot{i}",
                new Vector2(x, 0.28f),
                new Vector2(x + 0.04f, 0.32f),
                new Color(0.5f, 0.5f, 0.5f, 0.5f)
            );
        }

        CreateUIRect(
            levelCompletePanel,
            "ContinueArrow",
            new Vector2(0.44f, 0.13f),
            new Vector2(0.56f, 0.22f),
            new Color(1f, 1f, 1f, 0.7f)
        );
    }

    void BuildGameWon(GameObject parent)
    {
        gameWonPanel = CreatePanel(parent, "GameWon", new Color(0.02f, 0.02f, 0.05f, 0.95f));

        CreateUIRect(
            gameWonPanel,
            "WonIcon",
            new Vector2(0.3f, 0.4f),
            new Vector2(0.7f, 0.75f),
            new Color(1f, 0.85f, 0.3f, 0.9f)
        );

        CreateUIRect(
            gameWonPanel,
            "WonRetry",
            new Vector2(0.44f, 0.13f),
            new Vector2(0.56f, 0.22f),
            new Color(1f, 1f, 1f, 0.7f)
        );
    }

    void BuildSymbolPuzzle(GameObject parent)
    {
        symbolPuzzlePanel = CreatePanel(
            parent,
            "SymbolPuzzle",
            new Color(0.05f, 0.05f, 0.1f, 0.92f)
        );

        symbolSlots = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            float x = 0.3f + i * 0.1f;
            var slot = CreateUIRect(
                symbolPuzzlePanel,
                $"Slot{i}",
                new Vector2(x, 0.65f),
                new Vector2(x + 0.08f, 0.75f),
                new Color(0.2f, 0.2f, 0.3f, 0.8f)
            );
            symbolSlots[i] = slot.GetComponent<Image>();
        }

        symbolChoices = new Image[6];
        for (int i = 0; i < 6; i++)
        {
            float x = 0.15f + i * 0.12f;
            var choice = CreateUIRect(
                symbolPuzzlePanel,
                $"Choice{i}",
                new Vector2(x, 0.35f),
                new Vector2(x + 0.09f, 0.5f),
                new Color(0.6f, 0.6f, 0.7f, 0.8f)
            );
            symbolChoices[i] = choice.GetComponent<Image>();
        }
    }

    GameObject CreatePanel(GameObject parent, string name, Color bgColor)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var img = panel.AddComponent<Image>();
        img.color = bgColor;
        img.raycastTarget = bgColor.a > 0;
        return panel;
    }

    GameObject CreateUIRect(
        GameObject parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color
    )
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    void HideAllPanels()
    {
        hudPanel?.SetActive(false);
        mainMenuPanel?.SetActive(false);
        transfurPanel?.SetActive(false);
        levelCompletePanel?.SetActive(false);
        gameWonPanel?.SetActive(false);
        symbolPuzzlePanel?.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    void OnLevelChanged(int level)
    {
        HideAllPanels();
        hudPanel.SetActive(true);
        UpdateLevelDots(level);
    }

    void ShowTransfurScreen()
    {
        HideAllPanels();
        transfurPanel.SetActive(true);
        StartCoroutine(FadePanel(transfurPanel, 0f, 0.95f, 1f));
    }

    void ShowLevelComplete()
    {
        HideAllPanels();
        levelCompletePanel.SetActive(true);
        UpdateLevelDots(GameManager.Instance.currentLevel);
    }

    void ShowGameWon()
    {
        HideAllPanels();
        gameWonPanel.SetActive(true);
    }

    void UpdateLevelDots(int currentLevel)
    {
        if (levelCompletePanel == null)
            return;
        for (int i = 0; i < 5; i++)
        {
            var dot = levelCompletePanel.transform.Find($"LevelDot{i}")?.GetComponent<Image>();
            if (dot != null)
            {
                dot.color =
                    i <= currentLevel
                        ? new Color(0.3f, 0.9f, 0.3f, 0.9f)
                        : new Color(0.3f, 0.3f, 0.3f, 0.4f);
            }
        }
    }

    public void ShowHintIllumination(Color glowColor, Sprite icon)
    {
        VisualFeedback.Instance?.FlashScreen(glowColor, 0.3f);
        if (GameManager.Instance != null)
            GameManager.Instance.PauseForHint();
        StartCoroutine(AutoResumeHint(1.5f));
    }

    IEnumerator AutoResumeHint(float delay)
    {
        float elapsed = 0;
        while (elapsed < delay)
        {
            elapsed += Time.unscaledDeltaTime;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
                break;
            yield return null;
        }
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeFromHint();
    }

    public void ShowSymbolPuzzle(Door door)
    {
        currentSymbolDoor = door;
        currentSymbolInput = new int[4];
        symbolInputIndex = 0;
        symbolPuzzlePanel.SetActive(true);
        GameManager.Instance.isPaused = true;

        for (int i = 0; i < symbolSlots.Length; i++)
            symbolSlots[i].color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.SetCanMove(false);
    }

    void HideSymbolPuzzle()
    {
        symbolPuzzlePanel.SetActive(false);
        currentSymbolDoor = null;
        GameManager.Instance.isPaused = false;
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.SetCanMove(true);
    }

    public void UpdateInteractPrompt(bool show)
    {
        if (interactIcon == null)
            return;
        Color c = interactIcon.color;
        c.a = Mathf.Lerp(c.a, show ? 0.7f : 0f, Time.deltaTime * 8f);
        interactIcon.color = c;
    }

    public void UpdateKeyIndicator(bool hasKey)
    {
        if (keyIcon != null)
        {
            Color c = keyIcon.color;
            c.a = hasKey ? 0.9f : 0f;
            keyIcon.color = c;
        }
    }

    void Update()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            var play = mainMenuPanel.transform.Find("PlayTriangle")?.GetComponent<Image>();
            if (play != null)
            {
                float pulse = Mathf.Sin(Time.time * 2f) * 0.15f + 0.7f;
                play.color = new Color(1f, 1f, 1f, pulse);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                GameManager.Instance.StartGame();
        }

        if (transfurPanel != null && transfurPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
                GameManager.Instance.RestartGame();
            if (Input.GetKeyDown(KeyCode.Escape))
                ShowMainMenu();
        }

        if (levelCompletePanel != null && levelCompletePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                GameManager.Instance.AdvanceToNextLevel();
        }

        if (gameWonPanel != null && gameWonPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
                GameManager.Instance.RestartGame();
            if (Input.GetKeyDown(KeyCode.Escape))
                ShowMainMenu();
        }

        if (symbolPuzzlePanel != null && symbolPuzzlePanel.activeSelf)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    if (symbolInputIndex < 4)
                    {
                        currentSymbolInput[symbolInputIndex] = i;
                        if (symbolSlots[symbolInputIndex] != null)
                            symbolSlots[symbolInputIndex].color = new Color(0.6f, 0.8f, 1f, 0.9f);
                        symbolInputIndex++;

                        if (symbolInputIndex >= 4)
                        {
                            currentSymbolDoor?.TrySymbolSequence(currentSymbolInput);
                            HideSymbolPuzzle();
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
                HideSymbolPuzzle();
        }

        if (hudPanel != null && hudPanel.activeSelf)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                var inv = player.GetComponent<PlayerInventory>();
                if (inv != null)
                    UpdateKeyIndicator(inv.KeyCount > 0);
            }
        }
    }

    IEnumerator FadePanel(GameObject panel, float from, float to, float duration)
    {
        var img = panel.GetComponent<Image>();
        if (img == null)
            yield break;
        float elapsed = 0;
        Color c = img.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            img.color = c;
            yield return null;
        }
    }
}
