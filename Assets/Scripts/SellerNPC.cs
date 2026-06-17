using UnityEngine;
using System.Collections.Generic;

public class SellerNPC : MonoBehaviour
{
    public float interactDistance = 3f;
    public PlotUnlocker plotUnlocker;

    public void SellAll()
    {
        if (InventoryManager.Instance == null)
        {
            ShowMessage("Inventory is not ready.");
            return;
        }

        var inventory = InventoryManager.Instance.GetInventory();
        if (inventory == null)
        {
            ShowMessage("Inventory is empty.");
            return;
        }

        int totalMoney = 0;

        foreach (var item in new Dictionary<string, int>(inventory))
        {
            string vegetable = item.Key;
            int amount = item.Value;

            if (amount <= 0)
                continue;

            PlantData plant = PlantDatabase.Instance != null ? PlantDatabase.Instance.GetPlant(vegetable) : null;
            if (plant == null)
            {
                Debug.LogWarning("SellerNPC could not find PlantData for " + vegetable);
                continue;
            }

            totalMoney += amount * plant.sellPrice;

            InventoryManager.Instance.ClearVegetable(vegetable);
        }

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(totalMoney);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateInventoryUI();

        ShowMessage("Sold everything for $" + totalMoney);
    }

    public void BuyPlot()
    {
        if (plotUnlocker == null)
        {
            ShowMessage("Plot unlocker is not linked.");
            return;
        }

        if (plotUnlocker.UnlockNextPlot())
        {
            ShowMessage("New plot unlocked!");
        }
        else
        {
            ShowMessage("Cannot unlock plot.");
        }
    }

    private void ShowMessage(string message)
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowMessage(message);
        else Debug.Log(message);
    }
}
