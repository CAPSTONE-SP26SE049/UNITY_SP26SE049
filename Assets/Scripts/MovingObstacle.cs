using UnityEngine;

/// <summary>
/// Component làm cho vật cản di chuyển qua lại trái phải với animation sprite
/// Hỗ trợ: 2 sprite đứng yên (trái/phải) và nhiều sprite chạy (trái/phải) để tạo hiệu ứng animation
/// </summary>
public class MovingObstacle : MonoBehaviour
{
    [Header("Thiết lập di chuyển")]
    [Tooltip("Tốc độ di chuyển (đơn vị: units/giây)")]
    public float speed = 2f;
    
    [Tooltip("Khoảng cách di chuyển từ điểm xuất phát (đơn vị: units)")]
    public float distance = 5f;
    
    [Header("Hướng ban đầu")]
    [Tooltip("Hướng di chuyển ban đầu: true = phải, false = trái")]
    public bool startMovingRight = true;
    
    [Header("Sprites đứng yên")]
    [Tooltip("Sprite đứng yên hướng phải (kéo từ Project vào đây)")]
    public Sprite idleSpriteRight;
    
    [Tooltip("Sprite đứng yên hướng trái (kéo từ Project vào đây)")]
    public Sprite idleSpriteLeft;
    
    [Header("Sprites chạy (theo thứ tự khung hình)")]
    [Tooltip("Danh sách sprite chạy hướng phải (đặt theo thứ tự frame 1,2,3...)")]
    public Sprite[] runSpritesRight;
    
    [Tooltip("Danh sách sprite chạy hướng trái (đặt theo thứ tự frame 1,2,3...)")]
    public Sprite[] runSpritesLeft;
    
    [Header("Thiết lập Animation")]
    [Tooltip("Tốc độ chuyển đổi giữa các sprite chạy (frames/giây)")]
    public float animationSpeed = 8f;
    
    [Header("Tùy chọn")]
    [Tooltip("Tạm dừng di chuyển")]
    public bool isPaused = false;
    
    [Tooltip("Tự động bắt đầu di chuyển khi game bắt đầu")]
    public bool autoStart = true;
    
    [Tooltip("Sử dụng vị trí hiện tại làm điểm xuất phát")]
    public bool useCurrentPositionAsStart = true;
    
    // Vị trí điểm xuất phát
    private Vector3 startPosition;
    
    // Hướng di chuyển hiện tại (1 = phải, -1 = trái)
    private int direction = 1;
    
    // Vị trí hiện tại trong quỹ đạo (0 đến distance)
    private float currentDistance = 0f;
    
    // SpriteRenderer để hiển thị sprite
    private SpriteRenderer spriteRenderer;
    
    // Thời gian để tính toán animation
    private float animationTimer = 0f;
    
    // Index hiện tại của frame chạy
    private int runFrameIndex = 0;

    void Start()
    {
        // Lấy hoặc tạo SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Lưu vị trí ban đầu
        if (useCurrentPositionAsStart)
        {
            startPosition = transform.position;
        }
        else
        {
            startPosition = transform.position;
        }
        
        // Thiết lập hướng ban đầu
        direction = startMovingRight ? 1 : -1;
        
        // Cập nhật sprite ban đầu
        UpdateSprite();
        
        // Nếu không tự động bắt đầu, tạm dừng
        if (!autoStart)
        {
            isPaused = true;
            UpdateSprite(); // Cập nhật lại để hiển thị sprite đứng yên
        }
    }

    void Update()
    {
        // Nếu đang tạm dừng, hiển thị sprite đứng yên
        if (isPaused)
        {
            UpdateSprite();
            return;
        }
        
        // Cập nhật animation timer
        animationTimer += Time.deltaTime * animationSpeed;
        
        // Chuyển đổi giữa các frame chạy theo FPS
        float frameDuration = 1f / Mathf.Max(1f, animationSpeed);
        if (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;
            AdvanceRunFrame();
            UpdateSprite(); // Cập nhật sprite
        }
        
        // Cập nhật khoảng cách di chuyển
        currentDistance += speed * Time.deltaTime * direction;
        
        // Kiểm tra và đổi hướng khi đến điểm cuối
        if (currentDistance >= distance)
        {
            currentDistance = distance;
            direction = -1; // Đổi sang trái
            UpdateSprite(); // Cập nhật sprite khi đổi hướng
        }
        else if (currentDistance <= 0)
        {
            currentDistance = 0;
            direction = 1; // Đổi sang phải
            UpdateSprite(); // Cập nhật sprite khi đổi hướng
        }
        
        // Cập nhật vị trí mới
        Vector3 newPosition = startPosition;
        newPosition.x += currentDistance * (startMovingRight ? 1 : -1);
        transform.position = newPosition;
    }

