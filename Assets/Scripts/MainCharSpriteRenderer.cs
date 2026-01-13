using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MainCharSpriteRenderer : MonoBehaviour
{
    private MainCharMovement movement;
    public SpriteRenderer spriteRenderer { get; private set; }
    public Sprite idle;
    public Sprite jump;
    public Sprite slide;
    public AnimatedSprite run;

    private void Awake()
    {
        movement = GetComponentInParent<MainCharMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        // Kiểm tra null để tránh NullReferenceException
        if (movement == null || spriteRenderer == null)
        {
            return;
        }

        // Kiểm tra xem run animation có đang active không
        bool isRunActive = run != null && run.enabled && movement.running && !movement.jumping && !movement.sliding;
        
        // Chỉ set run animation enabled khi cần
        if (run != null)
        {
            run.enabled = movement.running && !movement.jumping && !movement.sliding;
        }

        // Nếu run animation đang active, KHÔNG set sprite (để run animation tự quản lý)
        if (isRunActive)
        {
            return; // Không làm gì, để run animation tự quản lý sprite
        }

        // Ưu tiên: Jump > Slide > Idle (khi không chạy)
        if (movement.jumping) {
            // Khi nhảy: hiển thị jump sprite
            if (jump != null)
            {
                spriteRenderer.sprite = jump;
            }
        } else if (movement.sliding) {
            // Khi trượt: hiển thị slide sprite
            if (slide != null)
            {
                spriteRenderer.sprite = slide;
            }
        } else {
            // Khi đứng yên: hiển thị idle sprite
            if (idle != null)
            {
                spriteRenderer.sprite = idle;
            }
        }
    }

    private void OnEnable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        if (run != null)
        {
            run.enabled = false;
        }
    }

}
