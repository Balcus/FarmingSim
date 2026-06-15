using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuManager : MonoBehaviour
{
    public static SettingsMenuManager Instance { get; private set; }

    private GameObject settingsCanvasRoot;

    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<SettingsMenuManager>() != null) return;
        var go = new GameObject("SettingsMenuManager");
        DontDestroyOnLoad(go);
        go.AddComponent<SettingsMenuManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        settingsCanvasRoot = BuildSettingsPanel();
        DontDestroyOnLoad(settingsCanvasRoot);
        settingsCanvasRoot.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded;
        WireMainMenuButton(); 
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        settingsCanvasRoot.SetActive(false); 
        WireMainMenuButton();
    }

    
    private void WireMainMenuButton()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu") return;
        var btnGO = GameObject.Find("SettingsButton");
        if (btnGO == null) return;
        var btn = btnGO.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveListener(Open); 
        btn.onClick.AddListener(Open);
    }

    public void Open()
    {
        EnsureEventSystem();
        settingsCanvasRoot.SetActive(true);
    }

    public void Close()
    {
        settingsCanvasRoot.SetActive(false);
        AudioManager.Instance?.SaveAll(); 
    }

   

    private GameObject BuildSettingsPanel()
    {
        EnsureEventSystem();

       
        var canvasGO = new GameObject("SettingsCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

       
        var overlay = MakeRect("Overlay", canvasGO.transform);
        overlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        Stretch(overlay);

        
        var panel = MakeRect("SettingsPanel", canvasGO.transform);
        panel.AddComponent<Image>().color = new Color(0.08f, 0.10f, 0.08f, 0.96f);
        Center(panel, new Vector2(420f, 500f), Vector2.zero);

        
        var titleGO  = MakeRect("Title", panel.transform);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "SETTINGS";
        titleTMP.fontSize  = 40;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color     = new Color(0.82f, 1f, 0.82f);
        Center(titleGO, new Vector2(340f, 55f), new Vector2(0f, 205f));

        
        var div = MakeRect("Divider", panel.transform);
        div.AddComponent<Image>().color = new Color(0.38f, 0.72f, 0.38f, 0.70f);
        Center(div, new Vector2(360f, 2f), new Vector2(0f, 163f));

        
        var am     = AudioManager.Instance;
        float master = am != null ? am.MasterVolume : PlayerPrefs.GetFloat("Vol_Master", 1f);
        float music  = am != null ? am.MusicVolume  : PlayerPrefs.GetFloat("Vol_Music",  0.75f);
        float sfx    = am != null ? am.SFXVolume    : PlayerPrefs.GetFloat("Vol_SFX",    1f);

        MakeSliderRow(panel.transform, "Master", "Master Volume", master,  new Vector2(0f,  90f),
                      v => AudioManager.Instance?.SetMasterVolume(v));

        MakeSliderRow(panel.transform, "Music",  "Music Volume",  music,   new Vector2(0f,   0f),
                      v => AudioManager.Instance?.SetMusicVolume(v));

        MakeSliderRow(panel.transform, "SFX",    "SFX Volume",    sfx,     new Vector2(0f, -90f),
                      v => AudioManager.Instance?.SetSFXVolume(v));

       
        MakeButton("Back", "Back", panel.transform, new Vector2(0f, -205f), Close);

        return canvasGO;
    }

   
    private void MakeSliderRow(Transform parent, string id, string label,
                               float initial, Vector2 pos, System.Action<float> onChange)
    {
        var row = MakeRect(id + "Row", parent);
        Center(row, new Vector2(360f, 75f), pos);

        

        var nameLabelGO  = MakeRect("NameLabel", row.transform);
        var nameLabel    = nameLabelGO.AddComponent<TextMeshProUGUI>();
        nameLabel.text      = label;
        nameLabel.fontSize  = 18;
        nameLabel.fontStyle = FontStyles.Bold;
        nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
        nameLabel.color     = new Color(0.82f, 1f, 0.82f);
        var nameLabelRT = nameLabelGO.GetComponent<RectTransform>();
        nameLabelRT.anchorMin = new Vector2(0f,   0.5f);
        nameLabelRT.anchorMax = new Vector2(0.75f, 1f);
        nameLabelRT.offsetMin = Vector2.zero;
        nameLabelRT.offsetMax = Vector2.zero;

        var valueLabelGO = MakeRect("ValueLabel", row.transform);
        var valueLabel   = valueLabelGO.AddComponent<TextMeshProUGUI>();
        valueLabel.text      = Mathf.RoundToInt(initial * 100) + "%";
        valueLabel.fontSize  = 18;
        valueLabel.alignment = TextAlignmentOptions.MidlineRight;
        valueLabel.color     = new Color(0.65f, 0.90f, 0.65f);
        var valueLabelRT = valueLabelGO.GetComponent<RectTransform>();
        valueLabelRT.anchorMin = new Vector2(0.75f, 0.5f);
        valueLabelRT.anchorMax = new Vector2(1f,     1f);
        valueLabelRT.offsetMin = Vector2.zero;
        valueLabelRT.offsetMax = Vector2.zero;

      

        var sliderGO = MakeRect(id + "Slider", row.transform);
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0f, 0f);
        sliderRT.anchorMax = new Vector2(1f, 0.5f);
        sliderRT.offsetMin = Vector2.zero;
        sliderRT.offsetMax = Vector2.zero;

        
        var bgGO = MakeRect("Background", sliderGO.transform);
        bgGO.AddComponent<Image>().color = new Color(0.12f, 0.15f, 0.12f);
        Stretch(bgGO);

        
        var fillAreaGO = MakeRect("Fill Area", sliderGO.transform);
        var fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f,  0.25f);
        fillAreaRT.anchorMax = new Vector2(1f,  0.75f);
        fillAreaRT.offsetMin = new Vector2(5f,  0f);
        fillAreaRT.offsetMax = new Vector2(-12f, 0f);

        var fillGO  = MakeRect("Fill", fillAreaGO.transform);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.28f, 0.58f, 0.28f);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0f, 1f); 
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

       
        var handleAreaGO = MakeRect("Handle Slide Area", sliderGO.transform);
        var handleAreaRT = handleAreaGO.GetComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = new Vector2(8f,  0f);
        handleAreaRT.offsetMax = new Vector2(-8f, 0f);

        var handleGO  = MakeRect("Handle", handleAreaGO.transform);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = new Color(0.85f, 1f, 0.85f);
        var handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.anchorMin        = new Vector2(0f, 0f);
        handleRT.anchorMax        = new Vector2(0f, 1f);
        handleRT.sizeDelta        = new Vector2(16f, 0f);
        handleRT.anchoredPosition = Vector2.zero;

       
        var slider        = sliderGO.AddComponent<Slider>();
        slider.fillRect   = fillRT;
        slider.handleRect = handleRT;
        slider.direction  = Slider.Direction.LeftToRight;
        slider.minValue   = 0f;
        slider.maxValue   = 1f;
        slider.value      = initial;

        var colors = slider.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(0.9f,  1f,   0.9f);
        colors.pressedColor     = new Color(0.7f,  0.95f, 0.7f);
        colors.fadeDuration     = 0.05f;
        slider.colors = colors;

        slider.onValueChanged.AddListener(v =>
        {
            valueLabel.text = Mathf.RoundToInt(v * 100) + "%";
            onChange?.Invoke(v);
        });
    }

  

    private static void MakeButton(string id, string label, Transform parent,
                                   Vector2 pos, System.Action onClick)
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

        Center(go, new Vector2(220f, 55f), pos);

        var labelGO  = MakeRect("Label", go.transform);
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = label;
        labelTMP.fontSize  = 20;
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
