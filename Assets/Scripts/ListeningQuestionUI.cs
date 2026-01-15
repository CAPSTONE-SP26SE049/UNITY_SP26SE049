using UnityEngine;
using UnityEngine.UI;

public class ListeningQuestionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject questionPanel;
    public Text questionText;
    public Button playButton;
    public Text playButtonText;
    public Button[] optionButtons; // 4 buttons: A, B, C, D
    public Text[] optionTexts; // Text for each option

    [Header("Settings")]
    public int damageOnWrongAnswer = 10;

    private ListeningQuestionData currentQuestion;
    private GameObject obstacleObject;
    private HealthManager playerHealth;
    private Player player;
    private Font pixelFont;
    private AudioSource audioSource;
    private AudioClip currentAudioClip;
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

        // Tạo AudioSource để phát audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Tạo UI nếu chưa có
        if (questionPanel == null)
        {
            CreateListeningUI(canvas.transform);
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

    private void CreateListeningUI(Transform parent)
    {
        // PANEL chính
        GameObject panelObj = new GameObject("ListeningQuestionPanel");
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
        // Dùng background giống QuestionUI
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

        // Không tạo viền pixel đen quanh khung nữa

        // Header nhỏ
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(questionBoxObj.transform, false);
        Text headerText = headerObj.AddComponent<Text>();
        headerText.text = "HÃY NGHE VÀ CHỌN ĐÁP ÁN ĐÚNG";
        headerText.font = pixelFont;
        headerText.fontSize = 24;
        headerText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        headerText.alignment = TextAnchor.UpperLeft;
        headerText.raycastTarget = false;

        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 0.75f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.sizeDelta = Vector2.zero;
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.offsetMin = new Vector2(16, 8);
        headerRect.offsetMax = new Vector2(-16, -4);

        // Text câu hỏi
        GameObject questionObj = new GameObject("QuestionText");
        questionObj.transform.SetParent(questionBoxObj.transform, false);
        questionText = questionObj.AddComponent<Text>();
        questionText.text = "Nghe câu hỏi";
        questionText.font = pixelFont;
        // Đồng bộ style với QuestionUI
        questionText.fontSize = 32;
        questionText.color = Color.black;
        // Căn giữa trong khung gỗ
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

        // Nút Play Audio
        GameObject playBtnObj = new GameObject("PlayButton");
        playBtnObj.transform.SetParent(questionBoxObj.transform, false);

        Image playBtnImage = playBtnObj.AddComponent<Image>();
        // Xóa nền trắng nút Play, chỉ để border trong background gỗ
        playBtnImage.color = new Color(1f, 1f, 1f, 0f);
        playBtnImage.raycastTarget = true;

        playButton = playBtnObj.AddComponent<Button>();
        playButton.interactable = true;

        ColorBlock playColors = playButton.colors;
        playColors.normalColor = new Color(1f, 1f, 1f, 0f);
        playColors.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
        playColors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
        playButton.colors = playColors;

        CreatePixelBorder(playBtnObj.transform, 2);

        RectTransform playBtnRect = playBtnObj.GetComponent<RectTransform>();
        playBtnRect.anchorMin = new Vector2(0.15f, 0.05f);
        playBtnRect.anchorMax = new Vector2(0.85f, 0.32f);
        playBtnRect.sizeDelta = Vector2.zero;
        playBtnRect.anchoredPosition = Vector2.zero;

        GameObject playTextObj = new GameObject("Text");
        playTextObj.transform.SetParent(playBtnObj.transform, false);
        playButtonText = playTextObj.AddComponent<Text>();
        playButtonText.text = "🔊 Nghe";
        playButtonText.font = pixelFont;
        playButtonText.fontSize = 32;
        playButtonText.color = Color.black;
        playButtonText.alignment = TextAnchor.MiddleCenter;
        playButtonText.raycastTarget = false;

        RectTransform playTextRect = playButtonText.GetComponent<RectTransform>();
        playTextRect.anchorMin = Vector2.zero;
        playTextRect.anchorMax = Vector2.one;
        playTextRect.sizeDelta = Vector2.zero;
        playTextRect.anchoredPosition = Vector2.zero;

        playButton.onClick.AddListener(PlayAudio);

        // === BOX ĐÁP ÁN BÊN PHẢI ===
        GameObject answerBoxObj = new GameObject("AnswerBox");
        answerBoxObj.transform.SetParent(panelObj.transform, false);

        Image answerBoxImage = answerBoxObj.AddComponent<Image>();
        // Dùng background giống AnswerBox trong QuestionUI
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

        // Không tạo viền pixel đen cho AnswerBox nữa

        // Buttons đáp án
        optionButtons = new Button[4];
        optionTexts = new Text[4];
        string[] labels = { "A", "B", "C", "D" };

        Vector2[] anchorMins = new Vector2[]
        {
            new Vector2(0.05f, 0.55f),
            new Vector2(0.55f, 0.55f),
            new Vector2(0.05f, 0.05f),
            new Vector2(0.55f, 0.05f)
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
            // Xóa nền trắng của button, dùng nền trong suốt
            btnImage.color = new Color(1f, 1f, 1f, 0f);
            btnImage.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();
            btn.interactable = true;

            ColorBlock colors = btn.colors;
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

    public void ShowQuestion(ListeningQuestionData question, GameObject obstacle)
    {
        if (question == null) return;

        currentQuestion = question;
        obstacleObject = obstacle;

        // Dừng game
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Dừng player
        if (player != null && player.movement != null)
        {
            player.movement.enabled = false;
        }

        // Hiển thị câu hỏi (câu đầy đủ có chỗ trống)
        questionText.text = question.sentence;

        // Load audio clip
        LoadAudioClip(question.audioFileName);

        // Hiển thị các lựa chọn
        for (int i = 0; i < 4 && i < question.options.Length; i++)
        {
            string[] labels = { "A", "B", "C", "D" };
            optionTexts[i].text = $"{labels[i]}. {question.options[i]}";
            
            if (optionButtons[i] != null)
            {
                optionButtons[i].interactable = true;
            }
        }

        // Reset play button
        playButtonText.text = "🔊 Nghe";
        playButton.interactable = true;

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

    private void LoadAudioClip(string audioFileName)
    {
        // Load audio từ Resources/Audio/Listening/
        string path = $"Audio/Listening/{audioFileName}";
        currentAudioClip = Resources.Load<AudioClip>(path);
        
        if (currentAudioClip == null)
        {
            Debug.LogWarning($"Không tìm thấy file audio: {path}");
        }
    }

    private void PlayAudio()
    {
        if (currentAudioClip == null)
        {
            Debug.LogWarning("Không có audio clip để phát!");
            return;
        }

        // Phát audio (vẫn hoạt động khi Time.timeScale = 0)
        audioSource.clip = currentAudioClip;
        audioSource.Play();
        
        // Đổi text button thành "Đang phát..."
        playButtonText.text = "▶ Đang phát...";
        playButton.interactable = false;

        // Sau khi phát xong, đổi lại text
        StartCoroutine(ResetPlayButtonAfterAudio());
    }

    private System.Collections.IEnumerator ResetPlayButtonAfterAudio()
    {
        // Đợi đến khi audio phát xong
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        playButtonText.text = "🔊 Nghe lại";
        playButton.interactable = true;
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
            
            Debug.Log($"Sai rồi! Đáp án đúng là: {GetAnswerLabel(currentQuestion.correctAnswerIndex)}. {currentQuestion.options[currentQuestion.correctAnswerIndex]}. Hãy chọn lại!");
            
            // Flash button
            StartCoroutine(FlashWrongButton(selectedIndex));
        }
    }

    private string GetAnswerLabel(int index)
    {
        string[] labels = { "A", "B", "C", "D" };
        return index >= 0 && index < labels.Length ? labels[index] : "";
    }

    private void HideQuestion()
    {
        // Dừng audio nếu đang phát
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Khôi phục time scale
        Time.timeScale = previousTimeScale;

        questionPanel.SetActive(false);
        currentQuestion = null;
        obstacleObject = null;
        currentAudioClip = null;

        // Cho phép player di chuyển lại
        if (player != null && player.movement != null)
        {
            player.movement.enabled = true;
        }
    }

    private System.Collections.IEnumerator FlashWrongButton(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= optionButtons.Length) yield break;

        Button btn = optionButtons[buttonIndex];
        Image btnImage = btn.GetComponent<Image>();
        Color originalColor = btnImage.color;

        btnImage.color = Color.red;
        yield return new WaitForSecondsRealtime(0.3f); // Dùng Realtime vì timeScale = 0

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

