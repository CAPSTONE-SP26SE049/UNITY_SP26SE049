using System.Collections.Generic;
using UnityEngine;

public class SpeakingQuestionManager : MonoBehaviour
{
    public static SpeakingQuestionManager Instance { get; private set; }

    private List<SpeakingQuestionData> questions = new List<SpeakingQuestionData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeQuestions();
    }

    private void InitializeQuestions()
    {
        // 1. Đọc to từ: "nắng" - Phát âm N đầu từ
        questions.Add(new SpeakingQuestionData(
            "Đọc to từ: \"nắng\"",
            "nắng",
            "Phát âm N đầu từ"
        ));

        // 2. Đọc to từ: "lá" - Phát âm L đầu từ
        questions.Add(new SpeakingQuestionData(
            "Đọc to từ: \"lá\"",
            "lá",
            "Phát âm L đầu từ"
        ));

        // 3. Đọc câu: "Trời hôm nay có nhiều nắng." - Phân biệt: nắng (N)
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Trời hôm nay có nhiều nắng.\"",
            "Trời hôm nay có nhiều nắng.",
            "Phân biệt: nắng (N)"
        ));

        // 4. Đọc câu: "Chiếc lá rơi xuống đất." - Phân biệt: lá (L)
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Chiếc lá rơi xuống đất.\"",
            "Chiếc lá rơi xuống đất.",
            "Phân biệt: lá (L)"
        ));

        // 5. Đọc to từ: "nồi" - Phân biệt: n vs lồi
        questions.Add(new SpeakingQuestionData(
            "Đọc to từ: \"nồi\"",
            "nồi",
            "Phân biệt: n vs lồi"
        ));

        // 6. Đọc to từ: "lúa" - Phân biệt: l vs núa
        questions.Add(new SpeakingQuestionData(
            "Đọc to từ: \"lúa\"",
            "lúa",
            "Phân biệt: l vs núa"
        ));

        // 7. Đọc câu: "Mẹ đang nấu cơm trong nồi." - Phân biệt: nấu, nồi
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Mẹ đang nấu cơm trong nồi.\"",
            "Mẹ đang nấu cơm trong nồi.",
            "Phân biệt: nấu, nồi"
        ));

        // 8. Đọc câu: "Cánh đồng lúa rất xanh." - Phân biệt: lúa
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Cánh đồng lúa rất xanh.\"",
            "Cánh đồng lúa rất xanh.",
            "Phân biệt: lúa"
        ));

        // 9. Đọc to cặp từ: "nặng – lặng" - Nhấn mạnh sự khác nhau N / L
        questions.Add(new SpeakingQuestionData(
            "Đọc to cặp từ:\n\"nặng – lặng\"",
            "nặng lặng",
            "Nhấn mạnh sự khác nhau N / L"
        ));

        // 10. Đọc to cặp từ: "nón – lón" (chỉ từ nón đúng) - Nhận diện phát âm đúng
        questions.Add(new SpeakingQuestionData(
            "Đọc to cặp từ:\n\"nón – lón\" (chỉ từ nón đúng)",
            "nón",
            "Nhận diện phát âm đúng"
        ));

        // 11. Đọc câu: "Em đội nón khi ra ngoài." - Phân biệt: nón
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Em đội nón khi ra ngoài.\"",
            "Em đội nón khi ra ngoài.",
            "Phân biệt: nón"
        ));

        // 12. Đọc câu: "Con mèo đang nằm trên ghế." - Phân biệt: nằm
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Con mèo đang nằm trên ghế.\"",
            "Con mèo đang nằm trên ghế.",
            "Phân biệt: nằm"
        ));

        // 13. Đọc câu: "Bé Lan đang học bài." - Phân biệt: Lan (L)
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Bé Lan đang học bài.\"",
            "Bé Lan đang học bài.",
            "Phân biệt: Lan (L)"
        ));

        // 14. Đọc to cặp từ: "nói – lói" (chỉ nói đúng) - Nhận diện âm đầu
        questions.Add(new SpeakingQuestionData(
            "Đọc to cặp từ:\n\"nói – lói\" (chỉ nói đúng)",
            "nói",
            "Nhận diện âm đầu"
        ));

        // 15. Đọc câu: "Anh Nam nói chuyện rất nhỏ." - Phân biệt: Nam, nói
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Anh Nam nói chuyện rất nhỏ.\"",
            "Anh Nam nói chuyện rất nhỏ.",
            "Phân biệt: Nam, nói"
        ));

        // 16. Đọc câu: "Chiếc ly nằm trên bàn." - Phân biệt: nằm vs ly
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Chiếc ly nằm trên bàn.\"",
            "Chiếc ly nằm trên bàn.",
            "Phân biệt: nằm vs ly"
        ));

        // 17. Đọc to cặp từ: "nắng – lắng" - So sánh rõ N / L
        questions.Add(new SpeakingQuestionData(
            "Đọc to cặp từ:\n\"nắng – lắng\"",
            "nắng lắng",
            "So sánh rõ N / L"
        ));

        // 18. Đọc câu: "Em hãy lắng nghe thầy giáo." - Phân biệt: lắng
        questions.Add(new SpeakingQuestionData(
            "Đọc câu:\n\"Em hãy lắng nghe thầy giáo.\"",
            "Em hãy lắng nghe thầy giáo.",
            "Phân biệt: lắng"
        ));
    }

    public SpeakingQuestionData GetRandomQuestion()
    {
        if (questions.Count == 0)
        {
            InitializeQuestions();
        }
        
        int randomIndex = Random.Range(0, questions.Count);
        return questions[randomIndex];
    }
}

