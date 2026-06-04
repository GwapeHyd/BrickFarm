using UnityEngine;
using UnityEngine.UI;

public class Brick : MonoBehaviour
{
    public PlayerData playerData;
    public GameObject dustEffectPrefab;
    private LevelManager levelManager;
    private BrickAnimator animator;

    public GameObject coinPrefab;

    [Header("Damage Popup")]
    public GameObject popupPrefab;

    [Header("Health Bar")]
    public GameObject healthBarRoot;  // parent à activer/désactiver
    public Image healthBarFill;       // Image type Filled, enfant de healthBarRoot
    private int health;
    private int maxHealth;
    public void SetMaxHealth(int hp)
    {
        maxHealth = hp;
        health = maxHealth;
    }

    public void Start()
    {
        animator = GetComponentInChildren<BrickAnimator>();
    }

    public void Init(LevelManager manager)
    {
        levelManager = manager;
        SetMaxHealth(manager.GetCurrentLevel() * 2);
        if (healthBarRoot != null) healthBarRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.OnBrickDestroyed(gameObject);
        }
    }

    void HandleHit(int damage)
    {
        if (animator != null)
            animator.StartCoroutine(animator.ShakeEffect());

        health -= damage;
        health = Mathf.Max(health, 0);

        // Popup dégâts
        if (popupPrefab != null)
        {
            var popup = Instantiate(popupPrefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            popup.GetComponent<FloatingPopup>().Init("-" + damage, Color.white);
        }

        // Barre de vie
        UpdateHealthBar();

        if (health <= 0)
        {
            if (healthBarRoot != null) healthBarRoot.SetActive(false);
            SpawnDustEffect();
            SpawnCoin();
            Destroy(gameObject);
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)health / maxHealth;
        if (healthBarRoot != null)
            healthBarRoot.SetActive(health > 0 && health < maxHealth);
    }

    void SpawnDustEffect()
    {
        if (dustEffectPrefab != null)
        {
            Instantiate(dustEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    void SpawnCoin()
    {
        if (coinPrefab != null)
        {
            var go = Instantiate(coinPrefab, transform.position + Vector3.down * 0.2f, Quaternion.identity);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ball")) return;

        Ball ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null)
        {
            int damage = playerData.damage * (int)playerData.damageMultiplier;
            // Ajout du bonus de damage du skill tree
            if (levelManager != null && levelManager.skillTreeManager != null)
            {
                damage += levelManager.skillTreeManager.GetDamageBonus();
            }
            HandleHit(damage);
        }
    }

    void OnMouseDown()
    {
        if (levelManager == null || !levelManager.IsLevelActive()) return;
        if (levelManager.skillTreeManager == null) return;
        if (!levelManager.skillTreeManager.IsClickUnlocked()) return;
        if (!levelManager.HasClicksLeft()) return;

        int clickDamage = levelManager.skillTreeManager.GetClickDamage();
        levelManager.UseClick();
        HandleHit(clickDamage);
    }

}
