using UnityEngine;
using UnityEngine.UI;

public class QuestionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questionPanel;
    public Text questionText;
    public Button[] optionButtons; // 4 buttons: A, B, C, D
    public Text[] optionTexts; // Text for each option

    [Header("Settings")]
    public int damageOnWrongAnswer = 10;

    private QuestionData currentQuestion;
    private GameObject obstacleObject; // Vật cản đang tương tác
    private HealthManager playerHealth;
    private Player player;
    private Font pixelFont;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        // Đảm bảo có EventSystem (cần thiết cho UI buttons)
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Tìm hoặc tạo Canvas nếu chưa có
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // Cao hơn health bar
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Tìm font VT323 hoặc dùng font mặc định
        pixelFont = FontHelper.GetVT323Font();

        // Tạo UI nếu chưa có
        if (questionPanel == null)
        {
            CreateQuestionUI(canvas.transform);
        }

        // Ẩn panel ban đầu
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Tìm Player
        player = FindObjectOfType<Player>();
        if (player != null)
        {
            if (player.healthManager != null)
            {
                playerHealth = player.healthManager;
            }
            else
            {
                // Tìm HealthManager trên player
                playerHealth = player.GetComponent<HealthManager>();
            }
        }
    }

    private void CreateQuestionUI(Transform parent)
    {
        // Tạo panel chính (nền trong suốt)
        GameObject panelObj = new GameObject("QuestionPanel");
        panelObj.transform.SetParent(parent, false);
        questionPanel = panelObj;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0); // Trong suốt

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // === BOX CÂU HỎI BÊN TRÁI (nền trắng, viền đen pixel) ===
        GameObject questionBoxObj = new GameObject("QuestionBox");
        questionBoxObj.transform.SetParent(panelObj.transform, false);
        
        Image questionBoxImage = questionBoxObj.AddComponent<Image>();
        questionBoxImage.color = Color.white; // Nền trắng
        
        RectTransform questionBoxRect = questionBoxObj.GetComponent<RectTransform>();
        questionBoxRect.anchorMin = new Vector2(0, 0);
        questionBoxRect.anchorMax = new Vector2(0.5f, 0.4f); // Chiếm phần dưới bên trái
        questionBoxRect.sizeDelta = Vector2.zero;
        questionBoxRect.anchoredPosition = Vector2.zero;
        questionBoxRect.offsetMin = new Vector2(10, 10);
        questionBoxRect.offsetMax = new Vector2(-5, -10);

        // Viền đen pixel cho box câu hỏi
        CreatePixelBorder(questionBoxObj.transform, 3);

        // Text câu hỏi
        GameObject questionObj = new GameObject("QuestionText");
        questionObj.transform.SetParent(questionBoxObj.transform, false);
        questionText = questionObj.AddComponent<Text>();
        questionText.text = "Câu hỏi";
        questionText.font = pixelFont;
        questionText.fontSize = 32;
        questionText.color = Color.black; // Màu đen
        questionText.alignment = TextAnchor.UpperLeft;
        questionText.raycastTarget = false;

        RectTransform questionRect = questionObj.GetComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0, 0);
        questionRect.anchorMax = new Vector2(1, 1);
        questionRect.sizeDelta = Vector2.zero;
        questionRect.anchoredPosition = Vector2.zero;
        questionRect.offsetMin = new Vector2(15, 15);
        questionRect.offsetMax = new Vector2(-15, -15);

        // === BOX ĐÁP ÁN BÊN PHẢI (nền trắng, viền đen pixel) ===
        GameObject answerBoxObj = new GameObject("AnswerBox");
        answerBoxObj.transform.SetParent(panelObj.transform, false);
        
        Image answerBoxImage = answerBoxObj.AddComponent<Image>();
        answerBoxImage.color = Color.white; // Nền trắng
        
        RectTransform answerBoxRect = answerBoxObj.GetComponent<RectTransform>();
        answerBoxRect.anchorMin = new Vector2(0.5f, 0);
        answerBoxRect.anchorMax = new Vector2(1, 0.4f); // Chiếm phần dưới bên phải
        answerBoxRect.sizeDelta = Vector2.zero;
        answerBoxRect.anchoredPosition = Vector2.zero;
        answerBoxRect.offsetMin = new Vector2(5, 10);
        answerBoxRect.offsetMax = new Vector2(-10, -10);

        // Viền đen pixel cho box đáp án
        CreatePixelBorder(answerBoxObj.transform, 3);

        // Tạo buttons cho các lựa chọn (2x2 grid)
        optionButtons = new Button[4];
        optionTexts = new Text[4];
        string[] labels = { "A", "B", "C", "D" };
        
        // Vị trí cho 2x2 grid (chiếm toàn bộ box đáp án)
        Vector2[] anchorMins = new Vector2[]
        {
            new Vector2(0.05f, 0.52f), // Top-left (A)
            new Vector2(0.52f, 0.52f), // Top-right (B)
            new Vector2(0.05f, 0.05f),  // Bottom-left (C)
            new Vector2(0.52f, 0.05f)  // Bottom-right (D)
        };
        
        Vector2[] anchorMaxs = new Vector2[]
        {
            new Vector2(0.48f, 0.95f), // Top-left (A)
            new Vector2(0.95f, 0.95f), // Top-right (B)
            new Vector2(0.48f, 0.48f),  // Bottom-left (C)
            new Vector2(0.95f, 0.48f)  // Bottom-right (D)
        };

        for (int i = 0; i < 4; i++)
        {
            // Button
            GameObject btnObj = new GameObject($"OptionButton{labels[i]}");
            btnObj.transform.SetParent(answerBoxObj.transform, false);
            
            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = Color.white; // Nền trắng
            btnImage.raycastTarget = true;
            
            Button btn = btnObj.AddComponent<Button>();
            btn.interactable = true;
            
            // Setup button colors
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            colors.selectedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            colors.disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            btn.colors = colors;
            
            // Viền đen pixel cho button
            CreatePixelBorder(btnObj.transform, 2);
            
            optionButtons[i] = btn;

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMins[i];
            btnRect.anchorMax = anchorMaxs[i];
            btnRect.sizeDelta = Vector2.zero;
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.offsetMin = new Vector2(5, 5);
            btnRect.offsetMax = new Vector2(-5, -5);

            // Text trong button
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "";
            text.font = pixelFont;
            text.fontSize = 28;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);

            optionTexts[i] = text;

            // Gán sự kiện click
            int index = i;
            btn.onClick.AddListener(() => OnOptionSelected(index));
        }
    }

    public void ShowQuestion(QuestionData question, GameObject obstacle)
    {
        if (question == null) return;

        currentQuestion = question;
        obstacleObject = obstacle;

        // Pause game
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Dừng player
        if (player != null && player.movement != null)
        {
            player.movement.enabled = false;
        }

        // Hiển thị câu hỏi
        questionText.text = question.question;

        // Hiển thị các lựa chọn
        for (int i = 0; i < 4 && i < question.options.Length; i++)
        {
            optionTexts[i].text = question.options[i];
            
            // Đảm bảo buttons có thể tương tác
            if (optionButtons[i] != null)
            {
                optionButtons[i].interactable = true;
            }
        }

        // Hiển thị panel
        questionPanel.SetActive(true);
        
        // Đảm bảo EventSystem hoạt động
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private void OnOptionSelected(int selectedIndex)
    {
        if (currentQuestion == null) return;

        bool isCorrect = selectedIndex == currentQuestion.correctAnswerIndex;

        if (isCorrect)
        {
            // Đúng: không mất máu, tiêu diệt vật cản
            if (obstacleObject != null)
            {
                Destroy(obstacleObject);
            }
            HideQuestion();
        }
        else
        {
            // Sai: mất máu, phải chọn lại
            if (playerHealth == null && player != null)
            {
                playerHealth = player.GetComponent<HealthManager>();
            }
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnWrongAnswer);
            }
            
            Debug.Log("Sai rồi! Hãy chọn lại. Bạn đã mất máu!");
            
            // Đổi màu button để báo hiệu sai
            StartCoroutine(FlashWrongButton(selectedIndex));
        }
    }

    private void HideQuestion()
    {
        questionPanel.SetActive(false);
        currentQuestion = null;
        obstacleObject = null;

        // Cho phép player di chuyển lại
        if (player != null && player.movement != null)
        {
            player.movement.enabled = true;
        }

        // Resume game
        Time.timeScale = previousTimeScale;
    }

    private System.Collections.IEnumerator FlashWrongButton(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= optionButtons.Length) yield break;

        Button btn = optionButtons[buttonIndex];
        Image btnImage = btn.GetComponent<Image>();
        Color originalColor = btnImage.color;

        // Đổi màu đỏ
        btnImage.color = Color.red;
        yield return new WaitForSeconds(0.3f);

        // Đổi lại màu gốc
        btnImage.color = originalColor;
    }

    // Tạo viền pixel (dạng chữ nhật đen xung quanh)
    private void CreatePixelBorder(Transform parent, int borderWidth)
    {
        RectTransform parentRect = parent.GetComponent<RectTransform>();
        if (parentRect == null) return;

        // Top border
        CreateBorderLine(parent, "TopBorder", new Vector2(0, 1), new Vector2(1, 1), borderWidth, true);
        // Bottom border
        CreateBorderLine(parent, "BottomBorder", new Vector2(0, 0), new Vector2(1, 0), borderWidth, true);
        // Left border
        CreateBorderLine(parent, "LeftBorder", new Vector2(0, 0), new Vector2(0, 1), borderWidth, false);
        // Right border
        CreateBorderLine(parent, "RightBorder", new Vector2(1, 0), new Vector2(1, 1), borderWidth, false);
    }

    private void CreateBorderLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int width, bool isHorizontal)
    {
        GameObject borderLine = new GameObject(name);
        borderLine.transform.SetParent(parent, false);
        
        Image borderImage = borderLine.AddComponent<Image>();
        borderImage.color = Color.black;
        borderImage.raycastTarget = false;
        
        RectTransform borderRect = borderLine.GetComponent<RectTransform>();
        borderRect.anchorMin = anchorMin;
        borderRect.anchorMax = anchorMax;
        
        if (isHorizontal)
        {
            // Top hoặc bottom border
            borderRect.sizeDelta = new Vector2(0, width);
            borderRect.anchoredPosition = Vector2.zero;
            if (anchorMin.y > 0.5f) // Top
            {
                borderRect.offsetMin = new Vector2(0, -width);
                borderRect.offsetMax = new Vector2(0, 0);
            }
            else // Bottom
            {
                borderRect.offsetMin = new Vector2(0, 0);
                borderRect.offsetMax = new Vector2(0, width);
            }
        }
        else
        {
            // Left hoặc right border
            borderRect.sizeDelta = new Vector2(width, 0);
            borderRect.anchoredPosition = Vector2.zero;
            if (anchorMin.x < 0.5f) // Left
            {
                borderRect.offsetMin = new Vector2(0, 0);
                borderRect.offsetMax = new Vector2(width, 0);
            }
            else // Right
            {
                borderRect.offsetMin = new Vector2(-width, 0);
                borderRect.offsetMax = new Vector2(0, 0);
            }
        }
    }
}
