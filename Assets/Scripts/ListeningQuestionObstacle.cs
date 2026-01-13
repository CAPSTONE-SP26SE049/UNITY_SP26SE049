using UnityEngine;

/// <summary>
/// Script cho vật cản có câu hỏi dạng listening (nghe và chọn đáp án)
/// Khi player đụng vào, sẽ mở form listening question
/// </summary>
[DefaultExecutionOrder(-50)]
public class ListeningQuestionObstacle : MonoBehaviour
{
    [Header("Question Settings")]
    public bool isActive = true;

    private ListeningQuestionUI questionUI;
    private ListeningQuestionManager questionManager;

    private void Start()
    {
        // Tắt các script có thể gây damage ngay từ đầu
        DisableDamageScripts();

        // Tìm ListeningQuestionUI
        questionUI = FindObjectOfType<ListeningQuestionUI>();
        if (questionUI == null)
        {
            GameObject uiObj = new GameObject("ListeningQuestionUI");
            questionUI = uiObj.AddComponent<ListeningQuestionUI>();
        }

        // Tìm ListeningQuestionManager
        questionManager = ListeningQuestionManager.Instance;
        if (questionManager == null)
        {
            GameObject managerObj = new GameObject("ListeningQuestionManager");
            questionManager = managerObj.AddComponent<ListeningQuestionManager>();
        }
    }

    private void DisableDamageScripts()
    {
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.transform.DotTest(transform, Vector2.down))
            {
                return;
            }

            PreventDamageImmediately();
            ShowQuestion();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            PreventDamageImmediately();
            ShowQuestion();
        }
    }

    private void PreventDamageImmediately()
    {
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

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    private void ShowQuestion()
    {
        if (questionUI == null || questionManager == null) return;

        ListeningQuestionData question = questionManager.GetRandomQuestion();
        questionUI.ShowQuestion(question, gameObject);

        isActive = false;
    }
}

