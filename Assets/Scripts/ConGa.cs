using UnityEngine;

public class ConGa : MonoBehaviour
{
    public Sprite flatSprite;

    private void Awake()
    {
        // Ignore collision với các enemy khác (Goomba, Trau, ConGa khác)
        IgnoreEnemyCollisions();
    }

    private void Start()
    {
        // Đảm bảo ignore collision được thiết lập sau khi tất cả object đã được khởi tạo
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

        // Ignore collision với Trau
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

        // Ignore collision với ConGa khác
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
                Hit();
            } else if (collision.transform.DotTest(transform, Vector2.down)) {
                Flatten();
            } else {
                player.Hit();
            }
            return;
        }

        // Kiểm tra MainChar
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent(out MainChar mainChar))
        {
            if (mainChar.starpower) {
                Hit();
            } else if (collision.transform.DotTest(transform, Vector2.down)) {
                Flatten();
            } else {
                mainChar.Hit();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Shell")) {
            Hit();
        }
    }

    private void Flatten()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<EntityMovement>().enabled = false;
        GetComponent<ConGaAnimatedSprite>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = flatSprite;
        Destroy(gameObject, 0.5f);
    }

    private void Hit()
    {
        GetComponent<ConGaAnimatedSprite>().enabled = false;
        GetComponent<ConGaDeathAnimation>().enabled = true;
        Destroy(gameObject, 3f);
    }

}
