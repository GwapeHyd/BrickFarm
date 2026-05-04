using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isLaunched = false;
    public bool isControllable = false;

    [Header("Vitesse")]
    public float startSpeed = 5f;

    [Header("Effets Sonores")]
    public AudioClip brickClip;
    public AudioClip unbreakableClip;
    public AudioClip enemyClip;
    private AudioSource audioSource;

    [Header("Effets Visuels")]
    public ParticleSystem impactEffect;
    public float squashDuration = 0.1f;
    public Vector2 squashScale = new Vector2(1.2f, 0.8f);
    private Vector3 originalScale;
    private TrailRenderer trail;

    [Header("Aiming")]
    public float aimAngleRange = 60f; // amplitude totale (ex: 60° -> -30° à +30°)
    public float aimSpeed = 2f;       // vitesse d'oscillation
    public float aimLength = 2f;      // longueur de la ligne
    public LineRenderer aimLine;      // assigné dans l'inspecteur ou via code

    [Header("AutoAim")]
    public UpgradeManager upgradeManager;
    [SerializeField] private float autoAimRandomOffSet = 8f;

    private BrickObject currentAutoAimTarget;

    private float aimTimer = 0f;      // temps pour l'oscillation
    private Vector2 aimDirection = Vector2.up; // direction actuelle de visée
    private float lastCorrectionTime = -1f;
    private float correctionCooldown = 0.05f;
    private Vector2 savedVelocity;
    private bool isPaused = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        trail = GetComponent<TrailRenderer>();
        originalScale = transform.localScale;
        if (trail != null)
        {
            trail.emitting = false; // Désactiver le trail au départ
        }

        if (aimLine == null)
        {
            GameObject lineObj = new GameObject("AimLine");
            aimLine = lineObj.AddComponent<LineRenderer>();
            aimLine.startWidth = 0.05f;
            aimLine.endWidth = 0.03f;
            aimLine.positionCount = 2;
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
            aimLine.startColor = Color.white;
            aimLine.endColor = Color.white;
        }
    }

    private void Start()
    {
        // Initialement la balle n'est pas lancée
        StopBall();

        if (originalScale == Vector3.zero)
        {
            originalScale = Vector3.one;
        }
    }

    private void Update()
    {
        if (!isControllable)
        {
            return;
        }

        if (!isLaunched && isControllable)
        {
            aimTimer += Time.deltaTime * aimSpeed;

            // Oscillation dans la plage d'angle
            float angle = Mathf.Sin(aimTimer) * (aimAngleRange / 2f);
            aimDirection = Quaternion.Euler(0, 0, angle) * Vector2.up;

            if (aimLine != null)
            {
                aimLine.SetPosition(0, transform.position);
                aimLine.SetPosition(1, transform.position + (Vector3)(aimDirection.normalized * aimLength));
            }

            Launch(aimDirection);
        }
    }

    private void FixedUpdate()
    {
        if (!isLaunched || !isControllable || isPaused)
        {
            return;
        }

        Vector2 velocity = rb.linearVelocity;

        // Si la balle est arrêtée (bug possible), on la relance dans une direction aléatoire
        if (velocity.magnitude < 0.1f)
        {
            Debug.LogWarning("Vélocité trop faible, relance de sécurité.");
            velocity = new Vector2(Random.Range(-1f, 1f), 1f).normalized * startSpeed;
            rb.linearVelocity = velocity;
            return;
        }

        // Normalisation et correction automatique
        rb.linearVelocity = velocity.normalized * startSpeed;

        // Correction des rebonds trop plats ou trop verticaux
        if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0.3f * Mathf.Sign(rb.linearVelocity.y != 0 ? rb.linearVelocity.y : 1f)).normalized * startSpeed;
            Debug.Log("Correction de l'angle de rebond (trop horizontal)");
        }

        if (Mathf.Abs(rb.linearVelocity.x) < 0.1f)
        {
            rb.linearVelocity = new Vector2(0.3f * Mathf.Sign(rb.linearVelocity.x != 0 ? rb.linearVelocity.x : 1f), rb.linearVelocity.y).normalized * startSpeed;
            Debug.Log("Correction de l'angle de rebond (trop vertical)");
        }

        transform.rotation = Quaternion.identity;

    }

    public void Launch(Vector2 direction)
    {
        direction.Normalize();
        rb.linearVelocity = direction * startSpeed;
        isLaunched = true;
        if (trail != null)
        {
            trail.emitting = true;
        }

        Debug.Log("Ball launched in direction: " + direction);
    }

    public void StopBall()
    {
        rb.linearVelocity = Vector2.zero;
        isLaunched = false;
        if (trail != null)
        {
            trail.emitting = false;
        }
    }

    public void PauseBall()
    {
        if (!isPaused && isLaunched)
        {
            savedVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            isPaused = true;
            if (trail != null)
                trail.emitting = false;
        }
    }

    public void ResumeBall()
    {
        if (isPaused)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = savedVelocity;
            isPaused = false;

            if (trail != null)
                trail.emitting = true;
        }
    }

    private void ApplyAutoAim()
    {
        if (upgradeManager == null || !upgradeManager.IsAutoAimUnlocked())
        {
            return;
        }

        currentAutoAimTarget = FindNearestBrick();
        if (currentAutoAimTarget == null) return;

        Vector2 dir = (currentAutoAimTarget.transform.position - transform.position).normalized;

        float offset = Random.Range(-autoAimRandomOffSet, autoAimRandomOffSet);
        dir = (Vector2)(Quaternion.Euler(0, 0, offset) * dir);

        rb.linearVelocity = dir.normalized * startSpeed;
    }

    private BrickObject FindNearestBrick()
    {
        BrickObject[] bricks = FindObjectsByType<BrickObject>();
        BrickObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var brick in bricks)
        {
            float d = Vector2.Distance(transform.position, brick.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = brick;
            }
        }
        return nearest;
    }

    private void CorrectBounceAngle()
    {
        Vector2 vel = rb.linearVelocity;
        float angle = Vector2.Angle(vel, Vector2.up); // angle entre la vélocité et le haut

        // Corrige uniquement si angle proche de 0° (trop vertical) ou proche de 90° (trop horizontal)
        if (angle < 10f || angle > 80f)
        {
            vel = Quaternion.Euler(0, 0, Random.Range(-10f, 10f)) * vel; // petite déviation
            rb.linearVelocity = vel.normalized * startSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched || !isControllable)
        {
            return;
        }

        // On corrige l'angle uniquement si la balle est lancée
        if (isLaunched && Time.time - lastCorrectionTime > correctionCooldown)
        {
            CorrectBounceAngle();
            lastCorrectionTime = Time.time;
        }

        // Effet de squash
        StartCoroutine(SquashAndStretch(collision));

        // Effet sonore
        if (collision.gameObject.CompareTag("Brick"))
        {
            if (brickClip) audioSource.PlayOneShot(brickClip);
        }
        else if (collision.gameObject.CompareTag("Unbreakable") || collision.gameObject.CompareTag("Walls") || collision.gameObject.CompareTag("Paddle"))
        {
            if (unbreakableClip) audioSource.PlayOneShot(unbreakableClip);
            ApplyAutoAim();
        }

        // Effet visuel d'impact
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator SquashAndStretch(Collision2D collision)
    {
        // On récupère la normale pour savoir l'axe de collision
        Vector2 normal = collision.GetContact(0).normal;
        Vector3 newScale = originalScale;

        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
        {
            // Collision horizontale → squash horizontal
            newScale = new Vector3(squashScale.x, squashScale.y, 1f);
        }
        else
        {
            // Collision verticale → squash vertical
            newScale = new Vector3(squashScale.y, squashScale.x, 1f);
        }

        transform.localScale = newScale;
        yield return new WaitForSeconds(squashDuration);
        transform.localScale = originalScale;
    }

    private void OnDrawGizmos()
{
    if (currentAutoAimTarget == null) return;

    Vector3 ballPos   = transform.position;
    Vector3 targetPos = currentAutoAimTarget.transform.position;

    // Croix sur la brick ciblée
    Gizmos.color = Color.red;
    float s = 0.15f;
    Gizmos.DrawLine(targetPos + Vector3.left  * s, targetPos + Vector3.right * s);
    Gizmos.DrawLine(targetPos + Vector3.up    * s, targetPos + Vector3.down  * s);

    // Cercle autour de la cible
    DrawGizmoCircle(targetPos, 0.3f, Color.red);

    // Ligne balle → cible
    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(ballPos, targetPos);

    // Direction effective de la vélocité (en vert)
    if (Application.isPlaying && rb != null && rb.linearVelocity.magnitude > 0.1f)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(ballPos, ballPos + (Vector3)rb.linearVelocity.normalized * 1.5f);
    }
}

// Helper : cercle en gizmo (Unity n'en a pas nativement en 2D)
private void DrawGizmoCircle(Vector3 center, float radius, Color color)
{
    Gizmos.color = color;
    int segments = 20;
    float angleStep = 360f / segments;
    Vector3 prev = center + new Vector3(radius, 0f, 0f);
    for (int i = 1; i <= segments; i++)
    {
        float angle = i * angleStep * Mathf.Deg2Rad;
        Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        Gizmos.DrawLine(prev, next);
        prev = next;
    }
}

}
