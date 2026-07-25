using UnityEngine;

public class PlayerLoadout : MonoBehaviour
{
    [Header("Run Resources")]
    [SerializeField] private int startingLifeForce;

    [Header("Selected Abilities")]
    [SerializeField] private bool hasBatRush;
    [SerializeField] private bool hasGlide;
    [SerializeField] private bool hasBloodShot;

    public int StartingLifeForce => startingLifeForce;
    public bool HasBatRush => hasBatRush;
    public bool HasGlide => hasGlide;
    public bool HasBloodShot => hasBloodShot;

    public void ConfigureLoadout(
        int lifeForce,
        bool batRushSelected,
        bool glideSelected,
        bool bloodShotSelected)
    {
        startingLifeForce = Mathf.Max(0, lifeForce);

        hasBatRush = batRushSelected;
        hasGlide = glideSelected;
        hasBloodShot = bloodShotSelected;
    }

    public void ClearLoadout()
    {
        startingLifeForce = 0;

        hasBatRush = false;
        hasGlide = false;
        hasBloodShot = false;
    }
}