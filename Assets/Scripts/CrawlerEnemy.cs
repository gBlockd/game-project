using UnityEngine;

/// <summary>
/// Simple ground-based crawler enemy.
///
/// Behavior:
/// - moves continuously left or right along the ground,
/// - turns around when it detects a wall in front,
/// - turns around when it reaches an edge,
/// - can be temporarily frozen by knockback,
/// - uses Rigidbody2D for grounded movement,
/// - can be combined with EnemyHealth / EnemyContactDamage for combat behavior.
/// </summary>
public class CrawlerEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public bool movingRight = true;

    [Header("Detection")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.1f;
    public Transform edgeCheck;
    public float edgeCheckRadius = 0.1f;
    public LayerMask groundLayer;
    public LayerMask crawlerLayer;

    private Rigidbody2D rb;
    private bool isFrozen;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (isFrozen)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        bool wallAhead = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
        bool crawlerAhead = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, crawlerLayer);
        bool groundAhead = Physics2D.OverlapCircle(edgeCheck.position, edgeCheckRadius, groundLayer);

        if (wallAhead || crawlerAhead || !groundAhead)
        {
            TurnAround();
        }

        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    private void TurnAround()
    {
        movingRight = !movingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }

        if (edgeCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(edgeCheck.position, edgeCheckRadius);
        }
    }
}