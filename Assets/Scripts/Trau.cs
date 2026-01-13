using UnityEngine;

public class Trau : MonoBehaviour
{
    public Sprite flatSprite;

    private int hitCount = 0;
    private const int maxHits = 2; // Cần 2 hit mới chết
    private TrauAnimatedSprite trauAnimation;
    private EntityMovement entityMovement;

    private void Awake()
    {
        trauAnimation = GetComponent<TrauAnimatedSprite>();
        entityMovement = GetComponent<EntityMovement>();
        
        // Đảm bảo EntityMovement được enable để Trau có thể di chuyển và quay đầu
        if (entityMovement != null)
        {
            // EntityMovement sẽ tự động enable khi OnBecameVisible
            // Không cần enable ở đây vì nó có logic riêng
        }

        // Ignore collision với các enemy khác (Goomba, Trau, ConGa)
        IgnoreEnemyCollisions();
    }

    private void IgnoreEnemyCollisions()
    {
        Collider2D thisCollider = GetComponent<Collider2D>();
        if (thisCollider == null) return;

        // Tìm tất cả các enemy trong scene
        Goomba[] goombas = FindObjectsOfType<Goomba>();
        Trau[] traus = FindObjectsOfType<Trau>();
        ConGa[] conGas = FindObjectsOfType<ConGa>();
        Doc[] docs = FindObjectsOfType<Doc>();

        // Ignore collision với Goomba
        foreach (Goomba goomba in goombas)
        {
            if (goomba != null && goomba.gameObject != gameObject)
            {
                Collider2D otherCollider = goomba.GetComponent<Collider2D>();
                if (otherCollider != null)
                {
                    Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
                }
            }
        }

        // Ignore collision với Trau khác
        foreach (Trau trau in traus)
        {
            if (trau != null && trau.gameObject != gameObject)
            {
                Collider2D otherCollider = trau.GetComponent<Collider2D>();
                if (otherCollider != null)
                {
                    Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
                }
            }
        }

        // Ignore collision với ConGa
        foreach (ConGa conGa in conGas)
        {
            if (conGa != null && conGa.gameObject != gameObject)
            {
                Collider2D otherCollider = conGa.GetComponent<Collider2D>();
                if (otherCollider != null)
                {
                    Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
                }
            }
        }

        // Ignore collision với Doc
        foreach (Doc doc in docs)
        {
            if (doc != null && doc.gameObject != gameObject)
            {
                Collider2D otherCollider = doc.GetComponent<Collider2D>();
                if (otherCollider != null)
                {
                    Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore collision với enemy khác (Goomba, Trau, ConGa, Doc)
        if (collision.gameObject.TryGetComponent<Goomba>(out _) ||
            collision.gameObject.TryGetComponent<Trau>(out _) ||
            collision.gameObject.TryGetComponent<ConGa>(out _) ||
            collision.gameObject.TryGetComponent<Doc>(out _))
        {
            Collider2D thisCollider = GetComponent<Collider2D>();
            Collider2D otherCollider = collision.collider;
            if (thisCollider != null && otherCollider != null)
            {
                Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
            }
            return; // Không xử lý collision với enemy khác
        }

        // Kiểm tra Player (Mario)
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent(out Player player))
        {
            if (player.starpower) {
                // Starpower: chết ngay lập tức
                Hit();
            } else if (collision.transform.DotTest(transform, Vector2.down)) {
                // Mario đạp lên đầu trâu
                TakeHit();
            } else {
                // Mario chạm trâu từ bên cạnh hoặc dưới = chết ngay
                player.Death();
            }
            return;
        }

        // Kiểm tra MainChar
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent(out MainChar mainChar))
        {
            if (mainChar.starpower) {
                // Starpower: chết ngay lập tức
                Hit();
            } else if (collision.transform.DotTest(transform, Vector2.down)) {
                // MainChar đạp lên đầu trâu
                TakeHit();
            } else {
                // MainChar chạm trâu từ bên cạnh hoặc dưới = chết ngay
                mainChar.Death();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Shell")) {
            // Shell đánh trúng: chết ngay (hoặc có thể đổi thành TakeHit() nếu muốn shell cũng cần 2 hit)
            Hit();
        }
    }

    private void TakeHit()
    {
        hitCount++;

        if (hitCount >= maxHits) {
            // Đã đủ 2 hit: chết
            Flatten();
        } else {
            // Hit đầu tiên: bị thương nhưng chưa chết
            GetHurt();
        }
    }

    private void GetHurt()
    {
        // Chuyển sang animation bị thương (vẫn đi bộ bình thường)
        if (trauAnimation != null) {
            trauAnimation.SetHurt(true);
        }

        // Flash effect khi bị thương
        StartCoroutine(HurtFlash());
    }

    private System.Collections.IEnumerator HurtFlash()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        // Flash màu đỏ nhẹ
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    private void Flatten()
    {
        // Sử dụng TrauDeathAnimation để hiển thị Trau_Flat với animation nhảy lên rồi rơi xuống
        TrauDeathAnimation trauDeathAnimation = GetComponent<TrauDeathAnimation>();
        if (trauDeathAnimation != null)
        {
            trauDeathAnimation.Activate(); // Sử dụng Activate() thay vì enabled = true
            Destroy(gameObject, 3.5f); // Thời gian phù hợp với animation (3s animation + buffer)
        }
        else
        {
            // Fallback: cách cũ nếu không có TrauDeathAnimation
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            GetComponent<EntityMovement>().enabled = false;

            if (trauAnimation != null)
                trauAnimation.enabled = false;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && flatSprite != null)
                sr.sprite = flatSprite;

            Destroy(gameObject, 0.5f);
        }
    }


    private void Hit()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        if (trauAnimation != null)
            trauAnimation.enabled = false;

        // Sử dụng TrauDeathAnimation để hiển thị Trau_Flat với animation nhảy lên rồi rơi xuống
        TrauDeathAnimation trauDeathAnimation = GetComponent<TrauDeathAnimation>();
        if (trauDeathAnimation != null)
        {
            trauDeathAnimation.Activate(); // Sử dụng Activate() thay vì enabled = true
        }
        else
        {
            // Fallback về DeathAnimation nếu không có TrauDeathAnimation
            DeathAnimation deathAnimation = GetComponent<DeathAnimation>();
            if (deathAnimation != null)
            {
                // Gán flatSprite vào deadSprite của DeathAnimation
                if (flatSprite != null)
                {
                    deathAnimation.deadSprite = flatSprite;
                }
                deathAnimation.enabled = true;
            }
        }

        Destroy(gameObject, 3.5f); // Thời gian phù hợp với animation
    }


}
