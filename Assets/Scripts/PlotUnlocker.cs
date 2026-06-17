using System.Collections.Generic;
using UnityEngine;

public class PlotUnlocker : MonoBehaviour
{
    public List<GameObject> lockedPlots;

    public int unlockCost = 50;

    private int nextPlotIndex = 0;

    public bool UnlockNextPlot()
    {
        if (nextPlotIndex >= lockedPlots.Count)
        {
            UIManager.Instance.ShowMessage(
                "All plots have been unlocked!");

            return false;
        }

        if (!MoneyManager.Instance.SpendMoney(unlockCost))
        {
            UIManager.Instance.ShowMessage(
                "Not enough money! Need $" + unlockCost);

            return false;
        }

        lockedPlots[nextPlotIndex].SetActive(true);

        UIManager.Instance.ShowMessage(
            "Plot unlocked for $" + unlockCost);

        nextPlotIndex++;

        unlockCost += 50; // Next plot costs more

        return true;
    }
}