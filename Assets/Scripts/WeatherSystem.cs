using UnityEngine;
using System.Collections;

public enum WeatherType { Sunny, Cloudy, Rainy }

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance;
    public WeatherType currentWeather = WeatherType.Sunny;

    [Header("Probabilities (must sum to 1)")]
    public float sunnyChance = 0.5f;
    public float cloudyChance = 0.3f;
    public float rainyChance  = 0.2f;

    [Header("References")]
    public GameObject rainParticles;   // assign Rain particle system
    public Light sunLight;             // assign Directional Light

    public delegate void OnWeatherChanged(WeatherType w);
    public event OnWeatherChanged WeatherChangedEvent;

    void Awake() { Instance = this; }

    void Start()
    {
        GameManager.Instance.TimeChangedEvent += OnDayChanged;
        RollWeather();
    }

    void OnDayChanged(int d, int m, int y) { RollWeather(); }

    void RollWeather()
    {
        float r = Random.value;
        if      (r < sunnyChance)                        currentWeather = WeatherType.Sunny;
        else if (r < sunnyChance + cloudyChance)         currentWeather = WeatherType.Cloudy;
        else                                             currentWeather = WeatherType.Rainy;

        ApplyWeatherEffects();
        WeatherChangedEvent?.Invoke(currentWeather);
    }

    void ApplyWeatherEffects()
    {
        if (rainParticles) rainParticles.SetActive(currentWeather == WeatherType.Rainy);
        if (sunLight)
        {
            sunLight.intensity = currentWeather == WeatherType.Sunny  ? 1.2f :
                                 currentWeather == WeatherType.Cloudy ? 0.6f : 0.3f;
        }
    }

    public bool IsRaining() => currentWeather == WeatherType.Rainy;
}
