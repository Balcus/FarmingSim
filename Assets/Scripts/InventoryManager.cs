using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private Dictionary<string, int> vegetables =
        new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [RuntimeInitializeOnLoadMethod]
    static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("InventoryManager");
            obj.AddComponent<InventoryManager>();
        }
    }

    public void AddVegetable(string veggieName, int amount)
    {
        if (string.IsNullOrEmpty(veggieName) || amount <= 0)
            return;

        if (!vegetables.ContainsKey(veggieName))
            vegetables.Add(veggieName, 0);

        vegetables[veggieName] += amount;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateInventoryUI();
    }

    public int GetAmount(string veggieName)
    {
        if (vegetables.ContainsKey(veggieName))
            return vegetables[veggieName];

        return 0;
    }

    public Dictionary<string, int> GetInventory()
    {
        return vegetables;
    }

    public void ClearVegetable(string veggieName)
    {
        if (vegetables.ContainsKey(veggieName))
            vegetables[veggieName] = 0;
    }
}
