using System.Collections;
using UnityEngine;

public class ConGaDeathAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite deadSprite;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        // Tự động disable component khi bắt đầu để tránh chạy animation ngay lập tức
        enabled = false;
    }

    private void OnEnable()
    {
        UpdateSprite();
        DisablePhysics();
        StartCoroutine(Animate());
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

        if (deadSprite != null) {
            spriteRenderer.sprite = deadSprite;
        }
    }

    private void DisablePhysics()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();

        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }

        if (TryGetComponent(out Rigidbody2D rigidbody)) {
            rigidbody.velocity = Vector2.zero;
            rigidbody.isKinematic = true;
        }

        if (TryGetComponent(out PlayerMovement playerMovement)) {
            playerMovement.enabled = false;
        }

        if (TryGetComponent(out EntityMovement entityMovement)) {
            entityMovement.enabled = false;
        }

        // Tắt ConGaAnimatedSprite khi chết
        if (TryGetComponent(out ConGaAnimatedSprite conGaAnimation)) {
            conGaAnimation.enabled = false;
        }
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        float duration = 3f;

        float jumpVelocity = 10f;
        float gravity = -36f;

        Vector3 velocity = Vector3.up * jumpVelocity;

        while (elapsed < duration)
        {
            transform.position += velocity * Time.deltaTime;
            velocity.y += gravity * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

}
