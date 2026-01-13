using UnityEngine;

public class AnimatedSprite : MonoBehaviour
{
    public Sprite[] sprites;
    public float framerate = 1f / 6f;
    public SpriteRenderer spriteRenderer; // Cho phép gán từ bên ngoài

    private int frame;

    private void Awake()
    {
        // Nếu chưa được gán, tự động lấy từ component
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(Animate), framerate, framerate);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Animate()
    {
        // Kiểm tra null để tránh lỗi
        if (spriteRenderer == null || sprites == null || sprites.Length == 0)
        {
            return;
        }

        frame++;

        if (frame >= sprites.Length) {
            frame = 0;
        }

        if (frame >= 0 && frame < sprites.Length && sprites[frame] != null) {
            spriteRenderer.sprite = sprites[frame];
        }
    }

}
