using UnityEngine;
using System.Collections;

public enum BrickType { Bush, Dirt, Mushroom}
public class BrickObject : MonoBehaviour
{
    public PlayerData playerData;
    public BrickType brickType;
    public BrickData data;
    private SpriteRenderer spriteRenderer;

    public int gridX;
    public int gridY;

    [Header("Stats")]
    private int health;
    private int maxHealth;

    [Header("Loots")]
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private GameObject mushroomPrefab;

    [Header("VFX")]
    [SerializeField] private GameObject dustEffectPrefab;
    [SerializeField] private GameObject popupPrefab;

    private BrickAnimator animator;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<BrickAnimator>();
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
                    SpawnMushroom();
                    break;
                // Ajouter d'autres types de briques et leurs loots ici
            }
            SpawnDustEffect();
            Destroy(gameObject);
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

    void SpawnMushroom()
    {
        if (mushroomPrefab != null)
        {
            var go = Instantiate(mushroomPrefab, transform.position + Vector3.down * 0.2f, Quaternion.identity);
            AutoCollect autoCollect = go.GetComponent<AutoCollect>();
            if (autoCollect != null)
            {
                autoCollect.playerData = playerData;
                autoCollect.currencyType = CurrencyType.mushroom;
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