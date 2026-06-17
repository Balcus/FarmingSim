using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int Money = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddMoney(int amount)
    {
        Money += amount;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateMoneyText(Money);
    }

    public bool SpendMoney(int amount)
    {
        if (Money < amount)
            return false;

        Money -= amount;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateMoneyText(Money);

        return true;
    }
}
