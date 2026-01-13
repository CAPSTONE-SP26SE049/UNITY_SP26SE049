using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Health Bar Reference")]
    public HealthBar healthBar;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Tìm HealthBar đã tồn tại (từ AutoHealthBar)
        if (healthBar == null)
        {
            healthBar = FindObjectOfType<HealthBar>();
        }

        // Create Canvas and HealthBar if they don't exist
        if (healthBar == null)
        {
            SetupHealthBar();
        }

        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }

    private void SetupHealthBar()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            // Add CanvasScaler for pixel-perfect rendering
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Add GraphicRaycaster
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Create HealthBar GameObject
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(canvas.transform, false);
        healthBar = healthBarObj.AddComponent<HealthBar>();
        healthBar.maxHealth = maxHealth;
        healthBar.currentHealth = currentHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }

    protected virtual void OnDeath()
    {
        // Override this in Player class or other classes
        Debug.Log("Health reached 0!");
    }
}

