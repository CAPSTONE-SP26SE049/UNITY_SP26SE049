using UnityEngine;

/// <summary>
/// Script cho vật cản có câu hỏi
/// Khi player đụng vào, sẽ mở form câu hỏi
/// </summary>
[DefaultExecutionOrder(-50)] // Chạy trước các script khác để ngăn damage
public class QuestionObstacle : MonoBehaviour
{
    [Header("Question Settings")]
    public bool isActive = true;
    
    // Public để các script khác có thể kiểm tra
    public bool HasQuestionObstacle => isActive;

    private QuestionUI questionUI;
    private QuestionManager questionManager;

    private void Start()
    {
        // Tắt các script có thể gây damage ngay từ đầu
        DisableDamageScripts();

        // Tìm QuestionUI
        questionUI = FindObjectOfType<QuestionUI>();
        if (questionUI == null)
        {
            // Tạo QuestionUI nếu chưa có
            GameObject uiObj = new GameObject("QuestionUI");
            questionUI = uiObj.AddComponent<QuestionUI>();
        }

        // Tìm QuestionManager
        questionManager = QuestionManager.Instance;
        if (questionManager == null)
        {
            GameObject managerObj = new GameObject("QuestionManager");
            questionManager = managerObj.AddComponent<QuestionManager>();
        }
    }

    private void DisableDamageScripts()
    {
        // Tắt Goomba nếu có
        Goomba goomba = GetComponent<Goomba>();
        if (goomba != null)
        {
            goomba.enabled = false;
        }

        // Tắt Koopa nếu có
        Koopa koopa = GetComponent<Koopa>();
        if (koopa != null)
        {
            koopa.enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Kiểm tra nếu player đụng từ phía trên (nhảy lên) thì không kích hoạt
            if (collision.transform.DotTest(transform, Vector2.down))
            {
                return; // Player đang đứng trên vật cản
            }

            // NGĂN CHẶN DAMAGE NGAY LẬP TỨC - phải làm trước khi các script khác xử lý
            PreventDamageImmediately();

            // Mở form câu hỏi
            ShowQuestion();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            // NGĂN CHẶN DAMAGE NGAY LẬP TỨC
            PreventDamageImmediately();

            // Mở form câu hỏi
            ShowQuestion();
        }
    }

    private void PreventDamageImmediately()
    {
        // Tắt các script gây damage NGAY LẬP TỨC
        Goomba goomba = GetComponent<Goomba>();
        if (goomba != null)
        {
            goomba.enabled = false;
        }

        Koopa koopa = GetComponent<Koopa>();
        if (koopa != null)
        {
            koopa.enabled = false;
        }

        // Tạm thời vô hiệu hóa collider để tránh trigger nhiều lần
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    private void PreventDamage()
    {
        PreventDamageImmediately();
    }

    private void ShowQuestion()
    {
        if (questionUI == null || questionManager == null) return;

        // Lấy câu hỏi ngẫu nhiên
        QuestionData question = questionManager.GetRandomQuestion();

        // Hiển thị form câu hỏi
        questionUI.ShowQuestion(question, gameObject);

        // Tạm thời vô hiệu hóa vật cản để tránh trigger nhiều lần
        isActive = false;
    }
}

