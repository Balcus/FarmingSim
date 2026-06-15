using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class MainMenuVisuals : MonoBehaviour
{
    private static MainMenuVisuals Instance;
    private GameObject styledCanvas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        var go = new GameObject("MainMenuVisuals");
        DontDestroyOnLoad(go);
        go.AddComponent<MainMenuVisuals>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        styledCanvas = BuildStyledCanvas();
        DontDestroyOnLoad(styledCanvas);
        styledCanvas.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManager.GetActiveScene().name == "MainMenu")
            OnMainMenuLoaded();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            OnMainMenuLoaded();
        else
            styledCanvas.SetActive(false);
    }

    private void OnMainMenuLoaded()
    {
        HideOriginalCanvas();
        styledCanvas.SetActive(true);
    }

    
    private static void HideOriginalCanvas()
    {
        var canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
           
            if (c.gameObject.scene.name == "MainMenu")
                c.gameObject.SetActive(false);
        }
    }

    
    private GameObject BuildStyledCanvas()
    {
        EnsureEventSystem();

        var canvasGO = new GameObject("MainMenuStyledCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2; 

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        
        var overlay = MakeRect("Overlay", canvasGO.transform);
        overlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
        Stretch(overlay);

        
        var panel = MakeRect("Panel", canvasGO.transform);
        panel.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.08f, 0.96f);
        Center(panel, new Vector2(380f, 340f), Vector2.zero);

        
        var titleGO  = MakeRect("Title", panel.transform);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "FARMING SIM";
        titleTMP.fontSize  = 44;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color     = new Color(0.82f, 1f, 0.82f);
        Center(titleGO, new Vector2(320f, 60f), new Vector2(0f, 118f));

        
        var div = MakeRect("Divider", panel.transform);
        div.AddComponent<Image>().color = new Color(0.38f, 0.72f, 0.38f, 0.70f);
        Center(div, new Vector2(300f, 2f), new Vector2(0f, 78f));

       
        AddButton("Play",     "Play",      panel.transform, new Vector2(0f,   30f),
                  () => SceneManager.LoadScene("SampleScene"));

        AddButton("SettingsButton", "Settings", panel.transform, new Vector2(0f, -50f),
                  () => SettingsMenuManager.Instance?.Open());

        AddButton("ExitGame", "Exit Game", panel.transform, new Vector2(0f, -130f),
                  () => { Debug.Log("EXIT"); Application.Quit(); });

        return canvasGO;
    }

    private static void AddButton(string id, string label, Transform parent, Vector2 pos,
                                  System.Action onClick)
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
        btn.onClick.AddListener(() => onClick());
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