    /// <summary>
    /// Cập nhật sprite dựa trên trạng thái (đứng yên/chạy) và hướng (trái/phải)
    /// </summary>
    private void UpdateSprite()
    {
        if (spriteRenderer == null)
            return;
        
        Sprite spriteToUse = null;
        
        if (isPaused)
        {
            // Đang đứng yên - hiển thị sprite đứng yên
            if (direction == 1) // Hướng phải
            {
                spriteToUse = idleSpriteRight;
            }
            else // Hướng trái
            {
                spriteToUse = idleSpriteLeft;
            }
        }
        else
        {
            // Đang di chuyển - hiển thị sprite chạy với animation
            Sprite[] runSet = direction == 1 ? runSpritesRight : runSpritesLeft;
            if (runSet != null && runSet.Length > 0)
            {
                int idx = Mathf.Clamp(runFrameIndex % runSet.Length, 0, runSet.Length - 1);
                spriteToUse = runSet[idx];
            }
        }
        
        // Áp dụng sprite nếu có
        if (spriteToUse != null)
        {
            spriteRenderer.sprite = spriteToUse;
        }
        else
        {
            // Nếu không tìm thấy sprite phù hợp, thử dùng sprite đứng yên
            Sprite fallbackSprite = null;
            if (direction == 1 && idleSpriteRight != null)
            {
                fallbackSprite = idleSpriteRight;
            }
            else if (direction == -1 && idleSpriteLeft != null)
            {
                fallbackSprite = idleSpriteLeft;
            }
            
            if (fallbackSprite != null)
            {
                spriteRenderer.sprite = fallbackSprite;
            }
        }
    }

    /// <summary>
    /// Bắt đầu di chuyển
    /// </summary>
    public void StartMoving()
    {
        isPaused = false;
        animationTimer = 0f;
        runFrameIndex = 0;
        UpdateSprite();
    }

    /// <summary>
    /// Dừng di chuyển
    /// </summary>
    public void StopMoving()
    {
        isPaused = true;
        UpdateSprite();
    }

    /// <summary>
    /// Đổi hướng di chuyển ngay lập tức
    /// </summary>
    public void ReverseDirection()
    {
        direction *= -1;
        UpdateSprite();
    }

    /// <summary>
    /// Reset về vị trí ban đầu
    /// </summary>
    public void ResetPosition()
    {
        transform.position = startPosition;
        currentDistance = 0f;
        direction = startMovingRight ? 1 : -1;
        animationTimer = 0f;
        runFrameIndex = 0;
        UpdateSprite();
    }

    /// <summary>
    /// Thiết lập tốc độ di chuyển mới
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    /// <summary>
    /// Thiết lập tốc độ animation mới
    /// </summary>
    public void SetAnimationSpeed(float newAnimationSpeed)
    {
        animationSpeed = newAnimationSpeed;
    }

    /// <summary>
    /// Thiết lập khoảng cách mới
    /// </summary>
    public void SetDistance(float newDistance)
    {
        distance = newDistance;
        // Đảm bảo currentDistance không vượt quá distance mới
        if (currentDistance > distance)
        {
            currentDistance = distance;
            direction = -1;
            UpdateSprite();
        }
    }

    // Tăng frame chạy, quay vòng khi đến cuối danh sách
    private void AdvanceRunFrame()
    {
        Sprite[] runSet = direction == 1 ? runSpritesRight : runSpritesLeft;
        if (runSet != null && runSet.Length > 0)
        {
            runFrameIndex = (runFrameIndex + 1) % runSet.Length;
        }
        else
        {
            runFrameIndex = 0;
        }
    }

    // Vẽ đường di chuyển trong Scene view để dễ chỉnh sửa
    void OnDrawGizmosSelected()
    {
        Vector3 start = useCurrentPositionAsStart && Application.isPlaying ? startPosition : transform.position;
        Vector3 end = start;
        end.x += distance * (startMovingRight ? 1 : -1);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.2f);
        Gizmos.DrawWireSphere(end, 0.2f);
    }
}

