using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int Money = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddMoney(int amount)
    {
        Money += amount;

        UIManager.Instance.UpdateMoneyText(Money);
    }

    public bool SpendMoney(int amount)
    {
        if (Money < amount)
            return false;

        Money -= amount;

        UIManager.Instance.UpdateMoneyText(Money);

        return true;
    }
}