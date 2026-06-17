using System.Collections.Generic;
using UnityEngine;

public class PlotUnlocker : MonoBehaviour
{
    public List<GameObject> lockedPlots;

    public int unlockCost = 50;

    private int nextPlotIndex = 0;

    public bool UnlockNextPlot()
    {
        if (lockedPlots == null || lockedPlots.Count == 0)
        {
            ShowMessage("No locked plots are linked.");
            return false;
        }

        if (nextPlotIndex >= lockedPlots.Count)
        {
            ShowMessage("All plots have been unlocked!");

            return false;
        }

        if (MoneyManager.Instance == null)
        {
            ShowMessage("Money system is not ready.");
            return false;
        }

        if (!MoneyManager.Instance.SpendMoney(unlockCost))
        {
            ShowMessage("Not enough money! Need $" + unlockCost);

            return false;
        }

        GameObject plot = lockedPlots[nextPlotIndex];
        if (plot != null)
            plot.SetActive(true);

        ShowMessage("Plot unlocked for $" + unlockCost);

        nextPlotIndex++;

        unlockCost += 50; // Next plot costs more

        return true;
    }

    private void ShowMessage(string message)
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowMessage(message);
        else Debug.Log(message);
    }
}
