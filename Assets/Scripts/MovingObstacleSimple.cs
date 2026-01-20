using UnityEngine;

/// <summary>
/// Phiên bản đơn giản hơn - sử dụng Mathf.PingPong để di chuyển qua lại
/// Component làm cho vật cản di chuyển qua lại trái phải một cách mượt mà
/// </summary>
public class MovingObstacleSimple : MonoBehaviour
{
    [Header("Thiết lập di chuyển")]
    [Tooltip("Tốc độ di chuyển (đơn vị: units/giây)")]
    public float speed = 2f;
    
    [Tooltip("Khoảng cách di chuyển từ điểm xuất phát (đơn vị: units)")]
    public float distance = 5f;
    
    [Header("Tùy chọn")]
    [Tooltip("Tạm dừng di chuyển")]
    public bool isPaused = false;
    
    [Tooltip("Sử dụng vị trí hiện tại làm điểm xuất phát")]
    public bool useCurrentPositionAsStart = true;
    
    // Vị trí điểm xuất phát
    private Vector3 startPosition;
    
    // Thời gian đã trôi qua để tính toán PingPong
    private float timeElapsed = 0f;

    void Start()
    {
        // Lưu vị trí ban đầu
        startPosition = transform.position;
    }

    void Update()
    {
        // Nếu đang tạm dừng, không di chuyển
        if (isPaused)
            return;
        
        // Tăng thời gian đã trôi qua
        timeElapsed += Time.deltaTime;
        
        // Sử dụng PingPong để tạo chuyển động qua lại (0 đến distance)
        float pingPongValue = Mathf.PingPong(timeElapsed * speed, distance);
        
        // Cập nhật vị trí mới
        Vector3 newPosition = startPosition;
        newPosition.x += pingPongValue;
        transform.position = newPosition;
    }

    /// <summary>
    /// Bắt đầu di chuyển
    /// </summary>
    public void StartMoving()
    {
        isPaused = false;
    }

    /// <summary>
    /// Dừng di chuyển
    /// </summary>
    public void StopMoving()
    {
        isPaused = true;
    }

    /// <summary>
    /// Reset về vị trí ban đầu
    /// </summary>
    public void ResetPosition()
    {
        transform.position = startPosition;
        timeElapsed = 0f;
    }

    // Vẽ đường di chuyển trong Scene view để dễ chỉnh sửa
    void OnDrawGizmosSelected()
    {
        Vector3 start = useCurrentPositionAsStart && Application.isPlaying ? startPosition : transform.position;
        Vector3 end = start;
        end.x += distance;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.2f);
        Gizmos.DrawWireSphere(end, 0.2f);
    }
}

