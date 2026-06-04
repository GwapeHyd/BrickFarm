using UnityEngine;
using System.Collections;

public class BrickAnimator : MonoBehaviour
{
    public float appearTime = 0.3f;

    private SpriteRenderer sr;
    private Vector3 originalScale;

    [Header("Shake Effect")]
    [SerializeField] private float shakeAngle = 10f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeRotations = 1f;
    [SerializeField] private float shakeSpeed = 10f;

    [Header("Floating Effect")]
    [SerializeField] private float floatAmplitude = 0.05f;   // Hauteur du flottement
    [SerializeField] private float floatFrequency = 1f;      // Vitesse du flottement
    [SerializeField] private float rotationAmplitude = 1f;   // Amplitude de rotation
    [SerializeField] private float rotationSpeed = 30f;      // Vitesse de rotation

    private Vector3 startPos;
    private float angleOffset;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;
        Debug.Log($"BrickAnimator.Awake sur {gameObject.name} (parent: {transform.parent?.name})");
    }

    private void Start()
    {
        Debug.Log($"BrickAnimator.Start sur {gameObject.name} (parent: {transform.parent?.name})");
        StartCoroutine(Appear());
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatFrequency + angleOffset) * floatAmplitude;
        float zRot = Mathf.Sin(Time.time * rotationSpeed + angleOffset) * rotationAmplitude;

        transform.localPosition = startPos + new Vector3(0, yOffset, 0);
        transform.localRotation = Quaternion.Euler(0, 0, zRot);
    }

    private IEnumerator Appear()
    {
        float t = 0f;
        while (t < appearTime)
        {
            t += Time.deltaTime;
            float ratio = t / appearTime;

            // Scale
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, ratio);
            // Alpha
            Color c = sr.color;
            c.a = Mathf.Lerp(0f, 1f, ratio);
            sr.color = c;

            yield return null;
        }

        // Assure état final
        transform.localScale = originalScale;
        Color final = sr.color;
        final.a = 1f;
        sr.color = final;
    }

    public IEnumerator ShakeEffect()
    {
        float elapsed = 0f;
        Quaternion original = transform.rotation;

        // On calcule la fréquence angulaire en radians/s : 2π * nombre de rotations / durée
        float angularFrequency = 2f * Mathf.PI * shakeRotations / shakeDuration;

        while (elapsed < shakeDuration)
        {
            // Le déplacement angulaire est sinusoïdal entre -shakeAngle et +shakeAngle
            // On convertit shakeAngle en radians puis on applique sin(angle) pour faire un effet de va-et-vient
            float angleZ = Mathf.Sin(elapsed * angularFrequency) * shakeAngle;

            transform.rotation = Quaternion.Euler(0, 0, angleZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = original;
    }
}
