using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Time Settings")]
    public int currentMonth = 3;   // Start in March (spring)
    public int currentYear  = 1;
    public float daysPerRealSecond = 100f; // speed of time
    private float dayTimer = 0f;
    public int currentDay  = 1;
    public int daysPerMonth = 30;

    public delegate void OnTimeChanged(int day, int month, int year);
    public event OnTimeChanged TimeChangedEvent;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Starting Season: " + GetSeasonName());
        Debug.Log("Starting Date: Day " + currentDay +
                  " Month " + currentMonth +
                  " Year " + currentYear);
    }

    void Update()
    {
        dayTimer += Time.deltaTime * daysPerRealSecond;

        if (dayTimer >= 1f)
        {
            dayTimer = 0f;
            AdvanceDay();
        }
    }

    void AdvanceDay()
    {
        currentDay++;

        if (currentDay > daysPerMonth)
        {
            currentDay = 1;
            currentMonth++;
        }

        if (currentMonth > 12)
        {
            currentMonth = 1;
            currentYear++;
        }

        string currentSeason = GetSeasonName();

        Debug.Log(
            "Day: " + currentDay +
            " | Month: " + currentMonth +
            " | Year: " + currentYear +
            " | Season: " + currentSeason
        );

        TimeChangedEvent?.Invoke(currentDay, currentMonth, currentYear);
    }

    public string GetSeasonName()
    {
        if (currentMonth >= 3  && currentMonth <= 5)  return "Spring";
        if (currentMonth >= 6  && currentMonth <= 8)  return "Summer";
        if (currentMonth >= 9  && currentMonth <= 11) return "Autumn";

        return "Winter";
    }
}