using UnityEngine;
using System.Collections;

/// <summary>
/// Spore libérée par une Mushroom brick détruite.
/// 1) Vol en arc jusqu'à une position cible.
/// 2) Explose au sol : endommage les bricks dans un rayon.
/// </summary>
public class SporeProjectile : MonoBehaviour
{
    [Header("Mouvement")]
    [SerializeField] private float travelDuration = 0.35f; // secondes pour atteindre la cible
    [SerializeField] private float arcHeight = 0.8f;       // hauteur maximale de l'arc

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 1.2f;
    [SerializeField] private GameObject explosionVFXPrefab;  // optionnel : particules à l'impact

    // Renseigné par BrickObject au spawn
    [HideInInspector] public Vector3 targetPosition;
    [HideInInspector] public int damage;
    [HideInInspector] public PlayerData playerData;

    private void Start()
    {
        StartCoroutine(FlyAndExplode());
    }

    private IEnumerator FlyAndExplode()
    {
        Vector3 origin = transform.position;
        float elapsed = 0f;

        while (elapsed < travelDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelDuration);

            // Interpolation linéaire XY + arc parabolique en Y
            Vector3 pos = Vector3.Lerp(origin, targetPosition, t);
            pos.y += arcHeight * Mathf.Sin(t * Mathf.PI); // sin(0..π) → bosse en milieu de trajet

            transform.position = pos;

            // Petite rotation pendant le vol
            transform.Rotate(0f, 0f, 360f * Time.deltaTime / travelDuration);

            yield return null;
        }

        transform.position = targetPosition;
        Explode();
    }

    private void Explode()
    {
        // VFX optionnel
        if (explosionVFXPrefab != null)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        // Dommages aux bricks dans le rayon
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            BrickObject brick = hit.GetComponent<BrickObject>();
            if (brick != null)
                brick.TakeExplosionDamage(damage);
        }

        Destroy(gameObject);
    }

    // Gizmo pour visualiser le rayon dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 1f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(targetPosition, explosionRadius);
    }
}