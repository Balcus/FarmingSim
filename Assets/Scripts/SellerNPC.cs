using UnityEngine;
using System.Collections.Generic;

public class SellerNPC : MonoBehaviour
{
    public float interactDistance = 3f;

    public void SellAll()
    {
        Debug.Log("InventoryManager = " + InventoryManager.Instance);
        var inventory =
            InventoryManager.Instance.GetInventory();

        int totalMoney = 0;

        foreach (var item in new Dictionary<string, int>(inventory))
        {
            string vegetable = item.Key;
            int amount = item.Value;

            if (amount <= 0)
                continue;

            PlantData plant =
                PlantDatabase.Instance.GetPlant(vegetable);

            totalMoney += amount * plant.sellPrice;

            InventoryManager.Instance.ClearVegetable(vegetable);
        }

        MoneyManager.Instance.AddMoney(totalMoney);

        UIManager.Instance.UpdateInventoryUI();

        UIManager.Instance.ShowMessage(
            "Sold everything for $" +
            totalMoney);
    }
}