using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MainCharMovement : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Collider2D capsuleCollider;

    private Vector2 velocity;
    private float inputAxis;
    private float jumpBufferTime = 0f;
    private const float jumpBufferDuration = 0.1f; // Thời gian buffer cho jump input

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);
    public bool falling => velocity.y < 0f && !grounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Lấy camera trong Start để đảm bảo nó đã được khởi tạo
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        rb.isKinematic = false;
        capsuleCollider.enabled = true;
        velocity = Vector2.zero;
        jumping = false;
    }

    private void OnDisable()
    {
        rb.isKinematic = true;
        capsuleCollider.enabled = false;
        velocity = Vector2.zero;
        inputAxis = 0f;
        jumping = false;
    }

    private void Update()
    {
        // Kiểm tra null để tránh lỗi
        if (rb == null) return;

        HorizontalMovement();

        grounded = rb.Raycast(Vector2.down);

        // Xử lý jump input - luôn kiểm tra ngay cả khi không grounded
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTime = jumpBufferDuration;
            
            // Nhảy ngay nếu đang grounded và không đang nhảy
            if (grounded && !jumping && velocity.y <= 0.1f)
            {
                velocity.y = jumpForce;
                jumping = true;
                jumpBufferTime = 0f;
            }
        }
        else
        {
            jumpBufferTime -= Time.deltaTime;
            if (jumpBufferTime < 0f)
            {
                jumpBufferTime = 0f;
            }
        }

        if (grounded) {
            GroundedMovement();
        }
        else
        {
            // Xử lý jump buffer khi không grounded (coyote time)
            if (jumpBufferTime > 0f && !jumping && velocity.y <= 0.1f)
            {
                velocity.y = jumpForce;
                jumping = true;
                jumpBufferTime = 0f;
            }
        }

        ApplyGravity();
    }

    private void FixedUpdate()
    {
        // Kiểm tra null để tránh lỗi
        if (rb == null) return;

        // Move main character based on velocity
        Vector2 position = rb.position;
        position += velocity * Time.fixedDeltaTime;

        // Đảm bảo nhân vật chạm đất khi grounded và không đang nhảy
        if (grounded && velocity.y <= 0f && !jumping)
        {
            // Kiểm tra lại grounded và điều chỉnh position nếu cần
            if (rb.Raycast(Vector2.down))
            {
                // Đảm bảo velocity.y không âm khi đã chạm đất
                velocity.y = 0f;
            }
        }

        // Clamp within the screen bounds (chỉ khi có camera)
        if (mainCamera != null)
        {
            Vector2 leftEdge = mainCamera.ScreenToWorldPoint(Vector2.zero);
            Vector2 rightEdge = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);
        }

        rb.MovePosition(position);
    }

    private void HorizontalMovement()
    {
        // Accelerate / decelerate
        inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * moveSpeed, moveSpeed * Time.deltaTime);

        // Check if running into a wall
        if (rb.Raycast(Vector2.right * velocity.x)) {
            velocity.x = 0f;
        }

        // Flip sprite to face direction
        if (velocity.x > 0f) {
            transform.eulerAngles = Vector3.zero;
        } else if (velocity.x < 0f) {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void GroundedMovement()
    {
        // Prevent gravity from infinitly building up
        velocity.y = Mathf.Max(velocity.y, 0f);
        
        // Reset jumping state khi đã chạm đất và không còn velocity lên
        if (velocity.y <= 0f)
        {
            jumping = false;
        }

        // Perform jump - sử dụng jump buffer để mượt mà hơn (nếu chưa nhảy trong Update)
        if (jumpBufferTime > 0f && !jumping)
        {
            velocity.y = jumpForce;
            jumping = true;
            jumpBufferTime = 0f; // Reset buffer sau khi nhảy
        }
    }

    private void ApplyGravity()
    {
        // Update jumping state dựa trên velocity (khi không grounded)
        if (!grounded)
        {
            jumping = velocity.y > 0.1f;
        }

        // Check if falling
        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = falling ? 2f : 1f;

        // Apply gravity and terminal velocity
        velocity.y += gravity * multiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Bounce off enemy head
            if (transform.DotTest(collision.transform, Vector2.down))
            {
                velocity.y = jumpForce / 2f;
                jumping = true;
            }
        }
        else if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
        {
            // Stop vertical movement if main character bonks head
            if (transform.DotTest(collision.transform, Vector2.up)) {
                velocity.y = 0f;
            }
        }
    }

}
