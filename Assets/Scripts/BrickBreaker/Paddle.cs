using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class Paddle : MonoBehaviour
{
    [Header("Déplacement")]
    public bool isControllable = true;
    public float moveSpeed = 4f;
    public float minX = -8f, maxX = 8f; // limites de déplacement du paddle

    [Header("Mode Auto")]
    public bool autoMode = false;
    public float autoSpeed = 3f;
    private int autoDirection = 1;

    [Header("Ball")]
    private Ball ball;
    public Transform ballSpawnPoint;

    [Header("Components")]
    private SpriteRenderer spriteRenderer;

    public float maxBounceAngle = 75f;


    [Header("Effets Sonores")]
    public AudioClip coinClip;
    public AudioSource audioSource;

    private UpgradeManager upgradeManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
        upgradeManager = FindAnyObjectByType<UpgradeManager>();
    }
    void Update()
    {
        if (upgradeManager != null && upgradeManager.IsAutoAimUnlocked())
            AutoMove();
        else
        {
            if (!isControllable)
            {
                return;
            }
            else
            {
                FollowMouse();
            }
        }

        WatchBall();
    }

    public void ToggleAutoMode()
    {
        autoMode = !autoMode;
        Debug.Log($"Mode auto : {(autoMode ? "ON" : "OFF")}");
    }

    void AutoMove()
    {
        Vector3 pos = transform.position;
        pos.x += autoDirection * autoSpeed * Time.deltaTime;

        // Rebondit sur les limites
        if (pos.x >= maxX)
        {
            pos.x = maxX;
            autoDirection = -1;
        }
        else if (pos.x <= minX)
        {
            pos.x = minX;
            autoDirection = 1;
        }

        transform.position = pos;
    }

    void FollowMouse()
    {
        if (isControllable)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            worldPosition.z = 0f;
             // Assurez-vous que la position Z est 0 pour un jeu 2D
            float targetX = Mathf.Clamp(worldPosition.x, minX, maxX);

            // Mouvement lissé
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * moveSpeed);
            transform.position = pos;
        }
    }

    void WatchBall()
    {
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        if (balls.Length == 0)
            return;

        GameObject plusProche = null;
        float distanceMin = Mathf.Infinity;

        foreach (GameObject balle in balls)
        {
            float dist = Vector2.Distance(transform.position, balle.transform.position);
            if (dist < distanceMin)
            {
                distanceMin = dist;
                plusProche = balle;
            }
        }

        if (plusProche != null)
        {
            float direction = plusProche.transform.position.x - transform.position.x;
            spriteRenderer.flipX = direction < 0f;
        }
    }

    private void BounceBall(Collision2D collision)
    {
        Vector3 paddlePos = transform.position;
        Vector3 contactPoint = collision.GetContact(0).point;

        float paddleWidth = GetComponent<BoxCollider2D>().size.x * transform.localScale.x;

        // Calcul distance du point d'impact au centre du paddle (-0.5 à +0.5)
        float relativeHitPoint = (contactPoint.x - paddlePos.x) / paddleWidth;

        // Calcule l'angle en fonction de la distance au centre
        float bounceAngle = relativeHitPoint * maxBounceAngle;

        // Convertir angle en direction 2D (x,y)
        float bounceAngleRad = bounceAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Sin(bounceAngleRad), Mathf.Cos(bounceAngleRad));

        // Forcer la balle à repartir vers le haut (y positif)
        if (direction.y < 0) direction.y = -direction.y;

        // Lancer la balle dans la nouvelle direction avec sa vitesse actuelle
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        float speed = ballRb.linearVelocity.magnitude;

        if (Mathf.Abs(relativeHitPoint) > 0.4f)
        {
            speed *= 1.05f; // légère accélération si rebond très excentré
        }
        ballRb.linearVelocity = direction.normalized * speed;
    }

    private void PlayCollectSound(string tag)
    {
        AudioClip clipToPlay = null;

        switch (tag)
        {
            case "Coin":
                clipToPlay = coinClip;
                break;
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == ball.gameObject)
        {
            BounceBall(collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayCollectSound(other.tag);
    }

}
