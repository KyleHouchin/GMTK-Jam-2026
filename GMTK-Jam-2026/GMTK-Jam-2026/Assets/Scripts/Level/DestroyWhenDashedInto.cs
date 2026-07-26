using UnityEngine;

public class DestroyWhenDashedInto : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && playerMovement.IsDashing)
        {
            Destroy(this.gameObject);
        }
    }
}
