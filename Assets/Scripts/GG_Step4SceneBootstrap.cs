using UnityEngine;

/// <summary>
/// Additive runtime helper. It does not edit the original GameManager script;
/// it only prevents the Step 4 demo from running at extreme time speed during Play Mode.
/// </summary>
public class GG_Step4SceneBootstrap : MonoBehaviour
{
    public GameObject rainParticles;
    public Light sceneSun;
    public float safeDaysPerRealSecond = 0.1f;

    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.daysPerRealSecond > 1f)
            GameManager.Instance.daysPerRealSecond = safeDaysPerRealSecond;

        if (WeatherSystem.Instance != null)
        {
            if (rainParticles != null) WeatherSystem.Instance.rainParticles = rainParticles;
            if (sceneSun != null) WeatherSystem.Instance.sunLight = sceneSun;
        }
    }
}
