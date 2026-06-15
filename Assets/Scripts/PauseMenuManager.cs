using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }
    public bool IsPaused { get; private set; }

    private GameObject pauseCanvasRoot;

    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<PauseMenuManager>() != null) return;
        new GameObject("PauseMenuManager").AddComponent<PauseMenuManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        pauseCanvasRoot = BuildUI();
        DontDestroyOnLoad(pauseCanvasRoot);
        pauseCanvasRoot.SetActive(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      
        IsPaused = false;
        Time.timeScale = 1f;
        pauseCanvasRoot.SetActive(false);

        bool isMenu      = scene.name == "MainMenu";
        Cursor.visible   = isMenu;
        Cursor.lockState = isMenu ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause() => SetPaused(!IsPaused);
    public void Resume()      => SetPaused(false);

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Debug.Log("EXIT");
        Application.Quit();
    }

    private void SetPaused(bool paused)
    {
        IsPaused         = paused;
        Time.timeScale   = paused ? 0f : 1f;
        pauseCanvasRoot.SetActive(paused);
        Cursor.lockState = paused ? CursorLockMode.None   : CursorLockMode.Locked;
        Cursor.visible   = paused;
    }

    

    private GameObject BuildUI()
    {
        EnsureEventSystem();

        
        var canvasGO = new GameObject("PauseMenuCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

       
        var overlay    = MakeRect("Overlay", canvasGO.transform);
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.72f);
        Stretch(overlay);

        
        var panel    = MakeRect("PausePanel", canvasGO.transform);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.10f, 0.08f, 0.96f);
        Center(panel, new Vector2(380f, 340f), Vector2.zero);

        
        var titleGO  = MakeRect("Title", panel.transform);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "PAUSED";
        titleTMP.fontSize  = 44;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color     = new Color(0.82f, 1f, 0.82f);
        Center(titleGO, new Vector2(320f, 60f), new Vector2(0f, 118f));

       
        var divGO  = MakeRect("Divider", panel.transform);
        var divImg = divGO.AddComponent<Image>();
        divImg.color = new Color(0.38f, 0.72f, 0.38f, 0.70f);
        Center(divGO, new Vector2(300f, 2f), new Vector2(0f, 78f));

        
        AddButton("Resume",   "Resume",    panel.transform, new Vector2(0f,   30f), Resume);
        AddButton("MainMenu", "Main Menu", panel.transform, new Vector2(0f,  -50f), GoToMainMenu);
        AddButton("ExitGame", "Exit Game", panel.transform, new Vector2(0f, -130f), ExitGame);

        return canvasGO;
    }

    private void AddButton(string id, string label, Transform parent, Vector2 pos,
                           UnityEngine.Events.UnityAction onClick)
    {
        var go  = MakeRect(id, parent);
        var img = go.AddComponent<Image>();

        var btn    = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = new Color(0.18f, 0.22f, 0.18f);
        colors.highlightedColor = new Color(0.28f, 0.48f, 0.28f);
        colors.pressedColor     = new Color(0.14f, 0.34f, 0.14f);
        colors.selectedColor    = new Color(0.28f, 0.48f, 0.28f);
        colors.fadeDuration     = 0.1f;
        btn.colors        = colors;
        btn.targetGraphic = img;
        img.color         = colors.normalColor;
        btn.onClick.AddListener(onClick);
        btn.onClick.AddListener(() => AudioManager.Instance?.PlayButtonClick());

        Center(go, new Vector2(280f, 55f), pos);

        var labelGO  = MakeRect("Label", go.transform);
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = label;
        labelTMP.fontSize  = 22;
        labelTMP.fontStyle = FontStyles.Bold;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color     = Color.white;
        Stretch(labelGO);
    }

   

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    private static GameObject MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void Center(GameObject go, Vector2 size, Vector2 pos)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
    }

    private static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
