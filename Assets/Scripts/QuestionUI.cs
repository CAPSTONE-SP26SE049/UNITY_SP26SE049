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
        // PANEL CHÍNH (trong suốt, full màn)
        GameObject panelObj = new GameObject("QuestionPanel");
        panelObj.transform.SetParent(parent, false);
        questionPanel = panelObj;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0); // trong suốt

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // === BOX CÂU HỎI BÊN TRÁI ===
        GameObject questionBoxObj = new GameObject("QuestionBox");
        questionBoxObj.transform.SetParent(panelObj.transform, false);

        Image questionBoxImage = questionBoxObj.AddComponent<Image>();
        // Load sprite background từ Resources
        Sprite bgSprite = Resources.Load<Sprite>("UI/backgroundquestion");
        if (bgSprite != null)
        {
            questionBoxImage.sprite = bgSprite;
            questionBoxImage.type = Image.Type.Sliced; // Dùng Sliced để giữ tỷ lệ khi scale
            questionBoxImage.color = Color.white; // Màu trắng để sprite hiển thị đúng
        }
        else
        {
            // Nếu không tìm thấy sprite, giữ trong suốt
            questionBoxImage.color = new Color(1f, 1f, 1f, 0f);
        }

        RectTransform questionBoxRect = questionBoxObj.GetComponent<RectTransform>();
        // Căn đều với AnswerBox, dịch cụm UI xuống dưới và cho QuestionBox to vừa chạm AnswerBox (không đè lên)
        questionBoxRect.anchorMin = new Vector2(0.03f, 0.02f);
        questionBoxRect.anchorMax = new Vector2(0.495f, 0.42f);
        questionBoxRect.sizeDelta = Vector2.zero;
        questionBoxRect.anchoredPosition = Vector2.zero;
        // Margin đều bên trong vùng anchor
        questionBoxRect.offsetMin = new Vector2(20, 20);
        questionBoxRect.offsetMax = new Vector2(-20, -20);

        // Không tạo viền đen cho QuestionBox để tránh khung đen xung quanh background

        // Header nhỏ: "READING" (nằm phía trên khung gỗ)
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(questionBoxObj.transform, false);
        Text headerText = headerObj.AddComponent<Text>();
        headerText.text = "HÃY ĐỌC VÀ TRẢ LỜI CÂU HỎI DƯỚI ĐÂY";
        headerText.font = pixelFont;
        headerText.fontSize = 24;
        headerText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        headerText.alignment = TextAnchor.UpperLeft;
        headerText.raycastTarget = false;

        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1.1f);
        headerRect.sizeDelta = Vector2.zero;
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.offsetMin = new Vector2(16, 8);
        headerRect.offsetMax = new Vector2(-16, -4);

        // Text câu hỏi
        GameObject questionObj = new GameObject("QuestionText");
        questionObj.transform.SetParent(questionBoxObj.transform, false);
        questionText = questionObj.AddComponent<Text>();
        questionText.text = "Câu hỏi";
        questionText.font = pixelFont;
        // Giảm size một chút để vừa khung gỗ
        questionText.fontSize = 32;
        questionText.color = Color.black;
        // Căn giữa để text nằm trong lòng khung
        questionText.alignment = TextAnchor.MiddleCenter;
        questionText.raycastTarget = false;

        RectTransform questionRect = questionObj.GetComponent<RectTransform>();
        // Chiếm gần hết phần trong của background, chừa viền gỗ xung quanh
        questionRect.anchorMin = new Vector2(0.08f, 0.2f);
        questionRect.anchorMax = new Vector2(0.92f, 0.8f);
        questionRect.sizeDelta = Vector2.zero;
        questionRect.anchoredPosition = Vector2.zero;
        questionRect.offsetMin = Vector2.zero;
        questionRect.offsetMax = Vector2.zero;

        // === BOX ĐÁP ÁN BÊN PHẢI ===
        GameObject answerBoxObj = new GameObject("AnswerBox");
        answerBoxObj.transform.SetParent(panelObj.transform, false);

        Image answerBoxImage = answerBoxObj.AddComponent<Image>();
        // Load sprite background riêng cho phần đáp án
        Sprite bgSpriteAnswer = Resources.Load<Sprite>("UI/backgroundanswer");
        if (bgSpriteAnswer != null)
        {
            answerBoxImage.sprite = bgSpriteAnswer;
            answerBoxImage.type = Image.Type.Sliced; // Dùng Sliced để giữ tỷ lệ khi scale
            answerBoxImage.color = Color.white; // Màu trắng để sprite hiển thị đúng
        }
        else
        {
            // Nếu không tìm thấy sprite, giữ trong suốt
            answerBoxImage.color = new Color(1f, 1f, 1f, 0f);
        }

        RectTransform answerBoxRect = answerBoxObj.GetComponent<RectTransform>();
        // Đặt đối xứng với QuestionBox bên phải, giữ một khoảng hở rất nhỏ để không chồng lên
        answerBoxRect.anchorMin = new Vector2(0.505f, 0.02f);
        answerBoxRect.anchorMax = new Vector2(0.97f, 0.42f);
        answerBoxRect.sizeDelta = Vector2.zero;
        answerBoxRect.anchoredPosition = Vector2.zero;
        answerBoxRect.offsetMin = new Vector2(20, 20);
        answerBoxRect.offsetMax = new Vector2(-20, -20);

        // Xóa viền ngoài của AnswerBox, chỉ giữ viền của từng button A, B, C, D
        // CreatePixelBorder(answerBoxObj.transform, 3);

        // Buttons đáp án (2x2)
        optionButtons = new Button[4];
        optionTexts = new Text[4];
        string[] labels = { "A", "B", "C", "D" };

        Vector2[] anchorMins = new Vector2[]
        {
            new Vector2(0.05f, 0.55f), // A: trên/trái
            new Vector2(0.55f, 0.55f), // B: trên/phải
            new Vector2(0.05f, 0.05f), // C: dưới/trái
            new Vector2(0.55f, 0.05f)  // D: dưới/phải
        };

        Vector2[] anchorMaxs = new Vector2[]
        {
            new Vector2(0.48f, 0.95f),
            new Vector2(0.95f, 0.95f),
            new Vector2(0.48f, 0.45f),
            new Vector2(0.95f, 0.45f)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject btnObj = new GameObject($"OptionButton{labels[i]}");
            btnObj.transform.SetParent(answerBoxObj.transform, false);

            Image btnImage = btnObj.AddComponent<Image>();
            // Xóa nền trắng của button (giữ lại viền pixel)
            btnImage.color = new Color(1f, 1f, 1f, 0f);
            btnImage.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();
            btn.interactable = true;

            ColorBlock colors = btn.colors;
            // Giữ phản hồi tương tác bằng cách đổi alpha nhẹ khi hover/press
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0.12f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.05f);
            btn.colors = colors;

            // Giữ lại viền pixel cho từng button A, B, C, D
            CreatePixelBorder(btnObj.transform, 2);

            optionButtons[i] = btn;

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = anchorMins[i];
            btnRect.anchorMax = anchorMaxs[i];
            btnRect.sizeDelta = Vector2.zero;
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.offsetMin = new Vector2(6, 4);
            btnRect.offsetMax = new Vector2(-6, -4);

            // Text trong button: hiển thị nội dung đáp án, label A/B/C/D sẽ gắn khi ShowQuestion
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "";
            text.font = pixelFont;
            text.fontSize = 32;
            text.color = Color.black;
            // Căn giữa text đáp án trong khung
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            // Đưa text vào bên trong khung với padding đều
            textRect.offsetMin = new Vector2(12, 8);
            textRect.offsetMax = new Vector2(-12, -8);

            optionTexts[i] = text;

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
