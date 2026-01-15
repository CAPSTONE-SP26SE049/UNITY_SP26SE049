using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeakingQuestionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questionPanel;
    public Text questionText;
    public Text goalText;
    public Button recordButton;
    public Text recordButtonText;
    public Button stopButton;
    public Text stopButtonText;
    public Text recognizedTextDisplay;
    public Button submitButton;
    public Text submitButtonText;

    [Header("Settings")]
    public int damageOnWrongAnswer = 10;
    public int frequency = 16000; // Tần số ghi âm (Azure yêu cầu 16000 Hz)

    private SpeakingQuestionData currentQuestion;
    private GameObject obstacleObject;
    private HealthManager playerHealth;
    private Player player;
    private Font pixelFont;
    private float previousTimeScale = 1f;

    // Microphone recording
    private AudioClip recordingClip;
    private string microphoneDevice;
    private bool isRecording = false;
    private int maxRecordingLength = 30; // Tối đa 30 giây (đủ cho câu/từ ngắn)
    private string lastRecognizedText = "";

    // Animation
    private Coroutine recordingAnimationCoroutine;

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

        // Tìm microphone device
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            Debug.LogWarning("Không tìm thấy microphone!");
        }

        // Tạo UI nếu chưa có
        if (questionPanel == null)
        {
            CreateSpeakingUI(canvas.transform);
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

    private void CreateSpeakingUI(Transform parent)
    {
        // PANEL chính
        GameObject panelObj = new GameObject("SpeakingQuestionPanel");
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
        Sprite bgQuestion = Resources.Load<Sprite>("UI/backgroundquestion");
        if (bgQuestion != null)
        {
            questionBoxImage.sprite = bgQuestion;
            questionBoxImage.type = Image.Type.Sliced;
            questionBoxImage.color = Color.white;
        }
        else
        {
            questionBoxImage.color = new Color(1f, 1f, 1f, 0f);
        }

        RectTransform questionBoxRect = questionBoxObj.GetComponent<RectTransform>();
        questionBoxRect.anchorMin = new Vector2(0f, 0f);
        questionBoxRect.anchorMax = new Vector2(0.5f, 0.35f);
        questionBoxRect.sizeDelta = Vector2.zero;
        questionBoxRect.anchoredPosition = Vector2.zero;
        questionBoxRect.offsetMin = new Vector2(12, 12);
        questionBoxRect.offsetMax = new Vector2(-6, -12);

        // Header nhỏ
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(questionBoxObj.transform, false);
        Text headerText = headerObj.AddComponent<Text>();
        headerText.text = "HÃY ĐỌC TO CÂU/TỪ THEO YÊU CẦU";
        headerText.font = pixelFont;
        headerText.fontSize = 24;
        headerText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        headerText.alignment = TextAnchor.UpperLeft;
        headerText.raycastTarget = false;

        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 0.85f);
        headerRect.anchorMax = new Vector2(1f, 1f);
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
        questionText.fontSize = 32;
        questionText.color = Color.black;
        questionText.alignment = TextAnchor.MiddleCenter;
        questionText.raycastTarget = false;

        RectTransform questionRect = questionObj.GetComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0.08f, 0.45f);
        questionRect.anchorMax = new Vector2(0.92f, 0.85f);
        questionRect.sizeDelta = Vector2.zero;
        questionRect.anchoredPosition = Vector2.zero;
        questionRect.offsetMin = Vector2.zero;
        questionRect.offsetMax = Vector2.zero;

        // Text mục tiêu (goal)
        GameObject goalObj = new GameObject("GoalText");
        goalObj.transform.SetParent(questionBoxObj.transform, false);
        goalText = goalObj.AddComponent<Text>();
        goalText.text = "";
        goalText.font = pixelFont;
        goalText.fontSize = 24;
        goalText.color = new Color(0.3f, 0.3f, 0.7f, 1f);
        goalText.alignment = TextAnchor.MiddleCenter;
        goalText.raycastTarget = false;

        RectTransform goalRect = goalObj.GetComponent<RectTransform>();
        goalRect.anchorMin = new Vector2(0.08f, 0.25f);
        goalRect.anchorMax = new Vector2(0.92f, 0.45f);
        goalRect.sizeDelta = Vector2.zero;
        goalRect.anchoredPosition = Vector2.zero;
        goalRect.offsetMin = Vector2.zero;
        goalRect.offsetMax = Vector2.zero;

        // === BOX GHI ÂM BÊN PHẢI ===
        GameObject answerBoxObj = new GameObject("AnswerBox");
        answerBoxObj.transform.SetParent(panelObj.transform, false);

        Image answerBoxImage = answerBoxObj.AddComponent<Image>();
        Sprite bgAnswer = Resources.Load<Sprite>("UI/backgroundanswer");
        if (bgAnswer != null)
        {
            answerBoxImage.sprite = bgAnswer;
            answerBoxImage.type = Image.Type.Sliced;
            answerBoxImage.color = Color.white;
        }
        else
        {
            answerBoxImage.color = new Color(1f, 1f, 1f, 0f);
        }

        RectTransform answerBoxRect = answerBoxObj.GetComponent<RectTransform>();
        answerBoxRect.anchorMin = new Vector2(0.5f, 0f);
        answerBoxRect.anchorMax = new Vector2(1f, 0.35f);
        answerBoxRect.sizeDelta = Vector2.zero;
        answerBoxRect.anchoredPosition = Vector2.zero;
        answerBoxRect.offsetMin = new Vector2(6, 12);
        answerBoxRect.offsetMax = new Vector2(-12, -12);

        // Nút Bắt đầu ghi âm (phía trên)
        GameObject recordBtnObj = new GameObject("RecordButton");
        recordBtnObj.transform.SetParent(answerBoxObj.transform, false);

        Image recordBtnImage = recordBtnObj.AddComponent<Image>();
        recordBtnImage.color = new Color(1f, 1f, 1f, 0f);
        recordBtnImage.raycastTarget = true;

        recordButton = recordBtnObj.AddComponent<Button>();
        recordButton.interactable = true;

        ColorBlock recordColors = recordButton.colors;
        recordColors.normalColor = new Color(1f, 1f, 1f, 0f);
        recordColors.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
        recordColors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
        recordButton.colors = recordColors;

        CreatePixelBorder(recordBtnObj.transform, 2);

        RectTransform recordBtnRect = recordBtnObj.GetComponent<RectTransform>();
        recordBtnRect.anchorMin = new Vector2(0.08f, 0.65f);
        recordBtnRect.anchorMax = new Vector2(0.92f, 0.95f);
        recordBtnRect.sizeDelta = Vector2.zero;
        recordBtnRect.anchoredPosition = Vector2.zero;

        GameObject recordTextObj = new GameObject("Text");
        recordTextObj.transform.SetParent(recordBtnObj.transform, false);
        recordButtonText = recordTextObj.AddComponent<Text>();
        recordButtonText.text = "🎤 Bắt đầu ghi âm";
        recordButtonText.font = pixelFont;
        recordButtonText.fontSize = 32;
        recordButtonText.color = Color.black;
        recordButtonText.alignment = TextAnchor.MiddleCenter;
        recordButtonText.raycastTarget = false;

        RectTransform recordTextRect = recordButtonText.GetComponent<RectTransform>();
        recordTextRect.anchorMin = Vector2.zero;
        recordTextRect.anchorMax = Vector2.one;
        recordTextRect.sizeDelta = Vector2.zero;
        recordTextRect.anchoredPosition = Vector2.zero;

        recordButton.onClick.AddListener(StartRecording);

        // Nút Dừng ghi âm (giữa)
        GameObject stopBtnObj = new GameObject("StopButton");
        stopBtnObj.transform.SetParent(answerBoxObj.transform, false);

        Image stopBtnImage = stopBtnObj.AddComponent<Image>();
        stopBtnImage.color = new Color(1f, 1f, 1f, 0f);
        stopBtnImage.raycastTarget = true;

        stopButton = stopBtnObj.AddComponent<Button>();
        stopButton.interactable = false;

        ColorBlock stopColors = stopButton.colors;
        stopColors.normalColor = new Color(1f, 1f, 1f, 0f);
        stopColors.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
        stopColors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
        stopColors.disabledColor = new Color(1f, 1f, 1f, 0.05f);
        stopButton.colors = stopColors;

        CreatePixelBorder(stopBtnObj.transform, 2);

        RectTransform stopBtnRect = stopBtnObj.GetComponent<RectTransform>();
        stopBtnRect.anchorMin = new Vector2(0.08f, 0.35f);
        stopBtnRect.anchorMax = new Vector2(0.92f, 0.65f);
        stopBtnRect.sizeDelta = Vector2.zero;
        stopBtnRect.anchoredPosition = Vector2.zero;

        GameObject stopTextObj = new GameObject("Text");
        stopTextObj.transform.SetParent(stopBtnObj.transform, false);
        stopButtonText = stopTextObj.AddComponent<Text>();
        stopButtonText.text = "⏹ Dừng ghi âm";
        stopButtonText.font = pixelFont;
        stopButtonText.fontSize = 32;
        stopButtonText.color = Color.black;
        stopButtonText.alignment = TextAnchor.MiddleCenter;
        stopButtonText.raycastTarget = false;

        RectTransform stopTextRect = stopButtonText.GetComponent<RectTransform>();
        stopTextRect.anchorMin = Vector2.zero;
        stopTextRect.anchorMax = Vector2.one;
        stopTextRect.sizeDelta = Vector2.zero;
        stopTextRect.anchoredPosition = Vector2.zero;

        stopButton.onClick.AddListener(StopRecording);

        // Text hiển thị kết quả nhận diện (phía dưới, nhỏ hơn)
        GameObject recognizedObj = new GameObject("RecognizedText");
        recognizedObj.transform.SetParent(answerBoxObj.transform, false);
        recognizedTextDisplay = recognizedObj.AddComponent<Text>();
        recognizedTextDisplay.text = "Bạn đã nói: ...";
        recognizedTextDisplay.font = pixelFont;
        recognizedTextDisplay.fontSize = 24;
        recognizedTextDisplay.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        recognizedTextDisplay.alignment = TextAnchor.MiddleCenter;
        recognizedTextDisplay.raycastTarget = false;

        RectTransform recognizedRect = recognizedObj.GetComponent<RectTransform>();
        recognizedRect.anchorMin = new Vector2(0.08f, 0.08f);
        recognizedRect.anchorMax = new Vector2(0.92f, 0.35f);
        recognizedRect.sizeDelta = Vector2.zero;
        recognizedRect.anchoredPosition = Vector2.zero;
        recognizedRect.offsetMin = new Vector2(8, 4);
        recognizedRect.offsetMax = new Vector2(-8, -4);

        // Nút Nộp bài (ở dưới cùng, nhỏ)
        GameObject submitBtnObj = new GameObject("SubmitButton");
        submitBtnObj.transform.SetParent(answerBoxObj.transform, false);

        Image submitBtnImage = submitBtnObj.AddComponent<Image>();
        submitBtnImage.color = new Color(1f, 1f, 1f, 0f);
        submitBtnImage.raycastTarget = true;

        submitButton = submitBtnObj.AddComponent<Button>();
        submitButton.interactable = false;

        ColorBlock submitColors = submitButton.colors;
        submitColors.normalColor = new Color(1f, 1f, 1f, 0f);
        submitColors.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
        submitColors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
        submitColors.disabledColor = new Color(1f, 1f, 1f, 0.05f);
        submitButton.colors = submitColors;

        CreatePixelBorder(submitBtnObj.transform, 2);

        RectTransform submitBtnRect = submitBtnObj.GetComponent<RectTransform>();
        submitBtnRect.anchorMin = new Vector2(0.25f, 0f);
        submitBtnRect.anchorMax = new Vector2(0.75f, 0.08f);
        submitBtnRect.sizeDelta = Vector2.zero;
        submitBtnRect.anchoredPosition = Vector2.zero;

        GameObject submitTextObj = new GameObject("Text");
        submitTextObj.transform.SetParent(submitBtnObj.transform, false);
        submitButtonText = submitTextObj.AddComponent<Text>();
        submitButtonText.text = "Nộp bài";
        submitButtonText.font = pixelFont;
        submitButtonText.fontSize = 24;
        submitButtonText.color = Color.black;
        submitButtonText.alignment = TextAnchor.MiddleCenter;
        submitButtonText.raycastTarget = false;

        RectTransform submitTextRect = submitButtonText.GetComponent<RectTransform>();
        submitTextRect.anchorMin = Vector2.zero;
        submitTextRect.anchorMax = Vector2.one;
        submitTextRect.sizeDelta = Vector2.zero;
        submitTextRect.anchoredPosition = Vector2.zero;

        submitButton.onClick.AddListener(OnSubmitAnswer);
    }

    public void ShowQuestion(SpeakingQuestionData question, GameObject obstacle)
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

        // Hiển thị câu hỏi và mục tiêu
        questionText.text = question.question;
        goalText.text = "👉 " + question.goal;

        // Reset UI
        lastRecognizedText = "";
        recognizedTextDisplay.text = "Bạn đã nói: ...";
        recognizedTextDisplay.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Reset về màu xanh lá mặc định
        recordButton.interactable = true;
        recordButtonText.text = "🎤 Bắt đầu ghi âm";
        stopButton.interactable = false;
        submitButton.interactable = false;

        // Dừng recording nếu đang ghi
        if (isRecording)
        {
            StopRecording();
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

    private void StartRecording()
    {
        if (isRecording) return;

        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogError("Không có microphone!");
            return;
        }

        // Bắt đầu ghi âm
        recordingClip = Microphone.Start(microphoneDevice, false, maxRecordingLength, frequency);
        isRecording = true;

        // Cập nhật UI
        recordButton.interactable = false;
        recordButtonText.text = "🔴 Đang ghi âm...";
        stopButton.interactable = true;
        recognizedTextDisplay.text = "Đang ghi âm...";

        // Bắt đầu animation
        if (recordingAnimationCoroutine != null)
        {
            StopCoroutine(recordingAnimationCoroutine);
        }
        recordingAnimationCoroutine = StartCoroutine(RecordingAnimation());
    }

    private void StopRecording()
    {
        if (!isRecording) return;

        // Dừng ghi âm
        if (Microphone.IsRecording(microphoneDevice))
        {
            Microphone.End(microphoneDevice);
        }

        isRecording = false;

        // Dừng animation
        if (recordingAnimationCoroutine != null)
        {
            StopCoroutine(recordingAnimationCoroutine);
            recordingAnimationCoroutine = null;
        }

        // Reset button colors
        Image recordBtnImage = recordButton.GetComponent<Image>();
        recordBtnImage.color = new Color(1f, 1f, 1f, 0f);

        // Cập nhật UI
        recordButton.interactable = true;
        recordButtonText.text = "🎤 Bắt đầu ghi âm lại";
        stopButton.interactable = false;

        // Xử lý audio clip
        if (recordingClip != null)
        {
            ProcessRecording();
        }
    }

    private void ProcessRecording()
    {
        recognizedTextDisplay.text = "Đang xử lý...";

        // Xử lý nhanh hơn: Convert audio clip và nhận diện ngay
        StartCoroutine(ProcessRecordingCoroutine());
    }

    private IEnumerator ProcessRecordingCoroutine()
    {
        // Đợi một chút để đảm bảo audio clip đã sẵn sàng
        yield return new WaitForSecondsRealtime(0.1f);

        if (recordingClip == null)
        {
            recognizedTextDisplay.text = "Không có dữ liệu ghi âm. Vui lòng thử lại.";
            submitButton.interactable = false;
            yield break;
        }

        // Nhận diện giọng nói
        bool recognitionDone = false;
        string resultText = "";
        bool success = false;

        SpeechRecognitionHelper.Instance.RecognizeSpeech(recordingClip, (recognizedText, recSuccess) =>
        {
            resultText = recognizedText;
            success = recSuccess;
            recognitionDone = true;
        });

        // Đợi kết quả với timeout ngắn hơn
        float timeout = 3f; // Giảm timeout xuống 3 giây
        float elapsed = 0f;

        while (!recognitionDone && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            // Cập nhật UI để người dùng biết đang xử lý
            if (elapsed > 0.5f)
            {
                recognizedTextDisplay.text = "Đang xử lý" + new string('.', (int)(elapsed * 2) % 4);
            }
            yield return null;
        }

        if (success && !string.IsNullOrEmpty(resultText))
        {
            lastRecognizedText = resultText;
            recognizedTextDisplay.text = "Bạn đã nói: " + resultText;
            
            // Tự động kiểm tra và pass nếu đúng
            if (currentQuestion != null)
            {
                bool isCorrect = SpeechRecognitionHelper.Instance.CompareText(resultText, currentQuestion.targetText);
                
                if (isCorrect)
                {
                    // Đúng: tự động pass
                    recognizedTextDisplay.text = "✓ Đúng rồi! " + resultText;
                    recognizedTextDisplay.color = new Color(0f, 0.8f, 0f, 1f); // Màu xanh lá
                    
                    // Đợi một chút để người dùng thấy kết quả
                    yield return new WaitForSecondsRealtime(1f);
                    
                    // Tự động pass
                    if (obstacleObject != null)
                    {
                        Destroy(obstacleObject);
                    }
                    HideQuestion();
                    yield break;
                }
                else
                {
                    // Sai: hiển thị thông báo và cho phép thử lại
                    recognizedTextDisplay.text = "✗ Chưa đúng. Bạn đã nói: " + resultText + "\nHãy thử lại!";
                    recognizedTextDisplay.color = new Color(0.8f, 0f, 0f, 1f); // Màu đỏ
                    submitButton.interactable = false; // Không cần nút nộp bài nữa, chỉ cần ghi âm lại
                }
            }
            else
            {
                submitButton.interactable = true;
            }
        }
        else
        {
            recognizedTextDisplay.text = "Không nhận diện được. Vui lòng thử lại.";
            recognizedTextDisplay.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Màu xanh lá mặc định
            submitButton.interactable = false;
        }
    }

    private IEnumerator RecordingAnimation()
    {
        Image recordBtnImage = recordButton.GetComponent<Image>();
        float time = 0f;

        while (isRecording)
        {
            // Nhấp nháy màu đỏ
            float alpha = Mathf.PingPong(time * 2f, 1f);
            recordBtnImage.color = new Color(1f, 0f, 0f, alpha * 0.3f);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        recordBtnImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private void OnSubmitAnswer()
    {
        if (currentQuestion == null || string.IsNullOrEmpty(lastRecognizedText)) return;

        // So sánh với đáp án đúng
        bool isCorrect = SpeechRecognitionHelper.Instance.CompareText(lastRecognizedText, currentQuestion.targetText);

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
            // Sai: mất máu, phải thử lại
            if (playerHealth == null && player != null)
            {
                playerHealth = player.GetComponent<HealthManager>();
            }
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageOnWrongAnswer);
            }
            
            Debug.Log($"Sai rồi! Đáp án đúng là: \"{currentQuestion.targetText}\". Hãy thử lại!");
            
            // Reset để thử lại
            lastRecognizedText = "";
            recognizedTextDisplay.text = "Sai rồi! Hãy thử lại.";
            submitButton.interactable = false;
            
            // Flash button
            StartCoroutine(FlashWrongButton());
        }
    }

    private void HideQuestion()
    {
        // Dừng recording nếu đang ghi
        if (isRecording)
        {
            StopRecording();
        }

        questionPanel.SetActive(false);
        currentQuestion = null;
        obstacleObject = null;
        recordingClip = null;
        lastRecognizedText = "";

        // Cho phép player di chuyển lại
        if (player != null && player.movement != null)
        {
            player.movement.enabled = true;
        }

        // Resume game
        Time.timeScale = previousTimeScale;
    }

    private IEnumerator FlashWrongButton()
    {
        if (submitButton == null) yield break;

        Image btnImage = submitButton.GetComponent<Image>();
        Color originalColor = btnImage.color;

        btnImage.color = Color.red;
        yield return new WaitForSecondsRealtime(0.3f);

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

    private void OnDestroy()
    {
        // Dừng recording khi destroy
        if (isRecording)
        {
            StopRecording();
        }
    }
}

