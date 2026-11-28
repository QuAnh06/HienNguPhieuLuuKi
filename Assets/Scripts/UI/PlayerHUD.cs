using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;

    [Header("Gold UI")]
    [SerializeField] private TMP_Text goldText;

    private PlayerHealth playerHealth;
    private EconomyManager economyManager;

    private void Start()
    {
        playerHealth = PlayerHealth.Instance;
        if (playerHealth == null)
        {
            // Scene không có Player (như Sample Scene 2) -> Tự ẩn an toàn
            gameObject.SetActive(false);
            return;
        }

        playerHealth.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        economyManager = EconomyManager.Instance;
        if (economyManager != null)
        {
            economyManager.OnGoldChanged += UpdateGoldText;
            UpdateGoldText(economyManager.CurrentGold);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }

        if (economyManager != null)
        {
            economyManager.OnGoldChanged -= UpdateGoldText;
        }
    }

    private void UpdateHealthBar(int currentHp, int maxHp)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHp;
            healthSlider.value = currentHp;
        }
    }

    private void UpdateGoldText(int gold)
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString("D3");
        }
    }
}
