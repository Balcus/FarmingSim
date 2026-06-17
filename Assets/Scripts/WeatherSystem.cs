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

    private GameObject runtimeRainParticles;
    private Transform rainFollowTarget;
    private Material runtimeRainMaterial;

    public delegate void OnWeatherChanged(WeatherType w);
    public event OnWeatherChanged WeatherChangedEvent;

    void Awake() { Instance = this; }

    void Start()
    {
        SetupRainSystem();
        if (GameManager.Instance != null)
            GameManager.Instance.TimeChangedEvent += OnDayChanged;
        RollWeather();
    }

    void LateUpdate()
    {
        if (runtimeRainParticles != null && runtimeRainParticles.activeSelf)
            runtimeRainParticles.transform.position = GetRainCenter();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TimeChangedEvent -= OnDayChanged;

        if (runtimeRainMaterial != null)
            Destroy(runtimeRainMaterial);
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
        SetupRainSystem();

        bool isRainy = currentWeather == WeatherType.Rainy;
        if (rainParticles)
        {
            rainParticles.SetActive(isRainy);

            ParticleSystem particles = rainParticles.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                if (isRainy) particles.Play(true);
                else particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (sunLight)
        {
            sunLight.intensity = currentWeather == WeatherType.Sunny  ? 1.2f :
                                 currentWeather == WeatherType.Cloudy ? 0.6f : 0.3f;
        }
    }

    public bool IsRaining() => currentWeather == WeatherType.Rainy;

    private void FindReferencesIfMissing()
    {
        if (sunLight == null)
            sunLight = FindFirstObjectByType<Light>();

        if (rainFollowTarget == null)
            rainFollowTarget = FindRainFollowTarget();
    }

    private void SetupRainSystem()
    {
        FindReferencesIfMissing();
        DisableImportedRainMaker();

        if (runtimeRainParticles == null)
            runtimeRainParticles = CreateRuntimeRainParticles();

        if (rainParticles == null || IsImportedRainMaker(rainParticles))
            rainParticles = runtimeRainParticles;
    }

    private void DisableImportedRainMaker()
    {
        GameObject importedRainMaker = GameObject.Find("ImportedVisual_RainMaker");
        if (importedRainMaker != null && importedRainMaker != runtimeRainParticles)
            importedRainMaker.SetActive(false);
    }

    private bool IsImportedRainMaker(GameObject candidate)
    {
        if (candidate == null) return false;
        return candidate.name.Contains("RainMaker") || candidate.name == "ImportedVisual_RainMaker";
    }

    private GameObject CreateRuntimeRainParticles()
    {
        GameObject rainObject = new GameObject("FarmingSim_RainParticles");
        rainObject.transform.SetParent(transform, false);
        rainObject.transform.position = GetRainCenter();

        ParticleSystem particles = rainObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.maxParticles = 1800;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.15f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.045f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.7f, 0.86f, 1f, 0.52f),
            new Color(0.95f, 1f, 1f, 0.72f));

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 650f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(34f, 0.1f, 34f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);
        velocity.y = new ParticleSystem.MinMaxCurve(-24f, -17f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);

        ParticleSystemRenderer renderer = rainObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 1.9f;
        renderer.velocityScale = 0.12f;
        renderer.cameraVelocityScale = 0f;
        renderer.minParticleSize = 0f;
        renderer.maxParticleSize = 0.08f;

        runtimeRainMaterial = CreateRainMaterial();
        if (runtimeRainMaterial != null)
            renderer.material = runtimeRainMaterial;

        rainObject.SetActive(false);
        return rainObject;
    }

    private Material CreateRainMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) return null;

        Material material = new Material(shader);
        material.name = "Runtime_Rain_Drop_Material";
        Color color = new Color(0.82f, 0.92f, 1f, 0.62f);

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        return material;
    }

    private Vector3 GetRainCenter()
    {
        if (rainFollowTarget == null)
            rainFollowTarget = FindRainFollowTarget();

        if (rainFollowTarget != null)
            return new Vector3(rainFollowTarget.position.x, rainFollowTarget.position.y + 14f, rainFollowTarget.position.z);

        return transform.position + Vector3.up * 14f;
    }

    private Transform FindRainFollowTarget()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
            return mainCamera.transform;

        GameObject player = null;
        try
        {
            player = GameObject.FindWithTag("Player");
        }
        catch (UnityException)
        {
            player = null;
        }

        if (player == null)
            player = GameObject.Find("Player");

        return player != null ? player.transform : null;
    }
}
