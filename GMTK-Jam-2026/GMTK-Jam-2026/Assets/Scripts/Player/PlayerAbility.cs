using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbility : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private PlayerMovement playerMovement;

    [SerializeField] private float projectileTimer = 1f;    //1 second between projectiles
    private float projectileTimerCounter;

    private bool dashAbilityActive = true;
    private bool glideAbilityActive = true;
    private bool projectileAbilityActive = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        projectileTimerCounter += Time.deltaTime;
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && dashAbilityActive)
        {
            playerMovement.SetDash();
        }

        if(Keyboard.current.spaceKey.isPressed && 
            !playerMovement.IsGrounded() && glideAbilityActive 
            && playerMovement.IsMovingDown())
        {
            playerMovement.StartGlide();
        }
        if(projectileAbilityActive)
        {
            Console.WriteLine("Ability is active");
            ShootProjectile();
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

    public void SetProjectileAbility(bool active)
    {
        this.projectileAbilityActive = active;
    }
    private void ShootProjectile()
    {
        if (projectileTimerCounter < projectileTimer)    //Not enough time since last projectile
        {
            return;
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            ProjectileController projectile = GameObject.Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
            projectile.setVelocityDirection(Vector2.up);
            projectileTimerCounter = 0;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            ProjectileController projectile = GameObject.Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
            projectile.setVelocityDirection(Vector2.down);
            projectileTimerCounter = 0;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            ProjectileController projectile = GameObject.Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
            projectile.setVelocityDirection(Vector2.left);
            projectileTimerCounter = 0;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ProjectileController projectile = GameObject.Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
            projectile.setVelocityDirection(Vector2.right);
            projectileTimerCounter = 0;
        }
    }
}
