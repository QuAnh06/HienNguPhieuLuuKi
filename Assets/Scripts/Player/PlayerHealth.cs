using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private int maxHealth = 7;
    [SerializeField] private float damageCooldown = 0.6f;
    [SerializeField] private float knockbackThrust = 10f;

    private int currentHealth;
    private bool canTakeDamage = true;
    private bool isDead = false;
    private Knockback knockback;
    private Flash flash;
    private Animator animator;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        knockback = GetComponent<Knockback>();
        flash = GetComponent<Flash>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void TakeDamage(int amount, Vector2 hitDirection)
    {
        if (!canTakeDamage || currentHealth <= 0 || isDead) return;

        canTakeDamage = false;
        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (knockback != null) knockback.GetKnockedBack(hitDirection, knockbackThrust);
        if (flash != null) StartCoroutine(flash.FlashRoutine());
        if (animator != null) animator.SetTrigger("Hurt");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageRecoveryRoutine());
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (ActiveWeapon.Instance != null)
        {
            ActiveWeapon.Instance.gameObject.SetActive(false);
        }

        StartCoroutine(PlayerDeathRoutine());
    }

    private IEnumerator PlayerDeathRoutine()
    {
        yield return new WaitForSeconds(0.4f);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(1.2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void HealPlayer(int amount = 1)
    {
        if (isDead) return;
        if (currentHealth < maxHealth)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }
}
