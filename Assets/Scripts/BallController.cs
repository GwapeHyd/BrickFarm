using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    public Transform paddle;
    public float launchSpeed = 8f;
    public float launchAngleRange = 45f;
    public LevelManager levelManager;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private bool canLaunch = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        AttachToPaddle();
    }

    void Update()
    {
        if (!isLaunched)
        {
            // Suivre le paddle
            if (paddle != null)
                transform.position = paddle.position + Vector3.up * 0.5f;

            // Autoriser le lancement seulement si le niveau est prêt
            if (canLaunch && Mouse.current.leftButton.wasPressedThisFrame)
            {
                LaunchBall();
            }
        }
    }

    public void AttachToPaddle()
    {
        isLaunched = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void EnableLaunch()
    {
        canLaunch = true;
    }

    void LaunchBall()
    {
        isLaunched = true;
        canLaunch = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        float angle = Random.Range(-launchAngleRange, launchAngleRange);
        Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
        rb.linearVelocity = dir * launchSpeed;
        Debug.Log($"Balle lancée direction: {dir}, vitesse: {rb.linearVelocity}");
        if (levelManager != null)
        {
            levelManager.StartLevelTimer();
        }
    }

    public void ResetBall()
    {
        AttachToPaddle();
    }
}
