using UnityEngine;
using UnityEngine.UI;

public class WritingQuestionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questionPanel;
    public Text questionText;
    public InputField answerInputField;
    public Button submitButton;
    public Text submitButtonText;

    [Header("Settings")]
    public int damageOnWrongAnswer = 10;

    private WritingQuestionData currentQuestion;
    private GameObject obstacleObject;
    private HealthManager playerHealth;
    private Player player;
    private Font pixelFont;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        // Đảm bảo có EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Tìm hoặc tạo Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Tìm font
        pixelFont = FontHelper.GetVT323Font();

        // Tạo UI nếu chưa có
        if (questionPanel == null)
        {
            CreateWritingUI(canvas.transform);
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
                playerHealth = player.GetComponent<HealthManager>();
            }
        }
    }

    private void CreateWritingUI(Transform parent)
    {
        // Tạo panel chính
        GameObject panelObj = new GameObject("WritingQuestionPanel");
        panelObj.transform.SetParent(parent, false);
        questionPanel = panelObj;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // === BOX CÂU HỎI BÊN TRÁI ===
        GameObject questionBoxObj = new GameObject("QuestionBox");
        questionBoxObj.transform.SetParent(panelObj.transform, false);
        
        Image questionBoxImage = questionBoxObj.AddComponent<Image>();
        questionBoxImage.color = Color.white;
        
        RectTransform questionBoxRect = questionBoxObj.GetComponent<RectTransform>();
        questionBoxRect.anchorMin = new Vector2(0, 0);
        questionBoxRect.anchorMax = new Vector2(0.5f, 0.4f);
        questionBoxRect.sizeDelta = Vector2.zero;
        questionBoxRect.anchoredPosition = Vector2.zero;
        questionBoxRect.offsetMin = new Vector2(10, 10);
        questionBoxRect.offsetMax = new Vector2(-5, -10);

        CreatePixelBorder(questionBoxObj.transform, 3);

        // Text câu hỏi
        GameObject questionObj = new GameObject("QuestionText");
        questionObj.transform.SetParent(questionBoxObj.transform, false);
        questionText = questionObj.AddComponent<Text>();
        questionText.text = "Câu hỏi";
        questionText.font = pixelFont;
        questionText.fontSize = 32;
        questionText.color = Color.black;
        questionText.alignment = TextAnchor.UpperLeft;
        questionText.raycastTarget = false;

        RectTransform questionRect = questionObj.GetComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0, 0);
        questionRect.anchorMax = new Vector2(1, 1);
        questionRect.sizeDelta = Vector2.zero;
        questionRect.anchoredPosition = Vector2.zero;
        questionRect.offsetMin = new Vector2(15, 15);
        questionRect.offsetMax = new Vector2(-15, -15);

        // === BOX NHẬP ĐÁP ÁN BÊN PHẢI ===
        GameObject answerBoxObj = new GameObject("AnswerBox");
        answerBoxObj.transform.SetParent(panelObj.transform, false);
        
        Image answerBoxImage = answerBoxObj.AddComponent<Image>();
        answerBoxImage.color = Color.white;
        
        RectTransform answerBoxRect = answerBoxObj.GetComponent<RectTransform>();
        answerBoxRect.anchorMin = new Vector2(0.5f, 0);
        answerBoxRect.anchorMax = new Vector2(1, 0.4f);
        answerBoxRect.sizeDelta = Vector2.zero;
        answerBoxRect.anchoredPosition = Vector2.zero;
        answerBoxRect.offsetMin = new Vector2(5, 10);
        answerBoxRect.offsetMax = new Vector2(-10, -10);

        CreatePixelBorder(answerBoxObj.transform, 3);

        // Input Field
        GameObject inputObj = new GameObject("AnswerInputField");
        inputObj.transform.SetParent(answerBoxObj.transform, false);
        
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = Color.white;
        
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.1f, 0.6f);
        inputRect.anchorMax = new Vector2(0.9f, 0.9f);
        inputRect.sizeDelta = Vector2.zero;
        inputRect.anchoredPosition = Vector2.zero;

        CreatePixelBorder(inputObj.transform, 2);

        answerInputField = inputObj.AddComponent<InputField>();
        answerInputField.targetGraphic = inputBg;
        answerInputField.characterLimit = 20;
        
        // Text component cho input
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.font = pixelFont;
        textComponent.fontSize = 28;
        textComponent.color = Color.black;
        textComponent.alignment = TextAnchor.MiddleLeft;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        
        answerInputField.textComponent = textComponent;
        
        // Placeholder text
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform, false);
        Text placeholderText = placeholderObj.AddComponent<Text>();
        placeholderText.text = "Nhập đáp án...";
        placeholderText.font = pixelFont;
        placeholderText.fontSize = 28;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.alignment = TextAnchor.MiddleLeft;
        
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.anchoredPosition = Vector2.zero;
        placeholderRect.offsetMin = new Vector2(10, 5);
        placeholderRect.offsetMax = new Vector2(-10, -5);
        
        answerInputField.placeholder = placeholderText;

        // Submit Button
        GameObject submitBtnObj = new GameObject("SubmitButton");
        submitBtnObj.transform.SetParent(answerBoxObj.transform, false);
        
        Image submitBtnImage = submitBtnObj.AddComponent<Image>();
        submitBtnImage.color = Color.white;
        submitBtnImage.raycastTarget = true;
        
        submitButton = submitBtnObj.AddComponent<Button>();
        submitButton.interactable = true;
        
        ColorBlock submitColors = submitButton.colors;
        submitColors.normalColor = Color.white;
        submitColors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        submitColors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        submitButton.colors = submitColors;
        
        CreatePixelBorder(submitBtnObj.transform, 2);

        RectTransform submitBtnRect = submitBtnObj.GetComponent<RectTransform>();
        submitBtnRect.anchorMin = new Vector2(0.2f, 0.1f);
        submitBtnRect.anchorMax = new Vector2(0.8f, 0.5f);
        submitBtnRect.sizeDelta = Vector2.zero;
        submitBtnRect.anchoredPosition = Vector2.zero;

        // Text cho button (tạo child)
        GameObject submitTextObj = new GameObject("Text");
        submitTextObj.transform.SetParent(submitBtnObj.transform, false);
        submitButtonText = submitTextObj.AddComponent<Text>();
        submitButtonText.text = "Nộp bài";
        submitButtonText.font = pixelFont;
        submitButtonText.fontSize = 28;
        submitButtonText.color = Color.black;
        submitButtonText.alignment = TextAnchor.MiddleCenter;
        submitButtonText.raycastTarget = false;

        RectTransform submitTextRect = submitTextObj.GetComponent<RectTransform>();
        submitTextRect.anchorMin = Vector2.zero;
        submitTextRect.anchorMax = Vector2.one;
        submitTextRect.sizeDelta = Vector2.zero;
        submitTextRect.anchoredPosition = Vector2.zero;

        submitButton.onClick.AddListener(OnSubmitAnswer);
    }

    public void ShowQuestion(WritingQuestionData question, GameObject obstacle)
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

        // Xóa input
        if (answerInputField != null)
        {
            answerInputField.text = "";
            answerInputField.ActivateInputField();
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

    private void OnSubmitAnswer()
    {
        if (currentQuestion == null || answerInputField == null) return;

        string userAnswer = answerInputField.text.Trim().ToLower();
        string correctAnswer = currentQuestion.correctAnswer.Trim().ToLower();

        bool isCorrect = userAnswer == correctAnswer;

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
            // Sai: mất máu, phải nhập lại
            if (playerHealth == null && player != null)
            {
                playerHealth = player.GetComponent<HealthManager>();
            }
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnWrongAnswer);
            }
            
            Debug.Log($"Sai rồi! Đáp án đúng là: {currentQuestion.correctAnswer}. Hãy nhập lại!");
            
            // Xóa input và focus lại
            answerInputField.text = "";
            answerInputField.ActivateInputField();
            
            // Flash button
            StartCoroutine(FlashWrongButton());
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

    private System.Collections.IEnumerator FlashWrongButton()
    {
        if (submitButton == null) yield break;

        Image btnImage = submitButton.GetComponent<Image>();
        Color originalColor = btnImage.color;

        btnImage.color = Color.red;
        yield return new WaitForSeconds(0.3f);

        btnImage.color = originalColor;
    }

    // Tạo viền pixel
    private void CreatePixelBorder(Transform parent, int borderWidth)
    {
        RectTransform parentRect = parent.GetComponent<RectTransform>();
        if (parentRect == null) return;

        CreateBorderLine(parent, "TopBorder", new Vector2(0, 1), new Vector2(1, 1), borderWidth, true);
        CreateBorderLine(parent, "BottomBorder", new Vector2(0, 0), new Vector2(1, 0), borderWidth, true);
        CreateBorderLine(parent, "LeftBorder", new Vector2(0, 0), new Vector2(0, 1), borderWidth, false);
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
            borderRect.sizeDelta = new Vector2(0, width);
            borderRect.anchoredPosition = Vector2.zero;
            if (anchorMin.y > 0.5f)
            {
                borderRect.offsetMin = new Vector2(0, -width);
                borderRect.offsetMax = new Vector2(0, 0);
            }
            else
            {
                borderRect.offsetMin = new Vector2(0, 0);
                borderRect.offsetMax = new Vector2(0, width);
            }
        }
        else
        {
            borderRect.sizeDelta = new Vector2(width, 0);
            borderRect.anchoredPosition = Vector2.zero;
            if (anchorMin.x < 0.5f)
            {
                borderRect.offsetMin = new Vector2(0, 0);
                borderRect.offsetMax = new Vector2(width, 0);
            }
            else
            {
                borderRect.offsetMin = new Vector2(-width, 0);
                borderRect.offsetMax = new Vector2(0, 0);
            }
        }
    }
}

