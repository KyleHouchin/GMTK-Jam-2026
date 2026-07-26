using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float playerSearchRange = 8f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float enemySpeed = 10f;
    [SerializeField] private Transform floorCheck;
    private Rigidbody2D rigidbody;
    private Transform playerPosition;
    private bool isDashing;

    private enum EnemyStates
    {
        Searching, 
        Dash, 
        Resting
    };
    private EnemyStates currentState = EnemyStates.Searching;
    private bool prepareToDash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == EnemyStates.Searching)
        {
            Debug.Log("Searching");
            SearchForPlayer();
        }
        else if(currentState == EnemyStates.Resting)
        {
            Debug.Log("Resting");
            if(!prepareToDash)
            {
                float waitTime = UnityEngine.Random.Range(2f, 4f);
                StartCoroutine(WaitTime(waitTime));
                prepareToDash = true;
            }
        }
    }
    private void FixedUpdate()
    {
        if(currentState == EnemyStates.Dash)
        {
            Debug.Log("Dash");
            if (!isDashing)
            {
                MoveTowardsPlayer();
            }
        }
    }

    private IEnumerator WaitTime(float time)
    {
        switch(currentState)
        {
            case EnemyStates.Resting:
                yield return new WaitForSeconds(time);
                currentState = EnemyStates.Dash;
                isDashing = false;
                break;
            case EnemyStates.Dash:
                float timeCounter = 0;
                while(timeCounter < time)
                {
                    timeCounter += Time.fixedDeltaTime;
                    if(!CheckForEdge())
                    {
                        break;
                    }
                    yield return new WaitForFixedUpdate();
                }
                rigidbody.linearVelocity = Vector2.zero;
                currentState = EnemyStates.Searching;
                isDashing = false;
                break;
        }
    }

    private bool CheckForEdge()
    {
        var edgeCheck = Physics2D.Raycast(floorCheck.position, Vector2.down, 1.5f, groundLayer);
        Debug.Log("Is there more ground? " + edgeCheck.collider != null);
        return edgeCheck.collider != null;
    }

    
    private void MoveTowardsPlayer()
    {
        if(transform.position.x < playerPosition.position.x)
        {
            //Enemy is to left of player, move to the right
            rigidbody.linearVelocity = Vector2.right * enemySpeed;
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            rigidbody.linearVelocity = Vector2.left * enemySpeed;
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }
        isDashing = true;
        StartCoroutine(WaitTime(2));

    }

    private void SearchForPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(this.transform.position, playerSearchRange, playerLayer);
        if(playerCollider != null )
        {
            prepareToDash = false;
            currentState = EnemyStates.Resting;
            playerPosition = playerCollider.transform;
            if (transform.position.x < playerPosition.position.x)
            {
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
