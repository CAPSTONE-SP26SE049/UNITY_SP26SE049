using UnityEngine;

[RequireComponent(typeof(EntityMovement))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Doc : MonoBehaviour
{
    public Sprite flatSprite;

    private DocAnimatedSprite docAnimation;
    private EntityMovement entityMovement;

    private void Awake()
    {
        docAnimation = GetComponent<DocAnimatedSprite>();
        entityMovement = GetComponent<EntityMovement>();

        // Ignore collision với các enemy khác
        IgnoreEnemyCollisions();
    }

    private void Start()
    {
        // Đảm bảo ignore collision được thiết lập sau khi tất cả object đã được khởi tạo
        IgnoreEnemyCollisions();
        
        // Đảm bảo EntityMovement được enable để Doc có thể di chuyển
        // EntityMovement tự enable khi OnBecameVisible, nhưng nếu object đã visible từ đầu
        // thì OnBecameVisible không được gọi, nên cần enable thủ công
        if (entityMovement == null)
        {
            entityMovement = GetComponent<EntityMovement>();
        }
        
        if (entityMovement != null)
        {
            // Đảm bảo direction ban đầu là từ phải sang trái (left)
            entityMovement.direction = Vector2.left;
            
            // Sprite Doc mặc định hướng về bên phải, nên khi đi sang trái cần quay 180 độ
            // Đảo ngược logic so với EntityMovement (vì sprite Doc hướng ngược lại)
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            
            // Luôn enable EntityMovement khi bắt đầu
            entityMovement.enabled = true;
        }
        
        // Bắt đầu với animation di chuyển
        if (docAnimation != null)
        {
            docAnimation.PlayMove();
        }
    }

    private void Update()
    {
        // Đảm bảo EntityMovement component tồn tại
        if (entityMovement == null)
        {
            entityMovement = GetComponent<EntityMovement>();
        }

        // Đảm bảo EntityMovement luôn enable (giống ConGa - để EntityMovement tự điều khiển di chuyển)
        if (entityMovement != null && !entityMovement.enabled)
        {
            entityMovement.enabled = true;
        }
    }

    private void LateUpdate()
    {
        // Đảo ngược logic flip cho Doc (vì sprite Doc hướng về bên phải mặc định)
        // Chạy sau FixedUpdate() của EntityMovement để override transform rotation
        if (entityMovement != null)
        {
            // Nếu EntityMovement set quay 180 độ (đi sang phải), ta không quay (để sprite hướng phải)
            // Nếu EntityMovement set không quay (đi sang trái), ta quay 180 độ (để sprite hướng trái)
            if (entityMovement.direction.x > 0f)
            {
                // Đi sang phải - sprite mặc định đã hướng phải, nên không cần quay
                transform.localEulerAngles = Vector3.zero;
            }
            else if (entityMovement.direction.x < 0f)
            {
                // Đi sang trái - sprite mặc định hướng phải, nên cần quay 180 độ để hướng trái
                transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            }
        }
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

        // Ignore collision với Doc khác
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
        // Ignore collision với enemy khác
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
        // Sử dụng DocDeathAnimation để hiển thị DocDeath với animation nhảy lên rồi rơi xuống
        DocDeathAnimation docDeathAnimation = GetComponent<DocDeathAnimation>();
        if (docDeathAnimation != null)
        {
            docDeathAnimation.Activate(); // Sử dụng Activate() thay vì enabled = true
            Destroy(gameObject, 3.5f); // Thời gian phù hợp với animation (3s animation + buffer)
        }
        else
        {
            // Fallback: cách cũ nếu không có DocDeathAnimation
            GetComponent<Collider2D>().enabled = false;
            if (entityMovement != null)
            {
                entityMovement.enabled = false;
            }
            if (docAnimation != null)
            {
                docAnimation.enabled = false;
            }
            GetComponent<SpriteRenderer>().sprite = flatSprite;
            Destroy(gameObject, 0.5f);
        }
    }

    private void Hit()
    {
        if (docAnimation != null)
        {
            docAnimation.enabled = false;
        }
        
        // Sử dụng DocDeathAnimation để hiển thị DocDeath với animation nhảy lên rồi rơi xuống
        DocDeathAnimation docDeathAnimation = GetComponent<DocDeathAnimation>();
        if (docDeathAnimation != null)
        {
            docDeathAnimation.Activate(); // Sử dụng Activate() thay vì enabled = true
        }
        else
        {
            // Fallback về DeathAnimation nếu không có DocDeathAnimation
            DeathAnimation deathAnimation = GetComponent<DeathAnimation>();
            if (deathAnimation != null)
            {
                deathAnimation.enabled = true;
            }
        }
        Destroy(gameObject, 3.5f); // Thời gian phù hợp với animation (3s animation + buffer)
    }

}
