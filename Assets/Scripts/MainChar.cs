using UnityEngine;

public class MainChar : MonoBehaviour
{
    public CapsuleCollider2D capsuleCollider { get; private set; }
    public MainCharMovement movement { get; private set; }
    public DeathAnimation deathAnimation { get; private set; }

    public MainCharSpriteRenderer spriteRenderer;
    private MainCharSpriteRenderer activeRenderer;

    public bool dead => deathAnimation.enabled;
    public bool starpower { get; private set; }

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        movement = GetComponent<MainCharMovement>();
        deathAnimation = GetComponent<DeathAnimation>();
        activeRenderer = spriteRenderer;
        
        // Đảm bảo DeathAnimation bị disable khi bắt đầu
        if (deathAnimation != null)
        {
            deathAnimation.enabled = false;
        }

        // Đảm bảo không có SpriteRenderer trên MainChar (parent) - chỉ có trên MainCharRenderer (child)
        SpriteRenderer parentRenderer = GetComponent<SpriteRenderer>();
        if (parentRenderer != null)
        {
            parentRenderer.enabled = false;
        }

        // Đảm bảo MainCharSpriteRenderer chỉ có trên child, không có trên parent
        MainCharSpriteRenderer parentSpriteRenderer = GetComponent<MainCharSpriteRenderer>();
        if (parentSpriteRenderer != null)
        {
            parentSpriteRenderer.enabled = false;
        }
    }

    private void Start()
    {
        // Đảm bảo MainCharMovement được enable để có thể di chuyển
        if (movement != null && !movement.enabled)
        {
            movement.enabled = true;
        }
    }

    public void Hit()
    {
        if (!dead && !starpower)
        {
            // MainChar không có cơ chế biến hình, nên bị hit là chết luôn
            Death();
        }
    }

    public void Death()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        deathAnimation.enabled = true;

        GameManager.Instance.ResetLevel(3f);
    }

    public void Starpower()
    {
        StartCoroutine(StarpowerAnimation());
    }

    private System.Collections.IEnumerator StarpowerAnimation()
    {
        starpower = true;

        float elapsed = 0f;
        float duration = 10f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (Time.frameCount % 4 == 0 && activeRenderer != null && activeRenderer.spriteRenderer != null) {
                activeRenderer.spriteRenderer.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            }

            yield return null;
        }

        if (activeRenderer != null && activeRenderer.spriteRenderer != null)
        {
            activeRenderer.spriteRenderer.color = Color.white;
        }
        starpower = false;
    }

}
