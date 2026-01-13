using System.Collections;
using UnityEngine;

public class DocDeathAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite deadSprite; // DocDeath sprite

    private Doc doc;
    private DeathAnimation deathAnimation;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private bool isActivated = false;

    private void Awake()
    {
        doc = GetComponent<Doc>();
        deathAnimation = GetComponent<DeathAnimation>();
        
        // Tự động disable component khi bắt đầu để tránh chạy animation ngay lập tức
        enabled = false;
        isActivated = false;
    }

    private void Start()
    {
        // Đảm bảo component bị disable sau khi Start() được gọi
        if (!isActivated)
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        // Chỉ chạy animation nếu được kích hoạt từ Doc.cs (Hit() hoặc Flatten())
        // Tránh chạy nếu component được enable trong Inspector
        if (!isActivated)
        {
            // Nếu component được enable nhưng chưa được kích hoạt, disable lại
            enabled = false;
            return;
        }

        // Lấy flatSprite từ Doc component nếu có
        if (doc != null && doc.flatSprite != null)
        {
            deadSprite = doc.flatSprite;
        }

        // Nếu có DeathAnimation component, tắt nó đi và dùng DocDeathAnimation thay thế
        if (deathAnimation != null)
        {
            deathAnimation.enabled = false;
        }

        UpdateSprite();
        DisablePhysics();
        StartCoroutine(Animate());
    }

    // Phương thức công khai để Doc.cs gọi khi muốn kích hoạt death animation
    public void Activate()
    {
        isActivated = true;
        enabled = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.enabled = true;
        spriteRenderer.sortingOrder = 10;

        // Ưu tiên dùng deadSprite (DocDeath), nếu không có thì lấy từ Doc component
        if (deadSprite != null)
        {
            spriteRenderer.sprite = deadSprite;
        }
        else if (doc != null && doc.flatSprite != null)
        {
            spriteRenderer.sprite = doc.flatSprite;
        }
    }

    private void DisablePhysics()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        // Tắt Rigidbody2D và reset velocity để animation tự điều khiển vị trí
        if (TryGetComponent(out Rigidbody2D rigidbody))
        {
            rigidbody.velocity = Vector2.zero;
            rigidbody.isKinematic = true;
        }

        if (TryGetComponent(out PlayerMovement playerMovement))
        {
            playerMovement.enabled = false;
        }

        if (TryGetComponent(out EntityMovement entityMovement))
        {
            entityMovement.enabled = false;
        }

        // Tắt DocAnimatedSprite khi chết
        if (TryGetComponent(out DocAnimatedSprite docAnimation))
        {
            docAnimation.enabled = false;
        }
    }

    private IEnumerator Animate()
    {
        // Lưu vị trí ban đầu
        Vector3 startPosition = transform.position;
        
        float elapsed = 0f;
        float duration = 3f;

        // Vận tốc nhảy lên giống Mario
        float jumpVelocity = 10f;
        // Trọng lực để rơi xuống
        float gravity = -36f;

        // Vận tốc ban đầu: nhảy thẳng lên
        Vector3 velocity = Vector3.up * jumpVelocity;

        while (elapsed < duration)
        {
            // Cập nhật vị trí dựa trên vận tốc
            transform.position += velocity * Time.deltaTime;
            
            // Áp dụng trọng lực (giảm vận tốc y)
            velocity.y += gravity * Time.deltaTime;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

}
