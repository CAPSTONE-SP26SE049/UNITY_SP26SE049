using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("UI References")]
    public Image healthBarFill;
    public Image healthBarBackground;
    public Image healthBarBorder;

    [Header("Pixel Style Settings")]
    public Color healthColor = new Color(1f, 0.2f, 0.2f); // Red
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f); // Dark gray
    public Color borderColor = new Color(0f, 0f, 0f); // Black
    public int borderWidth = 2;

    private RectTransform rectTransform;
    private Canvas canvas;
    private static Sprite whiteSprite;

    // Tạo sprite trắng đơn giản
    private static Sprite CreateWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        return whiteSprite;
    }

    private void Awake()
    {
        // Đảm bảo có RectTransform
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("HealthBar không có Canvas parent!");
        }

        // Create UI elements if they don't exist
        if (healthBarBackground == null || healthBarFill == null || healthBarBorder == null)
        {
            CreateHealthBarUI();
        }
    }

    private void OnEnable()
    {
        // Position in top right corner sau khi enable
        if (rectTransform != null)
        {
            PositionInTopRight();
        }
    }

    private void Start()
    {
        // Đảm bảo position được set lại
        if (rectTransform != null)
        {
            PositionInTopRight();
        }
        UpdateHealthBar();
    }

    private void CreateHealthBarUI()
    {
        Debug.Log("Đang tạo HealthBar UI elements...");
        
        // Create border (outermost)
        GameObject borderObj = new GameObject("HealthBarBorder");
        borderObj.transform.SetParent(transform, false);
        healthBarBorder = borderObj.AddComponent<Image>();
        healthBarBorder.color = borderColor;
        healthBarBorder.sprite = CreateWhiteSprite();
        healthBarBorder.raycastTarget = false; // Tối ưu performance

        RectTransform borderRect = borderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0, 0);
        borderRect.anchorMax = new Vector2(1, 1);
        borderRect.sizeDelta = Vector2.zero;
        borderRect.anchoredPosition = Vector2.zero;

        // Create background
        GameObject bgObj = new GameObject("HealthBarBackground");
        bgObj.transform.SetParent(borderObj.transform, false);
        healthBarBackground = bgObj.AddComponent<Image>();
        healthBarBackground.color = backgroundColor;
        healthBarBackground.sprite = CreateWhiteSprite();
        healthBarBackground.raycastTarget = false;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.sizeDelta = new Vector2(-borderWidth * 2, -borderWidth * 2);
        bgRect.anchoredPosition = Vector2.zero;

        // Create fill
        GameObject fillObj = new GameObject("HealthBarFill");
        fillObj.transform.SetParent(bgObj.transform, false);
        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = healthColor;
        healthBarFill.sprite = CreateWhiteSprite();
        healthBarFill.raycastTarget = false;
        
        Debug.Log($"HealthBar UI đã được tạo! Border: {healthBarBorder != null}, Background: {healthBarBackground != null}, Fill: {healthBarFill != null}");

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1); // Will be updated based on health
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
    }

    private void PositionInTopRight()
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform is null!");
            return;
        }

        // Set anchor to top right
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);

        // Set size (200x30 pixels for pixel style)
        rectTransform.sizeDelta = new Vector2(200, 30);

        // Position with offset from top right corner (10 pixels from edges)
        rectTransform.anchoredPosition = new Vector2(-10, -10);

        // Ensure pixel-perfect rendering
        SetPixelPerfect();
        
        Debug.Log($"HealthBar positioned at: {rectTransform.anchoredPosition}, Size: {rectTransform.sizeDelta}, Canvas: {canvas != null}");
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    public void SetMaxHealth(int maxHealth)
    {
        this.maxHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        UpdateHealthBar();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            
            // Update fill using anchor to create pixel-perfect scaling
            RectTransform fillRect = healthBarFill.GetComponent<RectTransform>();
            if (fillRect != null)
            {
                fillRect.anchorMin = new Vector2(0, 0);
                fillRect.anchorMax = new Vector2(healthPercentage, 1);
                fillRect.sizeDelta = Vector2.zero;
                fillRect.anchoredPosition = Vector2.zero;
            }
        }
    }

    // Method to ensure pixel-perfect rendering
    public void SetPixelPerfect()
    {
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }
}

