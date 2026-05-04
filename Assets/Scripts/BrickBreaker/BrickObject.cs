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
    [SerializeField] private int sporeCount = 3;            // nombre de spores lancées
    [SerializeField] private float sporeSprayRadius = 2f;   // rayon de dispersion des cibles

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

        int dmg = playerData != null
            ? playerData.damage * (int)playerData.damageMultiplier
            : 1;

        for (int i = 0; i < sporeCount; i++)
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