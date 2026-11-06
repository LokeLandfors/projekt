
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrain = 25f;
    public float staminaRegen = 15f;
    public float regenDelay = 1f;

    [Header("UI")]
    public Slider staminaBar;

    private float currentStamina;
    private float regenTimer;
    private bool isSprinting;

    public float CurrentStamina => currentStamina;

    void Start()
    {
        currentStamina = maxStamina;

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    void Update()
    {
        if (isSprinting && currentStamina > 0f)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);

            regenTimer = regenDelay;
        }
        else
        {
            if (regenTimer > 0f)
            {
                regenTimer -= Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegen * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }

        UpdateStaminaUI();
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting && currentStamina > 0f;
    }

    void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.value = Mathf.Lerp(
                staminaBar.value,
                currentStamina,
                10f * Time.deltaTime
            );
        }
    }
}
