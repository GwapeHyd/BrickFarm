using UnityEngine;
using System.Collections;

public enum BrickType { Bush, Dirt, Mushroom}
public class BrickObject : MonoBehaviour
{
    public PlayerData playerData;
    public BrickType brickType;
    public BrickData data;

    private UpgradeManager upgradeManager;
    private SpriteRenderer spriteRenderer;

    public int gridX;
    public int gridY;

    [Header("Stats")]
    private int health;
    private int maxHealth;

    [Header("Loots")]
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private GameObject mushPrefab;

    [Header("VFX")]
    [SerializeField] private GameObject dustEffectPrefab;
    [SerializeField] private GameObject popupPrefab;

    [Header("Spore Explosion")]
    [SerializeField] private GameObject sporePrefab;        // prefab avec SporeProjectile
    [SerializeField] private float sporeSprayRadius = 2f;   // rayon de dispersion des cibles

    [Header("Invincibilité au spawn")]
    [SerializeField] private float spawnInvincibilityDuration = 0.5f;
    private bool isInvincible = true;

    private Color originalColor;

    private BrickAnimator animator;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<BrickAnimator>();
        upgradeManager = FindAnyObjectByType<UpgradeManager>();
        if (data != null && data.icon != null)
        {
            spriteRenderer.sprite = data.icon;
            maxHealth = data.maxHP;
            health = maxHealth;
        }

        originalColor = spriteRenderer.color;

        StartCoroutine(SpawnInvincibility());
    }

    private IEnumerator SpawnInvincibility()
    {
        isInvincible = true;

        
        float appearTime = 0f;
        BrickAnimator brickAnim = GetComponentInChildren<BrickAnimator>();
        if (brickAnim != null)
            appearTime = brickAnim.appearTime;

        yield return new WaitForSeconds(appearTime + 0.05f);

        
        Color original = spriteRenderer.color;
        Color ghost    = new Color(original.r, original.g, original.b, 0.35f);

        float elapsed   = 0f;
        float blinkRate = 0.12f;
        float blinkDuration = spawnInvincibilityDuration;

        while (elapsed < blinkDuration)
        {
            spriteRenderer.color = (Mathf.FloorToInt(elapsed / blinkRate) % 2 == 0) ? ghost : original;
            elapsed += Time.deltaTime;
            yield return null;
        }

    
        spriteRenderer.color = original;
        isInvincible = false;
    }

    void UpdateSprite()
    {
        if (data == null) return;

        if (data.damagedSprite != null && health <= maxHealth)
        {
            spriteRenderer.sprite = data.damagedSprite;
        }
        else
        {
            spriteRenderer.sprite = data.icon;
        }
    }

    void HandleHit(int damage)
    {
        if (isInvincible) return;

        if (animator != null)
        {
            animator.StartCoroutine(animator.ShakeEffect());
        }

        health -= damage;
        UpdateSprite();

        if (health <= 0)
        {
            switch (brickType)
            {
                case BrickType.Bush:
                    SpawnLeaf();
                    break;
                case BrickType.Mushroom:
                    SpawnMush();
                    TrySporeExplosion();
                    break;
                // Ajouter d'autres types de briques et leurs loots ici
            }
            SpawnDustEffect();
            Destroy(gameObject);
        }
    }

    public void TakeExplosionDamage(int damage)
    {
        HandleHit(damage);
    }

    void TrySporeExplosion()
    {
        if (upgradeManager == null || sporePrefab == null) return;

        float chance = upgradeManager.GetSporeExplosionChance();
        if (chance <= 0f || Random.value > chance) return;

        int count = upgradeManager.GetSporeCount();
        int dmg = playerData != null
            ? playerData.damage * (int)playerData.damageMultiplier
            : 1;

        for (int i = 0; i < count; i++)
        {
            // Position cible aléatoire dans le rayon de spray
            Vector2 offset = Random.insideUnitCircle * sporeSprayRadius;
            Vector3 target = transform.position + new Vector3(offset.x, offset.y, 0f);

            GameObject sporeGO = Instantiate(sporePrefab, transform.position, Quaternion.identity);
            SporeProjectile spore = sporeGO.GetComponent<SporeProjectile>();
            if (spore != null)
            {
                spore.targetPosition = target;
                spore.damage = dmg;
                spore.playerData = playerData;
            }
        }
    }

    void SpawnDustEffect()
    {
        if (dustEffectPrefab != null)
        {
            Instantiate(dustEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    void SpawnLeaf()
    {
        if (leafPrefab != null)
        {
            var go = Instantiate(leafPrefab, transform.position + Vector3.down * 0.2f, Quaternion.identity);
            AutoCollect autoCollect = go.GetComponent<AutoCollect>();
            if (autoCollect != null)
            {
                autoCollect.playerData = playerData;
                autoCollect.currencyType = CurrencyType.leaf;
            }
        }
    }

    void SpawnMush()
    {
        if (mushPrefab != null)
        {
            var go = Instantiate(mushPrefab, transform.position + Vector3.down * 0.2f, Quaternion.identity);
            AutoCollect autoCollect = go.GetComponent<AutoCollect>();
            if (autoCollect != null)
            {
                autoCollect.playerData = playerData;
                autoCollect.currencyType = CurrencyType.mush;
            }
        }
    }

    void ShowFloatingPopup(string text, Vector3 worldPos, Color color)
    {
        if (popupPrefab == null) return;

        GameObject popup = Instantiate(popupPrefab, worldPos, Quaternion.identity);
        popup.GetComponent<FloatingPopup>().Init(text, color);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ball")) return;

        Ball ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null)
        {
            int damage = playerData.damage * (int)playerData.damageMultiplier;
            HandleHit(damage);
        }
    }
}