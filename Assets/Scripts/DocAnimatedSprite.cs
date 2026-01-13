using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DocAnimatedSprite : MonoBehaviour
{
    [Header("Idle Animation")]
    public Sprite[] idleSprites;
    public float idleFramerate = 1f / 6f;

    [Header("Move Animation")]
    public Sprite[] moveSprites;
    public float moveFramerate = 1f / 6f;

    private SpriteRenderer spriteRenderer;
    private int frame;
    private State currentState = State.Idle;
    private bool isAnimating = false;

    public enum State
    {
        Idle,
        Move
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        PlayMove(); // Bắt đầu với animation di chuyển
    }

    private void OnDisable()
    {
        CancelInvoke();
        isAnimating = false;
    }

    public void PlayIdle()
    {
        currentState = State.Idle;
        StartAnimation(idleSprites, idleFramerate);
    }

    public void PlayMove()
    {
        currentState = State.Move;
        StartAnimation(moveSprites, moveFramerate);
    }

    private void StartAnimation(Sprite[] sprites, float framerate, bool loop = true)
    {
        CancelInvoke();

        frame = 0;
        isAnimating = sprites != null && sprites.Length > 0;

        if (!isAnimating)
            return;

        // Set frame 0 ngay lập tức
        spriteRenderer.sprite = sprites[0];

        // Nếu chỉ có 1 sprite -> khỏi InvokeRepeating
        if (sprites.Length == 1)
            return;

        InvokeRepeating(nameof(Animate), framerate, framerate);
        _loop = loop;
    }

    private bool _loop = true;

    private void Animate()
    {
        Sprite[] current = GetCurrentSprites();
        if (current == null || current.Length == 0) return;

        frame++;

        if (frame >= current.Length)
        {
            frame = 0;
        }

        spriteRenderer.sprite = current[frame];
    }

    private Sprite[] GetCurrentSprites()
    {
        switch (currentState)
        {
            case State.Idle:
                return idleSprites;
            case State.Move:
                return moveSprites;
            default:
                return moveSprites;
        }
    }

}
