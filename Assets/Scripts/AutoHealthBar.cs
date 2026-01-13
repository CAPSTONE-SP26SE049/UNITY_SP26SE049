using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script tự động tạo Health Bar khi game chạy
/// Gắn script này vào bất kỳ GameObject nào trong scene (hoặc tạo GameObject mới)
/// </summary>
[DefaultExecutionOrder(-100)]
public class AutoHealthBar : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    private HealthBar healthBar;
    private Canvas canvas;

    private void Awake()
    {
        // Kiểm tra xem HealthBar đã tồn tại chưa
        healthBar = FindObjectOfType<HealthBar>();
        if (healthBar != null)
        {
            // HealthBar đã tồn tại, chỉ cập nhật giá trị
            healthBar.maxHealth = maxHealth;
            healthBar.currentHealth = currentHealth;
            return;
        }

        // Tạo Canvas nếu chưa có
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            // Add CanvasScaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Add GraphicRaycaster
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Tạo HealthBar
        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(canvas.transform, false);
        
        // Đảm bảo có RectTransform trước khi add HealthBar component
        RectTransform rect = healthBarObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(200, 30);
        rect.anchoredPosition = new Vector2(-10, -10);
        
        healthBar = healthBarObj.AddComponent<HealthBar>();
        healthBar.maxHealth = maxHealth;
        healthBar.currentHealth = currentHealth;
        
        Debug.Log($"Health Bar đã được tạo! Canvas: {canvas.name}, HealthBar: {healthBarObj.name}");
    }

    private void Start()
    {
        // Đảm bảo HealthBar được cập nhật
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }

    // Các method để điều khiển máu từ bên ngoài
    public void SetHealth(int health)
    {
        currentHealth = health;
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
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
}

