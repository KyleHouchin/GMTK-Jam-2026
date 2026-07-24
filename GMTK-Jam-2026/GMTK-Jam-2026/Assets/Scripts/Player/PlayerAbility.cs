using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbility : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private bool dashAbilityActive = true;
    private bool glideAbilityActive = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.leftShiftKey.wasPressedThisFrame && dashAbilityActive)
        {
            playerMovement.SetDash();
        }

        if(Keyboard.current.spaceKey.isPressed && 
            !playerMovement.IsGrounded() && glideAbilityActive 
            && playerMovement.IsMovingDown())
        {
            playerMovement.StartGlide();
        }
    }

    public void SetDashAbility(bool active)
    {
        this.dashAbilityActive = active; 
    }

    public void SetGlideAbility(bool active)
    {
        this.glideAbilityActive = active;
    }
}
