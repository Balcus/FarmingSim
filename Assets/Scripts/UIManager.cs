using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Step 3.7 from the guide. Manages the on-screen Step 4 HUD.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI seasonText;
    public TextMeshProUGUI weatherText;
    public TextMeshProUGUI toolText;
    public TextMeshProUGUI waterText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI crosshairTargetText;
    private Coroutine msgCoroutine;

    [Header("Seed Packet Panel")]
    public GameObject seedPacketPanel;
    public Image seedPacketImage;
    public TextMeshProUGUI seedName;
    public TextMeshProUGUI seedMonths;
    public TextMeshProUGUI seedDays;
    public TextMeshProUGUI seedFunFact;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TimeChangedEvent += UpdateDateUI;
            UpdateDateUI(GameManager.Instance.currentDay, GameManager.Instance.currentMonth, GameManager.Instance.currentYear);
        }

        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.WeatherChangedEvent += UpdateWeatherUI;
            UpdateWeatherUI(WeatherSystem.Instance.currentWeather);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.TimeChangedEvent -= UpdateDateUI;
        if (WeatherSystem.Instance != null) WeatherSystem.Instance.WeatherChangedEvent -= UpdateWeatherUI;
    }

    private void UpdateDateUI(int d, int m, int y)
    {
        if (dateText != null) dateText.text = "Day " + d + " | Month " + m + " | Year " + y;
        if (seasonText != null && GameManager.Instance != null) seasonText.text = "Season: " + GameManager.Instance.GetSeasonName();
    }

    private void UpdateWeatherUI(WeatherType w)
    {
        if (weatherText != null) weatherText.text = "Weather: " + w;
    }

    public void UpdateToolDisplay(PlayerTool tool)
    {
        if (toolText != null) toolText.text = "Tool: " + PrettyToolName(tool);
    }

    public void UpdateWaterDisplay(int current, int max)
    {
        if (waterText != null) waterText.text = "Water: " + current + "/" + max;
    }

    public void UpdateCrosshairTarget(string label)
    {
        if (crosshairTargetText != null) crosshairTargetText.text = label;
    }

    public void ShowMessage(string msg, float duration = 2.5f)
    {
        if (messageText == null) return;
        if (msgCoroutine != null) StopCoroutine(msgCoroutine);
        msgCoroutine = StartCoroutine(DisplayMessage(msg, duration));
    }

    private IEnumerator DisplayMessage(string msg, float duration)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        messageText.gameObject.SetActive(false);
    }

    public void ShowSeedPacket(PlantData plant)
    {
        if (plant == null || seedPacketPanel == null) return;
        seedPacketPanel.SetActive(true);
        if (seedPacketImage != null && plant.seedPacketSprite != null) seedPacketImage.sprite = plant.seedPacketSprite;
        if (seedName != null) seedName.text = plant.plantName;
        if (seedMonths != null) seedMonths.text = "Plant: Month " + plant.plantMonthStart + " - " + plant.plantMonthEnd;
        if (seedDays != null) seedDays.text = "Harvest in: " + plant.growthDays + " days";
        if (seedFunFact != null) seedFunFact.text = plant.funFact;
    }

    public void HideSeedPacket()
    {
        if (seedPacketPanel != null) seedPacketPanel.SetActive(false);
    }

    private string PrettyToolName(PlayerTool tool)
    {
        if (tool == PlayerTool.None) return "Seeds";
        if (tool == PlayerTool.WateringCan) return "Watering Can";
        return tool.ToString();
    }
}
