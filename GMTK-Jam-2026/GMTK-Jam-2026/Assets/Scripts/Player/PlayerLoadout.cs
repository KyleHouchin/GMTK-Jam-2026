using UnityEngine;

public class PlayerLoadout : MonoBehaviour
{
    [Header("Run Resources")]
    [SerializeField] private int startingLifeForce;

    [Header("Selected Abilities")]
    [SerializeField] private bool hasBatRush;
    [SerializeField] private bool hasWingedLeap;
    [SerializeField] private bool hasBloodShot;

    public int StartingLifeForce => startingLifeForce;
    public bool HasBatRush => hasBatRush;
    public bool HasWingedLeap => hasWingedLeap;
    public bool HasBloodShot => hasBloodShot;

    public void ConfigureLoadout(
        int lifeForce,
        bool batRushSelected,
        bool wingedLeapSelected,
        bool bloodShotSelected)
    {
        startingLifeForce = Mathf.Max(0, lifeForce);

        hasBatRush = batRushSelected;
        hasWingedLeap = wingedLeapSelected;
        hasBloodShot = bloodShotSelected;
    }

    public void ClearLoadout()
    {
        startingLifeForce = 0;

        hasBatRush = false;
        hasWingedLeap = false;
        hasBloodShot = false;
    }
}