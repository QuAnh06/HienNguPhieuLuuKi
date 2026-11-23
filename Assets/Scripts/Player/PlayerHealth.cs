using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float damageCooldown = 0.6f;
    [SerializeField] private float knockbackThrust = 10f;

    private int currentHealth;
    private bool canTakeDamage = true;
    private Knockback knockback;
    private Flash flash;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        knockback = GetComponent<Knockback>();
        flash = GetComponent<Flash>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 hitDirection)
    {
        if (!canTakeDamage || currentHealth <= 0) return;

        canTakeDamage = false;
        currentHealth -= amount;

        if (knockback != null) knockback.GetKnockedBack(hitDirection, knockbackThrust);
        if (flash != null) StartCoroutine(flash.FlashRoutine());

        StartCoroutine(DamageRecoveryRoutine());

        if (currentHealth <= 0)
        {
            Debug.Log("Player has died!");
        }
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }
}
