using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class HealthSystem : MonoBehaviour
{
    [Header("Death Settings")]
    public TMP_Text deathText;
    public float respawnDelay = 3f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Regeneration")]
    public float healthRegen = 5f;
    public float regenDelay = 3f;

    [Header("Kill Healing")]
    public float healOnKill = 20f;

    [Header("UI")]
    public Slider healthBar;
    public TMP_Text healthText;

    [Header("Testing")]
    public bool enableDebugControls = true;

    private float regenTimer;
    private bool isDead;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        RespawnPoint.SetStartingPosition(transform);
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        UpdateUI();
    }

    void Update()
    {
        // Test controls
        if (enableDebugControls && Keyboard.current != null)
        {
            if (Keyboard.current.f1Key.wasPressedThisFrame)
                TakeDamage(10f);

            if (Keyboard.current.f2Key.wasPressedThisFrame)
                Heal(10f);
        }

        // Health regeneration
        if (!isDead && currentHealth < maxHealth)
        {
            if (regenTimer > 0)
            {
                regenTimer -= Time.deltaTime;
            }
            else
            {
                currentHealth += healthRegen * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }

        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        regenTimer = regenDelay;

        if (currentHealth <= 0)
        {
            Die();
        }

        UpdateUI();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        UpdateUI();
    }

    public void RegisterEnemyKill()
    {
        Heal(healOnKill);
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Disable player movement
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Show death message
        if (deathText != null)
            deathText.gameObject.SetActive(true);

        // Start respawn countdown
        StartCoroutine(RespawnAfterDelay());
    }

    System.Collections.IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Respawn at the latest checkpoint
        RespawnPoint.RespawnPlayer(gameObject);
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Restore health
        currentHealth = maxHealth;
        isDead = false;


        // Hide death message
        if (deathText != null)
            deathText.gameObject.SetActive(false);
    }

    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.value = Mathf.MoveTowards(
                healthBar.value,
                currentHealth,
                100f * Time.deltaTime
            );
        }

        if (healthText != null)
        {
            healthText.text = Mathf.CeilToInt(currentHealth)
                              + " / " +
                              Mathf.CeilToInt(maxHealth) + " HP";
        }
    }
}
