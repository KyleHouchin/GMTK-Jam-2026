using UnityEngine;

public class DestroyWhenDashedInto : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerMovement.IsDashing)
        {
            if (SoundEffectsManager.Instance != null)
            {
                SoundEffectsManager.Instance
                    .PlayBoxDestructionSound();
            }

            Destroy(this.gameObject);
        }
    }
}