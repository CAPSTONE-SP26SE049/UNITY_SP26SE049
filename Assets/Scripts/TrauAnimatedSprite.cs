using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TrauAnimatedSprite : MonoBehaviour
{
    [Header("Normal Animation")]
    public Sprite[] normalSprites; // Sprites animation đi bộ bình thường
    public float normalFramerate = 1f / 6f;

    [Header("Hurt Animation")]
    public Sprite[] hurtSprites; // Sprites animation đi bộ khi bị thương (tùy chọn)
    public float hurtFramerate = 1f / 6f;

    private SpriteRenderer spriteRenderer;
    private int frame;
    private bool isHurt = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // Bắt đầu với animation bình thường
        isHurt = false;
        frame = 0;
        
        // Chỉ bắt đầu animation nếu có sprites
        if (normalSprites != null && normalSprites.Length > 0)
        {
            InvokeRepeating(nameof(Animate), normalFramerate, normalFramerate);
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    /// <summary>
    /// Chuyển sang animation bị thương
    /// </summary>
    public void SetHurt(bool hurt)
    {
        if (isHurt == hurt) return; // Đã ở trạng thái này rồi

        isHurt = hurt;
        frame = 0; // Reset frame khi chuyển đổi

        // Hủy invoke cũ
        CancelInvoke();

        // Bắt đầu animation mới với framerate tương ứng
        if (isHurt && hurtSprites != null && hurtSprites.Length > 0)
        {
            // Có animation bị thương riêng
            InvokeRepeating(nameof(Animate), hurtFramerate, hurtFramerate);
        }
        else if (!isHurt && normalSprites != null && normalSprites.Length > 0)
        {
            // Quay lại animation bình thường
            InvokeRepeating(nameof(Animate), normalFramerate, normalFramerate);
        }
        else if (normalSprites != null && normalSprites.Length > 0)
        {
            // Không có animation bị thương, dùng animation bình thường
            InvokeRepeating(nameof(Animate), normalFramerate, normalFramerate);
        }
        // Nếu không có sprites nào, không bắt đầu animation
    }

    private void Animate()
    {
        // Kiểm tra spriteRenderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                return; // Không có SpriteRenderer
            }
        }

        Sprite[] currentSprites = GetCurrentSprites();
        
        if (currentSprites == null || currentSprites.Length == 0)
        {
            return; // Không có sprites để animate
        }

        frame++;

        if (frame >= currentSprites.Length)
        {
            frame = 0;
        }

        if (frame >= 0 && frame < currentSprites.Length)
        {
            spriteRenderer.sprite = currentSprites[frame];
        }
    }

    private Sprite[] GetCurrentSprites()
    {
        // Nếu bị thương và có animation bị thương riêng
        if (isHurt && hurtSprites != null && hurtSprites.Length > 0)
        {
            return hurtSprites;
        }
        
        // Mặc định dùng animation bình thường
        return normalSprites;
    }

    /// <summary>
    /// Kiểm tra xem đang ở trạng thái bị thương không
    /// </summary>
    public bool IsHurt()
    {
        return isHurt;
    }

}
