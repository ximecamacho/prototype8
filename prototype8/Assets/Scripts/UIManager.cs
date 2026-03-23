using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GameObject hudPanel;
    private GameObject mainMenuPanel;
    private GameObject transfurPanel;
    private GameObject levelCompletePanel;
    private GameObject gameWonPanel;

    private Image interactIcon;
    private Image bottomBar;

    private Image keyIndicatorOuter;
    private Image keyIndicatorInner;
    private Image[] leverIndicatorOuter = new Image[3];
    private Image[] leverIndicatorInner = new Image[3];
    private Image safeZoneIndicator;

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

        HideAllPanels();
    }

    void BuildHUD(GameObject parent)
    {
        hudPanel = CreatePanel(parent, "HUD", Color.clear);

        var interactObj = CreateUIRect(
            hudPanel,
            "InteractIcon",
            new Vector2(0.47f, 0.12f),
            new Vector2(0.53f, 0.17f),
            new Color(1f, 1f, 1f, 0f)
        );
        interactIcon = interactObj.GetComponent<Image>();

        var barObj = CreateUIRect(
            hudPanel,
            "BottomBar",
            new Vector2(0f, 0f),
            new Vector2(1f, 0.07f),
            new Color(0.05f, 0.05f, 0.08f, 0.7f)
        );
        bottomBar = barObj.GetComponent<Image>();

        Color barBg = new Color(0.05f, 0.05f, 0.08f);

        var kOuter = CreateUIRect(
            barObj,
            "KeyOuter",
            new Vector2(0.02f, 0.1f),
            new Vector2(0.06f, 0.9f),
            new Color(1f, 0.85f, 0.2f, 0.8f)
        );
        keyIndicatorOuter = kOuter.GetComponent<Image>();
        var kInner = CreateUIRect(
            kOuter,
            "KeyInner",
            new Vector2(0.15f, 0.15f),
            new Vector2(0.85f, 0.85f),
            barBg
        );
        keyIndicatorInner = kInner.GetComponent<Image>();

        Color[] leverColors =
        {
            new Color(0.0f, 0.9f, 0.9f, 0.8f),
            new Color(0.9f, 0.0f, 0.9f, 0.8f),
            new Color(1.0f, 0.9f, 0.2f, 0.8f),
        };
        for (int i = 0; i < 3; i++)
        {
            float lx = 0.10f + i * 0.045f;
            var lOuter = CreateUIRect(
                barObj,
                $"LeverOuter{i}",
                new Vector2(lx, 0.1f),
                new Vector2(lx + 0.035f, 0.9f),
                leverColors[i]
            );
            leverIndicatorOuter[i] = lOuter.GetComponent<Image>();
            var lInner = CreateUIRect(
                lOuter,
                $"LeverInner{i}",
                new Vector2(0.15f, 0.15f),
                new Vector2(0.85f, 0.85f),
                barBg
            );
            leverIndicatorInner[i] = lInner.GetComponent<Image>();
        }

        var safeObj = CreateUIRect(
            barObj,
            "SafeIndicator",
            new Vector2(0.93f, 0.1f),
            new Vector2(0.97f, 0.9f),
            new Color(0.3f, 0.85f, 0.4f, 0f)
        );
        safeZoneIndicator = safeObj.GetComponent<Image>();
    }

    void BuildMainMenu(GameObject parent)
    {
        mainMenuPanel = CreatePanel(parent, "MainMenu", new Color(0.03f, 0.03f, 0.06f, 0.97f));

        CreateUIRect(
            mainMenuPanel,
            "MenuIcon",
            new Vector2(0.35f, 0.50f),
            new Vector2(0.65f, 0.78f),
            new Color(0.7f, 0.15f, 0.15f, 0.9f)
        );

        CreateUIRect(
            mainMenuPanel,
            "PlayTriangle",
            new Vector2(0.44f, 0.35f),
            new Vector2(0.56f, 0.45f),
            new Color(1f, 1f, 1f, 0.8f)
        );

        var startLabel = CreateUIText(
            mainMenuPanel,
            "StartLabel",
            new Vector2(0.30f, 0.27f),
            new Vector2(0.70f, 0.34f),
            "SPACE TO START",
            11,
            new Color(0.7f, 0.7f, 0.7f, 0.7f)
        );
        startLabel.alignment = TextAnchor.MiddleCenter;

        CreateUIRect(
            mainMenuPanel,
            "ArrowUp",
            new Vector2(0.27f, 0.155f),
            new Vector2(0.29f, 0.185f),
            new Color(0.6f, 0.6f, 0.6f, 0.6f)
        );
        CreateUIRect(
            mainMenuPanel,
            "ArrowDown",
            new Vector2(0.27f, 0.10f),
            new Vector2(0.29f, 0.13f),
            new Color(0.6f, 0.6f, 0.6f, 0.6f)
        );
        CreateUIRect(
            mainMenuPanel,
            "ArrowLeft",
            new Vector2(0.245f, 0.125f),
            new Vector2(0.265f, 0.155f),
            new Color(0.6f, 0.6f, 0.6f, 0.6f)
        );
        CreateUIRect(
            mainMenuPanel,
            "ArrowRight",
            new Vector2(0.295f, 0.125f),
            new Vector2(0.315f, 0.155f),
            new Color(0.6f, 0.6f, 0.6f, 0.6f)
        );
        var moveLabel = CreateUIText(
            mainMenuPanel,
            "MoveLabel",
            new Vector2(0.22f, 0.06f),
            new Vector2(0.34f, 0.10f),
            "MOVE",
            9,
            new Color(0.5f, 0.5f, 0.5f, 0.6f)
        );
        moveLabel.alignment = TextAnchor.MiddleCenter;

        CreateUIRect(
            mainMenuPanel,
            "SpaceKey",
            new Vector2(0.55f, 0.12f),
            new Vector2(0.75f, 0.155f),
            new Color(0.6f, 0.6f, 0.6f, 0.6f)
        );
        var interactLabel = CreateUIText(
            mainMenuPanel,
            "InteractLabel",
            new Vector2(0.52f, 0.06f),
            new Vector2(0.78f, 0.10f),
            "SPACE = INTERACT",
            9,
            new Color(0.5f, 0.5f, 0.5f, 0.6f)
        );
        interactLabel.alignment = TextAnchor.MiddleCenter;
    }

    Text CreateUIText(
        GameObject parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string content,
        int fontSize,
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
        var text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    void BuildTransfurScreen(GameObject parent)
    {
        transfurPanel = CreatePanel(parent, "Transfur", new Color(0.12f, 0.02f, 0.15f, 0f));
        CreateUIRect(
            transfurPanel,
            "DeathIcon",
            new Vector2(0.3f, 0.35f),
            new Vector2(0.7f, 0.75f),
            new Color(1.0f, 0.0f, 0.0f, 0.9f)
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
        UpdateHUDVisibility(level);
    }

    void UpdateHUDVisibility(int level)
    {
        bool hasKey = level < 2;
        bool hasPuzzle = level >= 2;

        if (keyIndicatorOuter != null)
            keyIndicatorOuter.gameObject.SetActive(hasKey);

        for (int i = 0; i < 3; i++)
        {
            if (leverIndicatorOuter[i] != null)
                leverIndicatorOuter[i].gameObject.SetActive(hasPuzzle);
        }
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

    public void ShowLockedMessage(string message)
    {
        VisualFeedback.Instance?.FlashScreen(new Color(1f, 0.15f, 0.15f), 0.2f);
    }

    public void UpdateInteractPrompt(bool show)
    {
        if (interactIcon == null)
            return;
        Color c = interactIcon.color;
        c.a = Mathf.Lerp(c.a, show ? 0.7f : 0f, Time.deltaTime * 8f);
        interactIcon.color = c;
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

        if (hudPanel != null && hudPanel.activeSelf)
            UpdateHUDIndicators();
    }

    void UpdateHUDIndicators()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null)
            return;
        var inv = player.GetComponent<PlayerInventory>();
        if (inv == null)
            return;

        if (
            keyIndicatorInner != null
            && keyIndicatorOuter != null
            && keyIndicatorOuter.gameObject.activeSelf
        )
        {
            bool hasKey = inv.KeyCount > 0;
            keyIndicatorInner.gameObject.SetActive(!hasKey);
        }

        var pm = FindFirstObjectByType<PuzzleManager>();
        for (int i = 0; i < 3; i++)
        {
            if (leverIndicatorInner[i] == null || leverIndicatorOuter[i] == null)
                continue;
            if (!leverIndicatorOuter[i].gameObject.activeSelf)
                continue;

            bool active = false;
            if (pm != null && pm.puzzlePieces != null && i < pm.puzzlePieces.Length)
                active = pm.puzzlePieces[i].isActivated;

            leverIndicatorInner[i].gameObject.SetActive(!active);
        }

        if (safeZoneIndicator != null)
        {
            float r = LevelGenerator.safeZoneRadius;
            Vector2 pp = player.transform.position;
            bool inSafe =
                Vector2.Distance(pp, LevelGenerator.entrancePos) < r
                || Vector2.Distance(pp, LevelGenerator.exitPos) < r;
            Color sc = safeZoneIndicator.color;
            sc.a = Mathf.Lerp(sc.a, inSafe ? 0.9f : 0f, Time.deltaTime * 6f);
            safeZoneIndicator.color = sc;
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
