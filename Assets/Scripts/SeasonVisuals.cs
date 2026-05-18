using UnityEngine;

public class SeasonVisuals : MonoBehaviour
{
    [Header("Terrain")]
    public Terrain terrain;

    [Header("Winter Material")]
    public Material White;

    private Material originalMaterial;
    private string lastSeason = "";

    void Start()
    {
        if (terrain != null)
        {
            // Save the terrain's current/original material
            originalMaterial = terrain.materialTemplate;
        }

        GameManager.Instance.TimeChangedEvent += OnDayChanged;

        UpdateSeason(GameManager.Instance.GetSeasonName());
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TimeChangedEvent -= OnDayChanged;
    }

    void OnDayChanged(int d, int m, int y)
    {
        string season = GameManager.Instance.GetSeasonName();

        if (season != lastSeason)
            UpdateSeason(season);
    }

    void UpdateSeason(string season)
    {
        lastSeason = season;

        if (terrain == null)
            return;

        if (season == "Winter")
        {
            ApplyTerrainMaterial(White);
        }
        else
        {
            // Keep the original terrain material
            ApplyTerrainMaterial(originalMaterial);
        }
    }

    void ApplyTerrainMaterial(Material mat)
    {
        if (mat == null)
            return;

        terrain.materialTemplate = mat;
    }
}