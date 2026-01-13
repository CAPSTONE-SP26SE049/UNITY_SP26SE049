using UnityEngine;

public static class Extensions
{
    private static LayerMask layerMask = LayerMask.GetMask("Default", "Road");

    // Checks if the rigidbody is colliding with an object in a given direction.
    // For example, if you want to check if the player is touching the ground,
    // you can pass Vector2.down. If you want to check if the player is running
    // into a wall, you can pass Vector2.right or Vector2.left.
    public static bool Raycast(this Rigidbody2D rigidbody, Vector2 direction)
    {
        if (rigidbody.isKinematic) {
            return false;
        }

        // Lấy collider để tính toán chính xác hơn (hỗ trợ collider có offset)
        Collider2D collider = rigidbody.GetComponent<Collider2D>();
        if (collider == null) {
            return false;
        }

        // Sử dụng bounds của collider thay vì rigidbody position để chính xác hơn
        Bounds bounds = collider.bounds;
        
        // Tính toán vị trí edge dựa trên bounds
        Vector2 center = bounds.center;
        Vector2 size = bounds.size;
        
        // Tính toán điểm kiểm tra dựa trên hướng
        Vector2 checkPoint;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            // Kiểm tra theo hướng ngang (trái/phải)
            float offsetX = (direction.x > 0 ? 1 : -1) * (size.x / 2f + 0.1f);
            checkPoint = center + new Vector2(offsetX, 0);
        } else {
            // Kiểm tra theo hướng dọc (lên/xuống)
            float offsetY = (direction.y > 0 ? 1 : -1) * (size.y / 2f + 0.1f);
            checkPoint = center + new Vector2(0, offsetY);
        }

        // Kiểm tra xem có collider nào ở vị trí đó không
        Collider2D hitCollider = Physics2D.OverlapCircle(checkPoint, 0.1f, layerMask);
        
        // Nếu không có collider, return false
        if (hitCollider == null || hitCollider.attachedRigidbody == rigidbody) {
            return false;
        }

        // Kiểm tra xem collider có phải là enemy khác không (Goomba, Trau, ConGa, Doc)
        // Nếu là enemy khác, không coi là wall (vì đã ignore collision)
        GameObject hitObject = hitCollider.gameObject;
        if (hitObject.TryGetComponent<Goomba>(out _) ||
            hitObject.TryGetComponent<Trau>(out _) ||
            hitObject.TryGetComponent<ConGa>(out _) ||
            hitObject.TryGetComponent<Doc>(out _))
        {
            // Đây là enemy khác, không phải wall
            return false;
        }

        // Đây là wall thực sự (platform, block, etc.)
        return true;
    }

    // Checks if the transform is facing another transform in a given direction.
    // For example, if you want to check if the player stomps on an enemy, you
    // would pass the player transform, the enemy transform, and Vector2.down.
    public static bool DotTest(this Transform transform, Transform other, Vector2 testDirection)
    {
        Vector2 direction = other.position - transform.position;
        return Vector2.Dot(direction.normalized, testDirection) > 0.25f;
    }

}
