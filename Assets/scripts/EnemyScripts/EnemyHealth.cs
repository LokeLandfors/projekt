using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Health Bar")]
    public Slider healthBar;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // F1 = Damage
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            TakeDamage(10f);
        }

        // F2 = Heal
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            Heal(10f);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.value = currentHealth;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
