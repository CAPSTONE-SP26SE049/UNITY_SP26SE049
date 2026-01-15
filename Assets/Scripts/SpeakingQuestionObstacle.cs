using UnityEngine;

/// <summary>
/// Script cho vật cản có câu hỏi dạng speaking (phát âm)
/// Khi player đụng vào, sẽ mở form speaking question
/// </summary>
[DefaultExecutionOrder(-50)]
public class SpeakingQuestionObstacle : MonoBehaviour
{
    [Header("Question Settings")]
    public bool isActive = true;

    private SpeakingQuestionUI questionUI;
    private SpeakingQuestionManager questionManager;

    private void Start()
    {
        // Tắt các script có thể gây damage ngay từ đầu
        DisableDamageScripts();

        // Tìm SpeakingQuestionUI
        questionUI = FindObjectOfType<SpeakingQuestionUI>();
        if (questionUI == null)
        {
            GameObject uiObj = new GameObject("SpeakingQuestionUI");
            questionUI = uiObj.AddComponent<SpeakingQuestionUI>();
        }

        // Tìm SpeakingQuestionManager
        questionManager = SpeakingQuestionManager.Instance;
        if (questionManager == null)
        {
            GameObject managerObj = new GameObject("SpeakingQuestionManager");
            questionManager = managerObj.AddComponent<SpeakingQuestionManager>();
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

        SpeakingQuestionData question = questionManager.GetRandomQuestion();
        questionUI.ShowQuestion(question, gameObject);

        isActive = false;
    }
}

